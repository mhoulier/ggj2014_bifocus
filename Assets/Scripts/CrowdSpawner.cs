using UnityEngine;
using System.Collections;

public class CrowdSpawner : MonoBehaviour {

	// Set of available animations
	public string[] animationNames;
	// Delay on spawn.
	public float spawnDelay = 20f;

	[SerializeField]
	private float m_CrowdDestructDelay = 20.0f;

	// Crowd controller prefab
	public GameObject crowdControllerPrefab;

	// Crowd target
	private GameObject crowdTarget;

	private Animator crowdTargetAnimator;
	private bool firstUpdate = true;

	private GameHunt m_GameMode = null;


	// Use this for initialization
	void Start () {
		// get the crowd target instance
		crowdTarget = GameObject.FindGameObjectWithTag("crowdTarget");

		crowdTargetAnimator = crowdTarget.GetComponent<Animator>();

		m_GameMode = (GameHunt)FindObjectOfType(typeof(GameHunt));

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
			
			// Instantiate a crowd controller
			if (crowdControllerPrefab != null)
			{
				crowdCtrl = Instantiate(crowdControllerPrefab, transform.position, transform.rotation) as GameObject;
				
			}


			// Set a random animation for the crowd target
			int animIndex = Random.Range(0, animationNames.Length);

			Debug.Log("#### SPAWN ####");

			// start a crowd target animation
			crowdTargetAnimator.SetTrigger(animationNames[animIndex]);

			// Notify that there is a new wave
			m_GameMode.OnNewWave();



			// hack : wait for anim to start ?
			//yield return new WaitForSeconds(2.0f);

			// wait for the end of the animation
			//yield return WaitForAnimation( crowdTargetAnimator.animation );

			//yield return WaitForAnimatorState( crowdTargetAnimator, animationNames[animIndex] );

			//Debug.Log("Animation over");

			// Destroy the crowd
			if (crowdCtrl != null)
			{
				//DestroyObject(crowdCtrl, 20.0f);
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

	private IEnumerator WaitForAnimation ( Animation animation )
	{
		do
		{
			yield return null;
		} while ( animation.isPlaying );
	}

	private IEnumerator WaitForAnimatorState ( Animator animator, string animName )
	{
		do
		{
			Debug.Log("Still in " + animName);
			yield return null;
		} while ( animator.GetAnimatorTransitionInfo(0).IsName(animName) );
	}
}
