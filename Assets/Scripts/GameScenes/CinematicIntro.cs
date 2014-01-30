using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CinematicIntro : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	
	private bool m_IsActiveSequence = false;
	
	[System.Serializable]
	public class CinematicSlide
	{
		public Texture2D m_SlideTexture;
		public float m_SlideDurationInSeconds;
	}
	
	[SerializeField]
	private List<CinematicSlide> m_IntroSlideShow;
	
	[SerializeField]
	private string m_PreviousSlideButtonName = "";
	[SerializeField]
	private string m_NextSlideButtonName = "";
	
	private int m_CurrentSlideIndex = -1;
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
		
		m_LevelManager = levelManager;
		
		m_IsActiveSequence = (m_LevelManager != null);
		
		InitCamera();
		StartSlide(0);
	}
	
	private void EndSequence()
	{
		m_IsActiveSequence = false;
			
		m_LevelManager.LoadNextLevel();
	}
	
	private void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	private void StartSlide(int _SlideIndex)
	{
		CinematicSlide introSlide = m_IntroSlideShow[_SlideIndex];
		m_CurrentSlideIndex = _SlideIndex;
		m_CurrentSlideTimer = introSlide.m_SlideDurationInSeconds;
		m_CurrentSlideTexture = introSlide.m_SlideTexture;
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence && deltaTime > 0.0f)
		{
			UpdateSlideInputs(deltaTime);
			
			if (m_CurrentSlideTimer > 0.0f)
			{
				UpdateSlideShow(deltaTime);
			}
		}
	}
	
	private void UpdateSlideInputs(float _DeltaTime)
	{
		bool prevSlideButtonPressed = (m_PreviousSlideButtonName.Length > 0) && Input.GetButtonDown(m_PreviousSlideButtonName);
		bool nextSlideButtonPressed = (m_NextSlideButtonName.Length > 0) && Input.GetButtonDown(m_NextSlideButtonName);
		
		if (prevSlideButtonPressed && !nextSlideButtonPressed)
		{
			StartPreviousSlide();
		}
		else if (nextSlideButtonPressed && !prevSlideButtonPressed)
		{
			StartNextSlide();
		}
	}
	
	private void UpdateSlideShow(float _DeltaTime)
	{
		m_CurrentSlideTimer -= _DeltaTime;
		
		if (m_CurrentSlideTimer <= 0.0f)
		{
			m_CurrentSlideTimer = 0.0f;
			StartNextSlide();
		}
	}
	
	private void StartPreviousSlide()
	{
		int prevSlideIndex = m_CurrentSlideIndex-1;
		
		if (prevSlideIndex >= 0)
		{
			StartSlide(prevSlideIndex);
		}
	}
	
	private void StartNextSlide()
	{
		int slideCount = m_IntroSlideShow.Count;
		int nextSlideIndex = m_CurrentSlideIndex+1;
		
		if (nextSlideIndex < slideCount)
		{
			StartSlide(nextSlideIndex);
		}
		else
		{
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
