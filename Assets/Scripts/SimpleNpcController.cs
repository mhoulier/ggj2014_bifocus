using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleNpcController : MonoBehaviour {

	// NPC prefab
	public GameObject prefab;

	public int civilianSize = 20;
	public int vilainSize = 1;

	// info
	public Vector3 flockCenter;
	public Vector3 flockVelocity;
	public Vector3 randomizeFactor = new Vector3(1.0f, 1.0f, 1.0f);

	private GameObject[] boids = null;

	private List<GameObject> m_BoidsToRemove = null;
	
	void Start()
	{
		boids = new GameObject[civilianSize + vilainSize];
		m_BoidsToRemove = new List<GameObject>(civilianSize + vilainSize);

		// create civilians
		for (var i=0; i<civilianSize + vilainSize; i++)
		{
			Vector3 position = new Vector3 (
				(Random.value * collider.bounds.size.x) * randomizeFactor.x,
				(Random.value * collider.bounds.size.y) * randomizeFactor.y,
				(Random.value * collider.bounds.size.z) * randomizeFactor.z
				) - collider.bounds.extents;

			GameObject boid = Instantiate(prefab, transform.position, transform.rotation) as GameObject;

			NpcController npcCtrl = boid.GetComponent<NpcController>();
			npcCtrl.SetBoidManager(this);

			if (i >= civilianSize)
			{
				// set as vilain
				npcCtrl.isVilain = true;
			}

			boid.transform.parent = transform;
			boid.transform.localPosition = position;
			boid.GetComponent<SimpleNpcFlocking>().SetController (gameObject);
			boids[i] = boid;

			//Debug.Log ("Boid " + i + " created");
		}
	}

	private int FindBoidIndex(GameObject _BoidToFind)
	{
		int foundIndex = -1;

		int boidCount = boids.Length;
		for (int boidIndex = 0; boidIndex < boidCount; ++boidIndex)
		{
			if ( boids[boidIndex] == _BoidToFind )
			{
				foundIndex = boidIndex;
				break;
			}
		}

		return foundIndex;
	}

	private void RemoveBoid(GameObject _BoidToRemove)
	{
		int boidCount = boids.Length;
		int boidIndex = FindBoidIndex(_BoidToRemove);
		if (0 <= boidIndex && boidIndex < boidCount)
		{
			Object.Destroy(_BoidToRemove);
			boids[boidIndex] = null;
		}
	}

	public void AddBoidToRemove(GameObject _BoidToRemove)
	{
		bool isPendingRemove = m_BoidsToRemove.Contains(_BoidToRemove);
		if (isPendingRemove == false)
		{
			m_BoidsToRemove.Add(_BoidToRemove);
		}
	}
	
	void Update ()
	{
		float deltaTime = Time.deltaTime;
		if (deltaTime > 0.0f)
		{
			UpdateBoids();
		}
		foreach (GameObject boidToRemove in m_BoidsToRemove)
		{
			RemoveBoid(boidToRemove);
		}
	}

	private void UpdateBoids()
	{
		Vector3 theCenter = Vector3.zero;
		Vector3 theVelocity = Vector3.zero;
		
		foreach (GameObject boid in boids)
		{
			if (boid != null)
			{
				theCenter = theCenter + boid.transform.localPosition;
				theVelocity = theVelocity + boid.rigidbody.velocity;
			}
		}
		
		flockCenter = theCenter/(civilianSize + vilainSize);
		flockVelocity = theVelocity/(civilianSize + vilainSize);
	}
}
