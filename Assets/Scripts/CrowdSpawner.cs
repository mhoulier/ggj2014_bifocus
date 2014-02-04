using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrowdSpawner : MonoBehaviour {

	// Set of available animations
	public string[] animationNames;
	// Delay on spawn.
	public float spawnDelay = 5.0f;

	[SerializeField]
	private float m_JoinArenaDelay = 10.0f;

	[SerializeField]
	private float m_PlayDelay = 60.0f;

	[SerializeField]
	private float m_CrowdDestructDelay = 20.0f;

	[SerializeField]
	private string[] m_arenaBoundsLayers;

	[SerializeField]
	private string[] m_arenaBoundsCollidersLayers;

	// Crowd controller prefab
	public GameObject crowdControllerPrefab;

	// Crowd target
	private GameObject crowdTarget;

	private Animator crowdTargetAnimator;
	private bool firstUpdate = true;

	private GameHunt m_GameMode = null;

	// Arena bounds (obstacles and colliders) 
	// they are disabled to let the NPCs enter/leave the arena
	private List<GameObject> m_arenaBounds = null;
	private List<GameObject> m_arenaBoundscolliders = null;


	// Use this for initialization
	void Start () {
		// get the crowd target instance
		crowdTarget = GameObject.FindGameObjectWithTag("crowdTarget");

		crowdTargetAnimator = crowdTarget.GetComponent<Animator>();

		m_GameMode = (GameHunt)FindObjectOfType(typeof(GameHunt));


		m_arenaBounds = new List<GameObject>();
		for (int i = 0; i < m_arenaBoundsLayers.Length; i++)
		{
			FindGameObjectsWithLayerName(ref m_arenaBounds, m_arenaBoundsLayers[i]);
		}

		m_arenaBoundscolliders = new List<GameObject>();
		for (int i = 0; i < m_arenaBoundsCollidersLayers.Length; i++)
		{
			FindGameObjectsWithLayerName(ref m_arenaBoundscolliders, m_arenaBoundsCollidersLayers[i]);
		}
	}

	void Update()
	{
		if (firstUpdate)
		{
			firstUpdate = false;
			// Start the first spawn.
			StartCoroutine(SpawnCrowd());
		}
	}


	public IEnumerator SpawnCrowd()
	{
		Debug.Log("#### WAIT TO SPAWN ####");

		// Wait for the spawn delay.
		yield return new WaitForSeconds(spawnDelay);

		if (animationNames.Length > 0)
		{

			GameObject crowdCtrl = null;
			SimpleNpcController npcManager = null;
			
			// Instantiate a crowd controller
			if (crowdControllerPrefab != null)
			{
				crowdCtrl = Instantiate(crowdControllerPrefab, transform.position, transform.rotation) as GameObject;
				
			}

			yield return new WaitForSeconds(1.0f);

			// Destroy the crowd
			if (crowdCtrl != null)
			{
				npcManager = crowdCtrl.GetComponent<SimpleNpcController>();

				// Join the arena
				Debug.Log("#### JOIN ARENA ####");
				EnableArenaBounds(false);
				// Join the arena
				npcManager.OnJoinArena();
				// Notify that there is a new wave
				m_GameMode.OnNewWave();
				yield return new WaitForSeconds(m_JoinArenaDelay);
				
				
				// Play
				Debug.Log("#### PLAY IN ARENA ####");
				EnableArenaBounds(true);
				// Set a random animation for the crowd target
				int animIndex = Random.Range(0, animationNames.Length);
				// start a crowd target animation
				crowdTargetAnimator.SetTrigger(animationNames[animIndex]);
				npcManager.OnPlay();
				yield return new WaitForSeconds(m_PlayDelay);

				// Leave the arena
				Debug.Log("#### LEAVE ARENA ####");
				EnableArenaBounds(false);
				npcManager.OnLeaveArena();
				yield return new WaitForSeconds(m_CrowdDestructDelay);

				Component[] npcs = crowdCtrl.GetComponentsInChildren<NpcController>();
				foreach (Component npc in npcs)
				{
					NpcController npcctrl = npc as NpcController;
					npcctrl.OnLeave();
				}

				// Check if the enemy is still alive
				DestroyObject(crowdCtrl);
			}
		}

		// Restart the coroutine to spawn another prop.
		StartCoroutine(SpawnCrowd());
	}

	private void EnableArenaBounds(bool enable)
	{
		foreach (GameObject go in m_arenaBounds)
		{
			go.SetActive(enable);
		}

		/*
		// lets the player cross !!
		foreach (GameObject go in m_arenaBoundscolliders)
		{
			go.collider.enabled = enable;

		}
		//*/

		int npcLayer = LayerMask.NameToLayer("npcLayer");
		for (int i = 0; i < m_arenaBoundsCollidersLayers.Length; i++)
		{
			Physics.IgnoreLayerCollision(npcLayer, LayerMask.NameToLayer(m_arenaBoundsCollidersLayers[i]) , !enable);
		}
	}

	private void FindGameObjectsWithLayer (ref List<GameObject> objects, int layer) 
	{
		GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		for (int i = 0; i < goArray.Length; i++) {
			if (goArray[i].layer == layer) {
				objects.Add(goArray[i]);
			}
		}
	}

	private void FindGameObjectsWithLayerName (ref List<GameObject> objects, string layer) 
	{
		FindGameObjectsWithLayer(ref objects, LayerMask.NameToLayer(layer));
	}


}
