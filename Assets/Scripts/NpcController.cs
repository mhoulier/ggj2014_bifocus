using UnityEngine;
using System.Collections;

public class NpcController : MonoBehaviour {

	public bool isVilain = false;

	public string layerWhenNotVisible = "Default";
	public string layerWhenSeenByBothAndEnemy = "Default";
	public string layerWhenSeenByBothAndCivilian = "Default";

	MeshRenderer [] graphics;
	GameObject [] graphicsGO;
	int childCount = 0;
	int maxChildren = 10;

	private SimpleNpcController m_BoidManager = null;
	public void SetBoidManager(SimpleNpcController _NpcManager)
	{
		m_BoidManager = _NpcManager;
	}

	private GameHunt m_GameMode = null;

	[SerializeField]
	private GameObject m_ParticleSystemPrefab = null;
//	private GameObject m_ParticleSystemInstance = null;


	// Use this for initialization
	void Start () {

		m_GameMode = (GameHunt)FindObjectOfType(typeof(GameHunt));

		graphics = new MeshRenderer[maxChildren];
		graphicsGO = new GameObject[maxChildren];

		/*
		childCount = 0;
		foreach (Transform t in transform)
		{
			if (child.enabled)
			{
				graphicsGO[childCount] = t.gameObject;
				graphics[childCount] = graphicsGO[childCount].GetComponent<MeshRenderer>();

				childCount++;
			}
		}//*/

		graphicsGO[childCount] = transform.Find("Chest2").gameObject;
		graphics[childCount] = graphicsGO[childCount].GetComponent<MeshRenderer>();
		childCount++;

		graphicsGO[childCount] = transform.Find("head").gameObject;
		graphics[childCount] = graphicsGO[childCount].GetComponent<MeshRenderer>();
		childCount++;

		graphicsGO[childCount] = transform.Find("legs").gameObject;
		graphics[childCount] = graphicsGO[childCount].GetComponent<MeshRenderer>();
		childCount++;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void UpdateVisibility(bool isVisibleByP1, bool isVisibleByP2)
	{
		if (!isVisibleByP1 || !isVisibleByP2)
		{
			for (int i = 0; i < childCount; i++)
			{
				graphicsGO[i].layer = LayerMask.NameToLayer(layerWhenNotVisible);

				// DEBUG : set to white
				graphics[i].material.color = new Color(1,1,1,1);
			}
		}
		else
		{
			// visible by both
			if (isVilain)
			{
				for (int i = 0; i < childCount; i++)
				{
					// Enemy
					graphicsGO[i].layer = LayerMask.NameToLayer(layerWhenSeenByBothAndEnemy);
					
					// DEBUG : set to white
					graphics[i].material.color = new Color(1,0,0,1);
				}
			}
			else
			{
				for (int i = 0; i < childCount; i++)
				{
					// civilian
					graphicsGO[i].layer = LayerMask.NameToLayer(layerWhenSeenByBothAndCivilian);
					
					// DEBUG : set to yelow
					graphics[i].material.color = new Color(1,1,0,1);
				}
			}
		}
	}

	// Called when a player is shot
	public void OnShoot()
	{
		// TODO : implement animation of death
		if (isVilain)
		{
			Debug.Log("### YOU KILLED A VILLAIN !!!! ###");
			m_GameMode.VillainKilled();
		}
		else
		{
			Debug.Log("### YOU KILLED A CIVILIAN !!!! ###");
			m_GameMode.CivilianKilled();
		}
		//m_ParticleSystemInstance = Instantiate(m_ParticleSystemPrefab, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
		Instantiate(m_ParticleSystemPrefab, gameObject.transform.position, gameObject.transform.rotation);

		m_BoidManager.AddBoidToRemove(gameObject);
	}

	public void OnLeave()
	{
		if (isVilain)
		{
			Debug.Log("### A VILLAIN ESCAPED!!!! ###");
			m_GameMode.VillainEscaped();
		}
		else
		{
			//Debug.Log("### A CIVILIAN IS SAFE!!!! ###");
		}
		m_BoidManager.AddBoidToRemove(gameObject);
	}
}
