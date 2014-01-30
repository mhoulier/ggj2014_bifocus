using UnityEngine;
using System.Collections;

public class SimpleNpcFlocking : MonoBehaviour {

	// max delta position for
//	private float m_MaxDeltaPosition = 5.0f;
	// Crowd target
	private GameObject m_CrowdTarget = null;
	// Simple NPC controller
	private GameObject m_NpcController = null;

	private Vector3 m_PreviousTargetPosition = Vector3.zero;
	private Vector3 m_VelocityFactor = Vector3.zero;

	private bool m_FirstUpdate = true;
	private bool m_WasStatic = true;

	// Use this for initialization
	void Start () {
		// get the crowd target instance
		m_CrowdTarget = GameObject.FindGameObjectWithTag("crowdTarget");

		//Debug.Log("TODO : handle NPC rotation !!!!!!");
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
	}

	void FixedUpdate()
	{
		float deltaTime = Time.fixedDeltaTime;
		if (m_CrowdTarget != null && deltaTime > 0.0f)
		{
			UpdateCrowdTarget(deltaTime);
		}
	}

	private void UpdateCrowdTarget(float _DeltaTime)
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

	public void SetController (GameObject theController)
	{
		m_NpcController = theController;
	}
}
