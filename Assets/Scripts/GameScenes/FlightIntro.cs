using UnityEngine;
using System.Collections;

public class FlightIntro : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private MenuManager m_MenuManager = null;
	
	private bool m_IsActiveSequence = false;

	private bool m_LevelTransitionRequested = false;

	void Start()
	{
		StartSequence();
	}
	
	void OnLevelWasLoaded(int _level)
	{
		//InitGameSequence();
	}
	
	private void StartSequence()
	{
		LevelManager levelManager = (LevelManager)FindObjectOfType( typeof(LevelManager) );
		if (levelManager == null && m_GameManagerPrefab != null)
		{
			GameObject gameManager = Instantiate(m_GameManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			levelManager = gameManager.GetComponent<LevelManager>();
		}

		MenuManager menuManager = (MenuManager)FindObjectOfType( typeof(MenuManager) );
		if (menuManager == null)
		{
			Debug.Log("GameManager prefab is missing a MenuManager component!");
		}
		
		m_LevelManager = levelManager;
		m_MenuManager = menuManager;

		m_IsActiveSequence = (m_LevelManager != null);
	}
	
	private void EndSequence()
	{
		m_IsActiveSequence = false;
		
		m_LevelManager.LoadNextLevel();
	}

	public void RequestLevelTransition()
	{
		m_LevelTransitionRequested = true;
	}

	private void ProcessLevelTransitionRequest()
	{
		if (m_LevelTransitionRequested)
		{
			EndSequence();
		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence && deltaTime > 0.0f)
		{
			UpdateInputs();
		}

		ProcessLevelTransitionRequest ();
	}

	private void UpdateInputs()
	{
		bool skipIntro = m_MenuManager.IsValidateMenuPressed();
		if (skipIntro)
		{
			RequestLevelTransition();
		}
	}
}
