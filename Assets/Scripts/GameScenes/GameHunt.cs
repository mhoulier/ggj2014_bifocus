#define DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
//Local players in a normal playthrough would be added in the menu scene

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState { E_GameLoading, E_GameInitializing, E_GameWaitingForPlayers, E_GamePreparingForStart, E_GamePlaying, E_GamePaused, E_GameEnding, };

public enum PlayerClass { E_ClassNone, E_ClassOnGround, E_ClassHighUp };

public enum EndGameStatus { E_EndGameNone, E_EndGameWon, E_EndGameLost };

public class GameHunt : MonoBehaviour
{

	public static string GetPlayerClassName(PlayerClass _PlayerClass)
	{
		string playerClassName = "";
		
		switch(_PlayerClass)
		{
		case PlayerClass.E_ClassOnGround:
			playerClassName = "Mr Trigger";
			break;
		case PlayerClass.E_ClassHighUp:
			playerClassName = "Mr LookOut";
			break;
		}
		
		return playerClassName;
	}

	public static PlayerClass GetNextPlayerClass(PlayerClass _currentPlayerClass)
	{
		PlayerClass nextPlayerClass = (_currentPlayerClass == PlayerClass.E_ClassOnGround) ? PlayerClass.E_ClassHighUp : PlayerClass.E_ClassOnGround;
		return nextPlayerClass;
	}

	private GameState m_GameState = GameState.E_GameLoading;
	private void ChangeGameState(GameState _NewGameState) { m_GameState = _NewGameState; }
	
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private InputManager m_InputManager = null;
	private MenuManager m_MenuManager = null;
	private PlayerManager m_PlayerManager = null;
	private TimeManager m_TimeManager = null;
	
	private bool m_IsActiveSequence = false;
	private bool IsSequenceActive() { return m_IsActiveSequence; }

	[SerializeField]
	private int m_NextLevelIndex = -1;

#if DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
	[SerializeField]
	private InputDevice m_DebugOnGroundPlayerInput = InputDevice.E_InputDeviceMouse;
	[SerializeField]
	private InputDevice m_DebugHighUpPlayerInput = InputDevice.E_InputDeviceKeyboard;
#endif

	// Use this for initialization
	void Start()
	{
		StartSequence();
		
		bool isSequenceActive = IsSequenceActive();
		if (isSequenceActive)
		{
			//InitGameAuthority();

#if DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
			//adding players when starting the scene without going through menu
			List<Player> players = m_PlayerManager.GetPlayers();
			if (players.Count == 0)
			{
				PlayerSpawner onGroundSpawner = PlayerSpawner.FindPlayerSpawner(PlayerClass.E_ClassOnGround);
				PlayerSpawner highUpSpawner = PlayerSpawner.FindPlayerSpawner(PlayerClass.E_ClassHighUp);
				if (onGroundSpawner != null && highUpSpawner != null)
				{
					m_PlayerManager.AddLocalPlayerToJoin(m_DebugOnGroundPlayerInput);
					m_PlayerManager.AddLocalPlayerToJoin(m_DebugHighUpPlayerInput);
					
					players = m_PlayerManager.GetPlayers();
					players[0].SetPlayerClass(PlayerClass.E_ClassOnGround);
					players[1].SetPlayerClass(PlayerClass.E_ClassHighUp);
				}
			}
#endif
			
			WaitForPlayers();
		}
	}

	void OnLevelWasLoaded(int _level)
	{
		Debug.Log("Game Chase Sequence: Level is loaded");
		
		LoadingFinished();
	}

