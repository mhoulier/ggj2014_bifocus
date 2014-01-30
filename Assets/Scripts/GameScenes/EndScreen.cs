using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndScreen : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GameManagerPrefab = null;
	private PlayerManager m_PlayerManager= null;
	private LevelManager m_LevelManager = null;
	
	private bool m_IsActiveSequence = false;

	[SerializeField]
	private int m_NextLevelIndex = -1;
	
	[System.Serializable]
	public class CinematicSlide
	{
		public Texture2D m_SlideTexture;
		public float m_SlideDurationInSeconds;
	}
	
	[SerializeField]
	private CinematicSlide m_WinScreen;

	[SerializeField]
	private CinematicSlide m_LoseScreen;

	private float m_CurrentSlideTimer = 0.0f;
	private Texture2D m_CurrentSlideTexture = null;
	
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

		PlayerManager playerManager = (PlayerManager)FindObjectOfType( typeof(PlayerManager) );
		if (playerManager == null)
		{
			Debug.Log("GameManager prefab is missing a PlayerManager component!");
		}
		
		m_LevelManager = levelManager;
		m_PlayerManager = playerManager;
		
		m_IsActiveSequence = (m_LevelManager != null);
		
		InitCamera();

		bool hasWon = (m_PlayerManager.GetEndGameStatus() == EndGameStatus.E_EndGameWon);
		StartSlide(hasWon);
	}
	
	private void EndSequence()
	{
		m_IsActiveSequence = false;
		
		m_LevelManager.LoadLevel(m_NextLevelIndex);
	}
	
	private void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	private void StartSlide(bool _WinScreen)
	{
		CinematicSlide endSlide = _WinScreen? m_WinScreen : m_LoseScreen;
		m_CurrentSlideTimer = endSlide.m_SlideDurationInSeconds;
		m_CurrentSlideTexture = endSlide.m_SlideTexture;
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence && deltaTime > 0.0f)
		{
			if (m_CurrentSlideTimer > 0.0f)
			{
				UpdateSlideShow(deltaTime);
			}
		}
	}
	
	private void UpdateSlideShow(float _DeltaTime)
	{
		m_CurrentSlideTimer -= _DeltaTime;
		
		if (m_CurrentSlideTimer <= 0.0f)
		{
			m_CurrentSlideTimer = 0.0f;
			EndSequence();
		}
	}
	
	void OnGUI()
	{
		if (m_IsActiveSequence)
		{
			GuiManager.GUIDrawTextureOnScreen(m_CurrentSlideTexture);
		}
	}
}
