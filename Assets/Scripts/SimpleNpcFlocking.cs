using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleNpcFlocking : MonoBehaviour {

	public NpcBehavior npcBehavior =  NpcBehavior.E_Wander;
	public bool evadePlayer = true;

	// TODO use classes to stucture vars !!!!
	public float maxVelocity = 5.0f;
	public float maxVelocityRandomRatio = 0.2f;
	public float maxForce = 20.0f;
	public bool debugDraw = true;
	public float debugRaySize = 3;
	public float arrivalSlowingRadius = 5;

	public float wanderCircleDistance = 8.0f;
	public float wanderCircleRadius = 7.0f;
	public float wanderRotationSpeed = 360.0f;

	public float avoidMaxAhead = 20.0f;
	public float avoidForce = 1000.0f;

	public float separationRadius = 1.5f;
	public float separationMaxForce = 2.0f;

	public float targetSightRadius = 10.0f;
	public float targetFieldOfView = 20.0f;

	public float followBehindDistance = 2.0f;

	public List<PlayerClass> evadePlayerTypes;

	// max delta position for
//	private float m_MaxDeltaPosition = 5.0f;
	// Crowd target
	private GameObject m_CrowdTarget = null;
	// Simple NPC controller
	private GameObject m_NpcController = null;

	// Crowd regroup position
	private GameObject m_CrowdRegroupPosition = null;
	// Crowd end position
	private GameObject m_CrowdEndPosition = null;

	private Vector3 m_PreviousTargetPosition = Vector3.zero;
	private Vector3 m_VelocityFactor = Vector3.zero;

	private bool m_FirstUpdate = true;
	private bool m_WasStatic = true;

	private Vector3 desired_velocity;
	//private Vector3 steering;
	private float wanderAngle = 0.0f;

	private PlayerManager m_playerManager = null;

	public enum NpcBehavior
	{
		E_JoinArena,
		E_Wander,
		E_PursuitTarget,
		E_SeekPreviousNpcOrWander,
		E_LeaveArena,
		E_SeekFirstNpcThatWanders,
	};



	// Use this for initialization
	void Start () {
		// get the crowd target instance
		m_CrowdTarget = GameObject.FindGameObjectWithTag("crowdTarget");
		// "crowdRegroupPoint"
		// "crowdEndPoint"
		m_CrowdRegroupPosition = GameObject.FindGameObjectWithTag("crowdRegroupPoint");
		m_CrowdEndPosition = GameObject.FindGameObjectWithTag("crowdEndPoint");

		m_playerManager = (PlayerManager)FindObjectOfType( typeof(PlayerManager) );

		// Add randommness to the max velocity
		maxVelocity += maxVelocity * maxVelocityRandomRatio * (Random.value*2 - 1);
	}
	
	// Update is called once per frame
	void Update () {
		if (m_FirstUpdate)
		{
			if (m_NpcController != null)
			{
				SimpleNpcController boidController = m_NpcController.GetComponent<SimpleNpcController>();

				m_VelocityFactor = new Vector3 (
					(0.5f + Random.value) * boidController.randomizeFactor.x,
					(0.5f + Random.value) * boidController.randomizeFactor.y,
					(0.5f + Random.value) * boidController.randomizeFactor.z
					);
			}
			else
			{
				// can happen for NPC put manually in the hierarchy
				m_VelocityFactor = new Vector3 (
					(0.5f + Random.value) * 1.0f,
					(0.5f + Random.value) * 1.0f,
					(0.5f + Random.value) * 1.0f
					);
			}

			m_FirstUpdate = false;
		}
		else
		{
		}

		// Look in velocity direction
		if (rigidbody.velocity.magnitude != 0.0f)
		{
			//Vector2 dir2D = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y);
			//dir2D.Normalize();
			//float angle = Mathf.Atan2(dir2D.y, dir2D.x);
			transform.LookAt(transform.position + rigidbody.velocity);
		}
	}

	void FixedUpdate()
	{
		float deltaTime = Time.fixedDeltaTime;
		if (m_CrowdTarget != null && deltaTime > 0.0f)
		{

			Vector3 targetDeltaPosition = m_CrowdTarget.transform.position - m_PreviousTargetPosition;

			// Compute the target velocity
			// some checks to prevent teleportation issues
			Vector3 targetVelocity = new Vector3();
			if (!m_WasStatic /*&& targetDeltaPosition.magnitude < maxDeltaPosition*/)
			{
				targetVelocity = targetDeltaPosition / deltaTime;
			}
			m_PreviousTargetPosition = m_CrowdTarget.transform.position;
			m_WasStatic = (targetDeltaPosition.magnitude == 0);

			Vector3 steering = new Vector3();
			Vector3 wanderSteering = new Vector3();
			Vector3 avoidSteering = new Vector3();
			Vector3 separationSteering = new Vector3();
			Vector3 stayOutSteering = new Vector3();

			bool separateFromOtherBoids = true;

			switch (npcBehavior)
			{
				case NpcBehavior.E_Wander:
					// wander
					wanderSteering = Wander(deltaTime);
					steering += wanderSteering;
					break;

				case NpcBehavior.E_PursuitTarget:
					{
						// chase the target
						steering += Pursuit(deltaTime, m_CrowdTarget.transform.position, targetVelocity);

						// Stay out of its way
						stayOutSteering =  StayOutOfTargetWay(deltaTime, m_CrowdTarget.transform.position, targetVelocity);
						steering += stayOutSteering;
					}
					break;

				case NpcBehavior.E_SeekPreviousNpcOrWander:
					{
						// get the position of the previous NPC
						SimpleNpcFlocking previousNpc = GetPreviousBoid();
						if (previousNpc != null)
						{
							Vector3 previousNpcPosition = previousNpc.transform.position;
							Vector3 previousNpcVelocity = previousNpc.rigidbody.velocity;

							// Purchase it
							steering += Follow(deltaTime, previousNpcPosition, previousNpcVelocity);

							// Stay out of its way
							stayOutSteering =  StayOutOfTargetWay(deltaTime, previousNpcPosition, previousNpcVelocity);
							steering += stayOutSteering;
						}
						else
						{
							// wander
							wanderSteering = Wander(deltaTime);
							steering += wanderSteering;
						}
					}
					break;

				case NpcBehavior.E_JoinArena:
					{
						// Go to the regroup position
						if (m_CrowdRegroupPosition != null)
							steering += Seek(deltaTime, m_CrowdRegroupPosition.transform.position);
						else
						{
							// Should not happen ! 
							// simply wander
							wanderSteering = Wander(deltaTime);
							steering += wanderSteering;
						}
					}
					break;

			case NpcBehavior.E_LeaveArena:
				{
					// Go to the end position
					if (m_CrowdEndPosition != null)
						steering += Seek(deltaTime, m_CrowdEndPosition.transform.position);
					else
					{
						// Should not happen ! 
						// simply wander
						wanderSteering = Wander(deltaTime);
						steering += wanderSteering;
					}
				}
				break;

			case NpcBehavior.E_SeekFirstNpcThatWanders:
				{
					// get the first boid
					SimpleNpcFlocking firstNpc = GetFirstBoid();
					if (firstNpc != null && firstNpc != this)
					{
						Vector3 firstNpcPosition = firstNpc.transform.position;
						Vector3 firstNpcVelocity = firstNpc.rigidbody.velocity;
						
						// Purchase it
						steering += Follow(deltaTime, firstNpcPosition, firstNpcVelocity);
						
						// Stay out of its way
						stayOutSteering =  StayOutOfTargetWay(deltaTime, firstNpcPosition, firstNpcVelocity);
						steering += stayOutSteering;
					}
					else
					{
						// wander
						wanderSteering = Wander(deltaTime);
						steering += wanderSteering;

						// The leader does not separate from the others
						separateFromOtherBoids = false;
					}
				}
				break;
			}

			// Avoid obstacles
			avoidSteering = Avoid(deltaTime);
			steering += avoidSteering;

			// Keep separation between the NPCs
			if (separateFromOtherBoids)
			{
				separationSteering = Separation(deltaTime);
				steering += separationSteering;
			}


			// evade from the player
			if (evadePlayer)
			{
				// get the player position
				List<Player> players = m_playerManager.GetPlayers();
				foreach (Player player in players)
				{
					// check if the player type is concerned
					if (evadePlayerTypes.Contains(player.GetPlayerClass()))
					{
						GameObject playerInstance =  player.GetPlayerInstance();
						if (playerInstance == null)
							continue;

						CharacterController character = playerInstance.GetComponent<CharacterController>();
						if (character == null)
							continue;

						// Evade
						//steering += Evade(deltaTime, playerInstance.transform.position, character.velocity);
						stayOutSteering = StayOutOfTargetWay(deltaTime, playerInstance.transform.position, playerInstance.transform.forward);
						steering += stayOutSteering;
					}
				}


			}


			//UpdateCrowdTargetSeek0_Naive(deltaTime);
			//steering += Seek(deltaTime, m_CrowdTarget.transform.position);
			//Vector3 steering = Flee(deltaTime, m_CrowdTarget.transform.position);
			//Vector3 steering = Arrive(deltaTime, m_CrowdTarget.transform.position);
			//wanderSteering = Wander(deltaTime);
			//steering += wanderSteering;


			//Vector3 steering = Evade(deltaTime, m_CrowdTarget.transform.position, targetVelocity);



			//rigidbody.velocity = dirToTarget * max_velocity;
			//rigidbody.AddForce(steering * _DeltaTime);
			if (steering.magnitude > maxForce)
			{
				steering.Normalize();
				steering *= maxForce;
			}
			
			steering = steering / rigidbody.mass;
			
			Vector3 velocity = rigidbody.velocity + steering;
			if (velocity.magnitude > maxVelocity)
			{
				velocity.Normalize();
				velocity *= maxVelocity;
			}
			rigidbody.velocity = velocity;


			if (debugDraw)
			{
				Vector3 dir = rigidbody.velocity;
				dir.Normalize();
				Debug.DrawLine(transform.position, transform.position + debugRaySize * dir, Color.green);

				if (wanderSteering != null)
				{
					dir = wanderSteering;
					dir.Normalize();
					Debug.DrawLine(transform.position, transform.position + debugRaySize *  dir, Color.black);
				}

				dir = steering;
				dir.Normalize();
				Debug.DrawLine(transform.position, transform.position + debugRaySize * dir, Color.red);
			
				if (avoidSteering != null)
				{
					dir = avoidSteering;
					dir.Normalize();
					Debug.DrawLine(transform.position, transform.position + debugRaySize * dir, Color.yellow);
				}

				dir = targetVelocity;
				dir.Normalize();
				Debug.DrawLine(m_CrowdTarget.transform.position, m_CrowdTarget.transform.position + debugRaySize * dir, Color.red);

				if (separationSteering != null)
				{
					dir = separationSteering;
					dir.Normalize();
					Debug.DrawLine(transform.position, transform.position + debugRaySize * dir, Color.cyan);
				}

				if (stayOutSteering != null)
				{
					dir = stayOutSteering;
					dir.Normalize();
					Debug.DrawLine(transform.position, transform.position + 3 * debugRaySize * dir, Color.magenta);
				}

			}
		}
	}


	private void UpdateCrowdTargetV1(float _DeltaTime)
	{
		Vector3 targetDeltaPosition = m_CrowdTarget.transform.position - m_PreviousTargetPosition;
		
		// some checks to prevent teleportation issues !!!
		if (!m_WasStatic /*&& targetDeltaPosition.magnitude < maxDeltaPosition*/)
		{
			Vector3 targetVelocity = targetDeltaPosition / _DeltaTime;
			
			// tweek velocity
			targetVelocity.x = targetVelocity.x * m_VelocityFactor.x;
			targetVelocity.y = targetVelocity.y * m_VelocityFactor.y;
			targetVelocity.z = targetVelocity.z * m_VelocityFactor.z;
			
			// Set velocity on the boid
			rigidbody.velocity = targetVelocity;
		}
		m_PreviousTargetPosition = m_CrowdTarget.transform.position;
		m_WasStatic = (targetDeltaPosition.magnitude == 0);
	}

	private void UpdateCrowdTargetSeek0_Naive(float _DeltaTime)
	{
		Vector3 dirToTarget = (m_CrowdTarget.transform.position - transform.position);
		dirToTarget.Normalize();
		rigidbody.velocity = dirToTarget * maxVelocity;
	}

	private Vector3 Seek(float _DeltaTime, Vector3 targetPosition)
	{
		// compute the deisred velocity : velocity in order to reach the target at max speed
		desired_velocity = (targetPosition - transform.position);
		desired_velocity.y = 0; // No vertical velocity (due to bad vertical positioning)
		desired_velocity.Normalize();
		desired_velocity *= maxVelocity;

		// compute the steering : force in order to change from current velocity to desired velocity 
		Vector3 steering = desired_velocity - rigidbody.velocity;

		return steering;
	}

	private Vector3 Arrive(float _DeltaTime, Vector3 targetPosition)
	{
		// compute the deisred velocity : velocity in order to reach the target at max speed
		desired_velocity = (targetPosition - transform.position);
		desired_velocity.y = 0; // No vertical velocity (due to bad vertical positioning)
		float distance = desired_velocity.magnitude;
		desired_velocity.Normalize();
		desired_velocity *= maxVelocity;

		// slow down in the radius
		if (distance < arrivalSlowingRadius)
		{
			desired_velocity *= (distance / arrivalSlowingRadius);
		}
		
		// compute the steering : force in order to change from current velocity to desired velocity 
		Vector3 steering = desired_velocity - rigidbody.velocity;

		return steering;
	}

	private Vector3 Flee(float _DeltaTime, Vector3 targetPosition)
	{
		// compute the deisred velocity : velocity in order to reach the target at max speed
		desired_velocity = (transform.position - targetPosition);
		desired_velocity.y = 0; // No vertical velocity (due to bad vertical positioning)
		desired_velocity.Normalize();
		desired_velocity *= maxVelocity;
		
		// compute the steering : force in order to change from current velocity to desired velocity 
		Vector3 steering = desired_velocity - rigidbody.velocity;
		
		return steering;
	}

	private Vector3 Wander(float _DeltaTime)
	{

		// Calculate the circle center
		Vector3 circleCenter = rigidbody.velocity;
		circleCenter.Normalize();
		circleCenter *= (wanderCircleDistance);

		// Calculate the displacement force
		Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), 0.0f, Mathf.Sin(wanderAngle));
		displacement *= wanderCircleRadius;

		// Change wanderAngle just a bit, so it
		// won't have the same value in the
		// next game frame.
		float wander_angleChange = (wanderRotationSpeed * Mathf.PI / 180.0f) * _DeltaTime;
		wanderAngle += Random.value * wander_angleChange - wander_angleChange * 0.5f;
		//
		// Finally calculate and return the wander force
		Vector3 wanderForce = circleCenter + displacement;
		desired_velocity = wanderForce;
		//Debug.Log(wanderForce.ToString());
		return wanderForce;
	}

	private Vector3 Pursuit(float _DeltaTime, Vector3 targetPosition, Vector3 targetVelocity)
	{
		Vector3 distance = targetPosition - transform.position;
		// compute the time ahead for prediction of future position
		// It decreases when target is closer
		float T = distance.magnitude / maxVelocity;

		Vector3 futurePosition = targetPosition + targetVelocity * T;

		return Seek(_DeltaTime, futurePosition);
	}

	private Vector3 Follow(float _DeltaTime, Vector3 targetPosition, Vector3 targetVelocity)
	{
		Vector3 dir = targetVelocity;
		dir.Normalize();
		Vector3 behindPosition = targetPosition - dir * followBehindDistance;
		
		return Arrive(_DeltaTime, behindPosition);
	}

	private Vector3 Evade(float _DeltaTime, Vector3 targetPosition, Vector3 targetVelocity)
	{
		Vector3 distance = targetPosition - transform.position;
		// compute the time ahead for prediction of future position
		// It decreases when target is closer
		float T = distance.magnitude / maxVelocity;
		
		Vector3 futurePosition = targetPosition + targetVelocity * T;
		
		return Flee(_DeltaTime, futurePosition);
	}

	private Vector3 Avoid(float _DeltaTime)
	{
		RaycastHit hit = new RaycastHit();
		bool hasHit = findMostThreateningObstacle(ref hit);
		if (!hasHit)
			return new Vector3();

		// Custom : use an avoid force that is the reflection of the ahead vector by the obstacle (normal)
		Vector3 rayDir = rigidbody.velocity;
		rayDir.y = 0;
		rayDir.Normalize();
		Vector3 reflect = Vector3.Reflect(rayDir, hit.normal);
		reflect.Normalize();
		reflect *= avoidForce;

		return reflect;
	}

	private Vector3 Separation(float _DeltaTime)
	{
		Vector3 force = new Vector3();


		if (m_NpcController != null)
		{
			SimpleNpcController boidController = m_NpcController.GetComponent<SimpleNpcController>();
			
			SimpleNpcFlocking [] boids = boidController.GetBoids();
			int neighborCount = 0;

			for (int i = 0; i < boids.Length; i++) 
			{
				SimpleNpcFlocking boid = boids[i];
				if (boid == null)
					continue;

				// if other boid is close, add a separation force
				if (boid != this && distance(boid, this) <= separationRadius) 
				{
					force.x += transform.position.x - boid.transform.position.x;
					force.z += transform.position.z - boid.transform.position.z;
					neighborCount++;
				}
			}

			// make an average of the forces
			if (neighborCount > 1) 
			{
				force.x /= neighborCount;
				force.z /= neighborCount;
			}


			force.Normalize();
			force *= separationMaxForce;
		}

		return force;
	}

	private Vector3 StayOutOfTargetWay(float _DeltaTime, Vector3 targetPosition, Vector3 targetVelocity)
	{
		Vector3 targetDir = targetVelocity;
		targetDir.Normalize();

		Vector3 targetToSelf = transform.position - targetPosition;
		if (targetToSelf.magnitude > targetSightRadius)
		{
			// Too far to be in target way
			return new Vector3();
		}
		targetToSelf.Normalize();

		// Self is in the target way if it is in its field of view
		float maxCosAngle = Mathf.Cos (targetFieldOfView); 
		if (Vector3.Dot(targetDir, targetToSelf) >= maxCosAngle)
		{
			// we are in the target way : try to evade 
			return Evade(_DeltaTime, targetPosition, targetVelocity);
		}
		else
		{
			// Nothing to do
			return new Vector3();
		}
	}

	private bool findMostThreateningObstacle(ref RaycastHit hit)
	{
		//RaycastHit hit = new RaycastHit();
		int obstacleLayer = LayerMask.NameToLayer("NpcObstacleLayer");
		int obstacleLayerMask = 1 << obstacleLayer;
		
		Vector3 rayStart = transform.position;
		Vector3 rayDir = rigidbody.velocity;
		rayDir.y = 0;
		rayDir.Normalize();

		// reduce ahead check distance when speed is low
		float aheadMax = avoidMaxAhead * (rigidbody.velocity.magnitude / maxVelocity);

		bool res =  Physics.Raycast(rayStart, rayDir, out hit, aheadMax, obstacleLayerMask);

		if (res && debugDraw)
		{
			Debug.DrawLine(rayStart, rayStart + avoidMaxAhead * rayDir, Color.blue);
		}

		return res;
	}

	private float distance(SimpleNpcFlocking b1, SimpleNpcFlocking b2)
	{
		return (b1.transform.position - b2.transform.position).magnitude;
	}

	public void SetController (GameObject theController)
	{
		m_NpcController = theController;
	}

	private SimpleNpcFlocking GetPreviousBoid()
	{
		if (m_NpcController == null)
			return null;


		SimpleNpcController boidController = m_NpcController.GetComponent<SimpleNpcController>();
		SimpleNpcFlocking previous = null;

		SimpleNpcFlocking [] boids = boidController.GetBoids();
			
		for (int i = 0; i < boids.Length; i++) 
		{
			SimpleNpcFlocking boid = boids[i];
			if (boid == null)
				continue;
				
			if (boid == this) 
			{
				return previous;
			}

			previous = boid;
		}
		
		return null;
	}

	private SimpleNpcFlocking GetFirstBoid()
	{
		if (m_NpcController == null)
			return null;

		SimpleNpcController boidController = m_NpcController.GetComponent<SimpleNpcController>();
		SimpleNpcFlocking [] boids = boidController.GetBoids();

		for (int i = 0; i < boids.Length; i++) 
		{
			SimpleNpcFlocking boid = boids[i];
			if (boid == null)
				continue;
			
			return boid;
		}
		
		return null;
	}
}