	private void StartSequence()
	{
		Debug.Log("Starting Game Chase Sequence");
		
		LevelManager levelManager = (LevelManager)FindObjectOfType( typeof(LevelManager) );
		if (levelManager == null && m_GameManagerPrefab != null)
		{
			GameObject gameManager = Instantiate(m_GameManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			levelManager = gameManager.GetComponent<LevelManager>();
			
			Debug.Log("Instantiating gameManager!");
		}
		
		InputManager inputManager = (InputManager)FindObjectOfType( typeof(InputManager) );
		if (inputManager == null)
		{
			Debug.Log("GameManager prefab is missing a InputManager component!");
		}
		else
		{
			InputDevice[] supportedDevices = inputManager.GetSupportedInputDevices();
			if (supportedDevices.Length == 0)
			{
				Debug.Log("InputManager needs at least one supported device to work!");
				inputManager = null;
			}
		}

		MenuManager menuManager = (MenuManager)FindObjectOfType( typeof(MenuManager) );
		if (menuManager == null)
		{
			Debug.Log("GameManager prefab is missing a MenuManager component!");
		}
		
		PlayerManager playerManager = (PlayerManager)FindObjectOfType( typeof(PlayerManager) );
		if (playerManager == null)
		{
			Debug.Log("GameManager prefab is missing a PlayerManager component!");
		}

		TimeManager timeManager = (TimeManager)FindObjectOfType( typeof(TimeManager) );
		if (timeManager == null)
		{
			Debug.Log("GameManager prefab is missing a TimeManager component!");
		}

		m_LevelManager = levelManager;
		m_InputManager = inputManager;
		m_MenuManager = menuManager;
		m_PlayerManager = playerManager;
		m_TimeManager = timeManager;
		
		m_IsActiveSequence = (m_LevelManager != null) && (m_InputManager != null) && (m_MenuManager != null) && (m_PlayerManager != null);
		
		InitCamera();
	}

	void InitCamera()
	{

	}

	private void UpdateCamera()
	{
		List<Player> players = m_PlayerManager.GetPlayers ();

		Object[] playerCameras = FindObjectsOfType(typeof(PlayerSupportCamera));
		foreach ( Object camera in playerCameras )
		{
			PlayerSupportCamera playerCamera = camera as PlayerSupportCamera;

			foreach (Player player in players)
			{
				PlayerClass playerClass = player.GetPlayerClass();
				GameObject playerInstance = player.GetPlayerInstance();

				bool matchingCamera = playerCamera.IsPlayerSupported(playerClass);
				if (matchingCamera && playerInstance != null)
				{
					if (playerClass == PlayerClass.E_ClassOnGround)
					{
						OnGroundCamera onGroundCamera = playerCamera.gameObject.GetComponent<OnGroundCamera>();

						PlayerViewCone viewCone = playerInstance.GetComponentInChildren<PlayerViewCone>();
						Transform viewConeOrigin = viewCone.GetOriginTransform();

						Vector3 cameraOriginOffset = playerCamera.GetCameraOriginOffset();

						onGroundCamera.SetFollowOrigin(viewConeOrigin, cameraOriginOffset);
					}
					else if (playerClass == PlayerClass.E_ClassHighUp)
					{
						HighUpCamera highUpCamera = playerCamera.gameObject.GetComponent<HighUpCamera>();
						highUpCamera.SetFollowTarget(playerInstance.transform);
					}
					break;
				}
			}
		}
	}

	void EndSequence()
	{
		m_IsActiveSequence = false;
		
		m_LevelManager.LoadLevel(m_NextLevelIndex);
	}

	private void LoadingFinished()
	{
		ChangeGameState(GameState.E_GameInitializing);
	}
	
	private void WaitForPlayers()
	{
		ChangeGameState(GameState.E_GameWaitingForPlayers);
		
		//LocalPlayersAreWaiting();
	}

	private void UpdateGameWaitingForPlayers(float _DeltaTime)
	{
		PrepareForStart();
	}

	private void PrepareForStart()
	{
		ChangeGameState(GameState.E_GamePreparingForStart);

		List<Player> waitingPlayers = m_PlayerManager.GetPlayers ();
		SpawnPlayers(waitingPlayers);
		
		//@TODO spawn AIs / NPCs
	}

	private void SpawnPlayers(List<Player> _Players)
	{
		foreach (Player player in _Players)
		{
			PlayerClass playerClass = player.GetPlayerClass();
			
			PlayerSpawner spawner = PlayerSpawner.FindPlayerSpawner(playerClass);
			if (spawner != null)
			{
				Transform spawnTransform = spawner.gameObject.transform;
				SpawnPlayer(player, playerClass, spawnTransform.position, spawnTransform.rotation);

				Debug.Log("Spawned player " + m_InputManager.GetInputDeviceName(player.GetPlayerInput()) + " with player class " + playerClass.ToString());
			}
			else
			{
				Debug.Log("Failed to spawn player " + m_InputManager.GetInputDeviceName(player.GetPlayerInput()) + ": coudln't find spawner for player class " + playerClass.ToString());	
			}
		}
	}

	public GameObject m_OnGroundPlayerPrefab = null;
	public GameObject m_HighUpPlayerPrefab = null;

	public GameObject SpawnPlayer(Player _Player, PlayerClass _PlayerClass, Vector3 _Position, Quaternion _Rotation)
	{
		bool spawnPlayerOnGround = (_PlayerClass == PlayerClass.E_ClassOnGround);
		GameObject playerPrefab = spawnPlayerOnGround? m_OnGroundPlayerPrefab : m_HighUpPlayerPrefab;
		GameObject playerInstance = Instantiate(playerPrefab, _Position, _Rotation) as GameObject;

		InputDevice playerInput = _Player.GetPlayerInput();
		if (spawnPlayerOnGround)
		{
			OnGroundController playerController = playerInstance.GetComponent<OnGroundController>();
			playerController.SetControllerInput(playerInput);
		}
		else
		{
			HighUpController playerController = playerInstance.GetComponent<HighUpController>();
			playerController.SetControllerInput(playerInput);
		}

		_Player.SetPlayerInstance(playerInstance);
		
		return playerInstance;
	}

	private void UpdateGamePreparingForStart(float _DeltaTime)
	{
		StartGame();
	}
	
	private void StartGame()
	{
		ChangeGameState(GameState.E_GamePlaying);

		Debug.Log("StartGame");
		
		UpdateCamera();
	}

	private int m_CivilianKilled = 0;
	private int m_VillainKilled = 0;
	private int m_VillainEscaped = 0;

	[SerializeField]
	private int m_VictoryCondition_VillainKilledCount = 3;

	[SerializeField]
	private int m_LossCondition_MistakeCount = 3;

	private int m_CurrentMistakeCount = 0;
	
	public void CivilianKilled()
	{
		if (m_GameState == GameState.E_GamePlaying)
		{
			++m_CivilianKilled;
			++m_CurrentMistakeCount;
		}	
	}

	public void VillainKilled()
	{
		if (m_GameState == GameState.E_GamePlaying) {
				++m_VillainKilled;
		}
	}

	public void VillainEscaped()
	{
			if (m_GameState == GameState.E_GamePlaying) {
					++m_VillainEscaped;
					++m_CurrentMistakeCount;
			}
	}
	
	private void UpdateGamePlaying(float _DeltaTime)
	{
		bool pauseGame = CheckPauseSwitchRequest();
		if (pauseGame)
		{
			PauseGame();
		}
		else
		{
			EndGameStatus endStatus = CheckEndGameStatus();
			bool endGame = (endStatus != EndGameStatus.E_EndGameNone);
			if (endGame)
			{
				EndGame(endStatus);
			}
		}
	}

	private EndGameStatus CheckEndGameStatus()
	{
		EndGameStatus endStatus = EndGameStatus.E_EndGameNone;
		if (m_CurrentMistakeCount >= m_LossCondition_MistakeCount)
		{
			Debug.Log("### YOU LOST !!!! ###");
			endStatus = EndGameStatus.E_EndGameLost;
		}
		else if (m_VillainKilled >= m_VictoryCondition_VillainKilledCount)
		{
			Debug.Log("### YOU WIN !!!! ###");
			endStatus = EndGameStatus.E_EndGameWon;
		}

		return endStatus;
	}

	private bool CheckPauseSwitchRequest()
	{
		bool pauseGame = false;

		List<Player> players = m_PlayerManager.GetPlayers();
		foreach (Player player in players)
		{
			InputDevice playerInput = player.GetPlayerInput();
			bool playerRequestedPause = m_MenuManager.IsPauseGamePressed(playerInput);
			if (playerRequestedPause)
			{
				pauseGame = true;
				break;
			}
		}

		return pauseGame;
	}

	private void PauseGame()
	{
		ChangeGameState(GameState.E_GamePaused);
		
		Debug.Log("PauseGame");

		m_TimeManager.SetTimeScale(0.0f);
	}

	private void UnpauseGame()
	{
		ChangeGameState(GameState.E_GamePlaying);
		
		Debug.Log("UnpauseGame");

		m_TimeManager.SetTimeScale(1.0f);
	}

	private void UpdateGamePaused(float _DeltaTime)
	{
		bool unpauseGame = CheckPauseSwitchRequest();
		if (unpauseGame)
		{
			UnpauseGame();
		}
	}
	
	private void EndGame(EndGameStatus _EndGameStatus)
	{
		ChangeGameState(GameState.E_GameEnding);
		
		Debug.Log("EndGame");

		m_PlayerManager.SetEndGameStatus(_EndGameStatus);

		m_TimeBeforeEndSequence = m_EndingWaitTime;
	}

	[SerializeField]
	private float m_EndingWaitTime = 10.0f;
	private float m_TimeBeforeEndSequence = 0.0f;

	void UpdateGameEnding(float _DeltaTime)
	{
		m_TimeBeforeEndSequence -= _DeltaTime;

		if (m_TimeBeforeEndSequence <= 0.0f)
		{
			EndSequence();
		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		bool isSequenceActive = IsSequenceActive();
		if (isSequenceActive)
		{
			switch (m_GameState)
			{
			case GameState.E_GameLoading:
				break;
			case GameState.E_GameWaitingForPlayers:
				UpdateGameWaitingForPlayers(deltaTime);
				break;
			case GameState.E_GamePreparingForStart:
				UpdateGamePreparingForStart(deltaTime);
				break;
			case GameState.E_GamePlaying:
				UpdateGamePlaying(deltaTime);
				break;
			case GameState.E_GamePaused:
				UpdateGamePaused(deltaTime);
				break;
			case GameState.E_GameEnding:
				UpdateGameEnding(deltaTime);
				break;
			}
		}
	}

	void OnGUI()
	{
		if (m_GameState == GameState.E_GamePaused)
		{
			float screenWidth = Screen.width;
			float screenHeight = Screen.height;
			float labelWidth = 50;
			float labelHeight = 30;
			GUI.Label (new Rect( (screenWidth - labelWidth)/2, (screenWidth - screenHeight)/2, labelWidth, labelHeight), "Pause");
		}
	}

	public void OnNewWave()
	{
		Debug.Log("#### ON NEW WAVE ####");

		// Notify all the NewWaveSignal objects
		Object[] signals = FindObjectsOfType(typeof(NewWaveSignal));
		foreach ( Object signal in signals )
		{
			NewWaveSignal objectToNotify = signal as NewWaveSignal;
			objectToNotify.OnNewWave();
		}
	}
}
