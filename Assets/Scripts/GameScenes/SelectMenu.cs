#define DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public class SelectMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private NetworkManager m_NetworkManager = null;
	private InputManager m_InputManager = null;
	private MenuManager m_MenuManager = null;
	private PlayerManager m_PlayerManager = null;
	
	private bool m_IsActiveSequence = false;
	private bool IsSequenceActive() { return m_IsActiveSequence; }
	
	private int m_NextLevelIndex = -1;
	
	[SerializeField]
	private int m_GameSceneIndex = 0;

	[SerializeField]
	private int m_MainMenuSceneIndex = 0;

	[SerializeField]
	private Texture2D m_BackgroundImage = null;

	//private MenuState m_MenuState = MenuState.E_MenuNone;
	//private void ChangeMenuState(MenuState _NewMenuState) { m_MenuState = _NewMenuState; }
	
	private bool m_enableUI = false;
	private bool IsUIEnable() { return m_enableUI; }
	private void EnableUI() { m_enableUI = true; }
	private void DisableUI(){ m_enableUI = false; }
	
	private int m_CurrentPanelIndex = 0;	//TODO: move current panel index to menu panel container
	private int m_FocusOnButtonIndex = -1;
	
	private float m_MenuInputVerticalAxisWaitTime = 0.0f;
	//private bool m_InputVerticalDown = false;
	//private bool m_InputVerticalUp = false;

	private InputDevice m_MenuControllerInputDevice = InputDevice.E_InputDeviceNone;
	private InputDevice GetMenuControllerInputDevice(){ return m_MenuControllerInputDevice; }
	private void SetMenuControllerInputDevice(InputDevice _InputDevice)
	{
		bool isMouseAndKeyboard = m_InputManager.IsInputMouseAndKeyboard(_InputDevice);
		m_MenuControllerInputDevice = isMouseAndKeyboard ? m_InputManager.GetIncludedKeyboardInput(_InputDevice) : _InputDevice;
	}
	
	private int m_PlayerCount = 0;
	private int GetLocalPlayerCount(){ return m_PlayerCount; }
	
	private InputDevice[] m_PlayersInputDevice = null;
	
	private InputDevice GetLocalPlayerInputDevice(int _PlayerIndex)
	{
		bool validPlayerIndex = (0 <= _PlayerIndex && _PlayerIndex < GetLocalPlayerCount());
		InputDevice playerDevice = validPlayerIndex? m_PlayersInputDevice[_PlayerIndex] : InputDevice.E_InputDeviceNone;
		
		return playerDevice;
	}
	private bool SetLocalPlayerInputDevice(int _PlayerIndex, InputDevice _InputDevice)
	{
		bool validPlayerIndex = (0 <= _PlayerIndex && _PlayerIndex < GetLocalPlayerCount());
		if (validPlayerIndex)
		{
			m_PlayersInputDevice[_PlayerIndex] = _InputDevice;
		}
		
		return validPlayerIndex;
	}
	private InputDevice GetNextAvailableInputDevice(InputDevice[] _SupportedInputDevices, InputDevice _CurrentInput)
	{
		InputDevice availableInput = InputDevice.E_InputDeviceNone;
		int inputCount = _SupportedInputDevices.Length;

		int currentInputIndex = -1;
		for (int inputIndex = 0; inputIndex < inputCount; ++inputIndex)
		{
			InputDevice supportedInput = _SupportedInputDevices [inputIndex];
			bool isCurrentInput = (supportedInput == _CurrentInput);
			if (isCurrentInput)
			{
				currentInputIndex = inputIndex;
				break;
			}
		}

		for (int inputIndex = currentInputIndex+1; inputIndex < inputCount; ++inputIndex)
		{
			InputDevice supportedInput = _SupportedInputDevices [inputIndex];

			bool isAvailable = true;
			foreach (InputDevice playerInput in m_PlayersInputDevice)
			{
				bool areInputConflicting = m_InputManager.AreInputConflicting(supportedInput, playerInput);
				if (areInputConflicting)
				{
					isAvailable = false;
					break;
				}
			}
			
			if (isAvailable)
			{
				availableInput = supportedInput;
				break;
			}
		}

		bool foundValidInput = m_InputManager.IsInputDeviceValid (availableInput);
		if (foundValidInput == false)
		{
			for (int inputIndex = 0; inputIndex < currentInputIndex; ++inputIndex)
			{
				InputDevice supportedInput = _SupportedInputDevices [inputIndex];
				bool isAvailable = true;
				foreach (InputDevice playerInput in m_PlayersInputDevice)
				{
					bool areInputConflicting = m_InputManager.AreInputConflicting(supportedInput, playerInput);
					if (areInputConflicting)
					{
						isAvailable = false;
						break;
					}
				}
				
				if (isAvailable)
				{
					availableInput = supportedInput;
					break;
				}
			}
		}
		
		return availableInput;
	}
	
	private PlayerClass[] m_PlayersClass = null;
	private PlayerClass GetLocalPlayerClass(int _PlayerIndex)
	{
		bool validPlayerIndex = (0 <= _PlayerIndex && _PlayerIndex < GetLocalPlayerCount());
		PlayerClass playerClass = validPlayerIndex? m_PlayersClass[_PlayerIndex] : PlayerClass.E_ClassNone;
		
		return playerClass;
	}
	private bool SetLocalPlayerClass(int _PlayerIndex, PlayerClass _PlayerClass)
	{
		bool validPlayerIndex = (0 <= _PlayerIndex && _PlayerIndex < GetLocalPlayerCount());
		if (validPlayerIndex)
		{
			m_PlayersClass[_PlayerIndex] = _PlayerClass;
		}
		
		return validPlayerIndex;
	}

	private MenuButton[] m_MenuButtons = null;
	
	void OnLevelWasLoaded(int _level)
	{
		//InitGameSequence();
	}
	
	void Start()
	{
		StartSequence();
		
		bool isSequenceActive = IsSequenceActive();
		if (isSequenceActive)
		{
			InitPlayers();

			InitMenu();
			
			EnableUI();
			
			//ChangeMenuState(MenuState.E_MenuMainPanel);
		}
	}
	
	void StartSequence()
	{
		LevelManager levelManager = (LevelManager)FindObjectOfType( typeof(LevelManager) );
		if (levelManager == null && m_GameManagerPrefab != null)
		{
			GameObject gameManager = Instantiate(m_GameManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			levelManager = gameManager.GetComponent<LevelManager>();
			
			Debug.Log("Instantiating gameManager!");
		}
		
		NetworkManager networkManager = (NetworkManager)FindObjectOfType( typeof(NetworkManager) );
		if (networkManager == null)
		{
			Debug.Log("GameManager prefab is missing a NetworkManager component!");
		}
		
		InputManager inputManager = (InputManager)FindObjectOfType( typeof(InputManager) );
		if (inputManager == null)
		{
			Debug.Log("GameManager prefab is missing a InputManager component!");
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
		
		m_LevelManager = levelManager;
		m_NetworkManager = networkManager;
		m_InputManager = inputManager;
		m_MenuManager = menuManager;
		m_PlayerManager = playerManager;
		
		m_IsActiveSequence = (m_LevelManager != null) && (m_InputManager != null) && (m_MenuManager != null) && (m_PlayerManager != null);
		
		InitCamera();
	}
	
	private void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	private void InitPlayers()
	{
		List<Player> players = m_PlayerManager.GetPlayers();
		int playerCount = players.Count;

#if DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
		//adding players when starting the scene without going through main menu
		bool addLocalPlayers = (players.Count == 0);
		if (addLocalPlayers)
		{
			NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
			bool isAuthority = true;

			m_PlayerManager.AddLocalPlayerToJoin(localNetClient, InputDevice.E_InputDeviceNone, isAuthority);
			m_PlayerManager.AddLocalPlayerToJoin(localNetClient, InputDevice.E_InputDeviceNone, isAuthority);

			players = m_PlayerManager.GetPlayers();
			playerCount = players.Count;
		}
#endif

		m_PlayersInputDevice = new InputDevice[playerCount];
		m_PlayersClass = new PlayerClass[playerCount];
		m_PlayerCount = playerCount;

#if DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
		if (addLocalPlayers)
		{
			InputDevice[] supportedDevices = m_InputManager.GetSupportedInputDevices();
			if (supportedDevices.Length >= 2)
			{
				InputDevice inputPlayer1 = supportedDevices[0];
				SetLocalPlayerInputDevice(0, inputPlayer1);

				InputDevice inputPlayer2 = GetNextAvailableInputDevice(supportedDevices, inputPlayer1);
				
				players[0].SetPlayerInput(inputPlayer1);
				players[1].SetPlayerInput(inputPlayer2);
			}
		}
#endif

		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			Player player = players[playerIndex];
			InputDevice playerInput = player.GetPlayerInput();
			SetLocalPlayerInputDevice(playerIndex, playerInput);

			PlayerClass playerClass = (playerIndex%2 == 0)? PlayerClass.E_ClassOnGround : PlayerClass.E_ClassHighUp;
			SetLocalPlayerClass(playerIndex, playerClass);
		}
	}

	private void InitMenu()
	{
		InputDevice menuControllerInput = GetLocalPlayerInputDevice(0);
		SetMenuControllerInputDevice(menuControllerInput);

		int playerCount = GetLocalPlayerCount();

		//@HACK: this should be data driven, specified from Unity editor
		int playerSelectButtonCount = 2;
		
		int menuButtonCount = playerCount * playerSelectButtonCount + 2;
		MenuButton[] menu = new MenuButton[menuButtonCount];

		menu[menuButtonCount - 2] = new MenuButton("StartButton", "Start", StartButtonPressed);
		menu[menuButtonCount - 1] = new MenuButton("BackButton", "Back", BackButtonPressed);
		
		m_MenuButtons = menu;

		UpdateMenuLabels();
	}
	
	private string GetPlayerSelectInputButtonName(int _PlayerIndex)
	{
		string buttonName = string.Format("Player {0} Select Input Button", _PlayerIndex + 1);
		return buttonName;
	}
	
	private string GetPlayerClassButtonName(int _PlayerIndex)
	{
		string buttonName = string.Format("Player {0} Select Class Button", _PlayerIndex + 1);
		return buttonName;
	}
	
	private void UpdateMenuLabels()
	{
		//@HACK: this should be data driven, specified from Unity editor
		
		int playerCount = GetLocalPlayerCount();
		int playerSelectButtonCount = 2;
		
		MenuButton[] menu = m_MenuButtons;
		
		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			InputDevice playerInput = GetLocalPlayerInputDevice(playerIndex);
			PlayerClass playerClass = GetLocalPlayerClass(playerIndex);
			
			string playerSelectInputButton = GetPlayerSelectInputButtonName(playerIndex);
			string playerSelectClassButton = GetPlayerClassButtonName(playerIndex);
			
			int selectInputButtonIndex = playerSelectButtonCount * playerIndex;
			menu[selectInputButtonIndex] = new MenuButton(playerSelectInputButton, m_InputManager.GetInputDeviceName(playerInput), SelectInputPressed);
			
			int selectClassButtonIndex = selectInputButtonIndex + 1;
			menu[selectClassButtonIndex] = new MenuButton(playerSelectClassButton, GameHunt.GetPlayerClassName(playerClass), SelectClassPressed);
		}
		
		m_MenuButtons = menu;
	}
	
	private void EndSequence()
	{
		//@FIXME: should it use EndSequence() when playing a local game?
		
		m_IsActiveSequence = false;
		
		m_LevelManager.LoadLevel(m_NextLevelIndex);
	}
	
	private void ClientEndSequence()
	{
		m_IsActiveSequence = false;
		
		//@FIXME: should client request level load or the server should send it automatically?? => probably server
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		m_LevelManager.ClientLoadLevelRequest(localNetClient);
	}

	private void ServerEndSequence()
	{
		m_IsActiveSequence = false;
		
		m_LevelManager.ServerLoadLevel(m_NextLevelIndex);
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		bool isSequenceActive = IsSequenceActive();
		if (isSequenceActive)
		{
			UpdateMenuLabels();

			UpdateMenuInputs(deltaTime);
			
			UpdateMenuPanels(deltaTime);
			
			UpdateLevelTransitionRequest(deltaTime);
		}
	}
	
	private void UpdateMenuInputs(float _DeltaTime)
	{
		InputManager inputMgr = m_InputManager;
		MenuManager menuMgr = m_MenuManager;

		int buttonCount = m_MenuButtons.Length;

		InputDevice menuControllerDevice = GetMenuControllerInputDevice();

		bool mouseMoved = inputMgr.IsMouseMoved();
		
		bool validateMenuPressed = menuMgr.IsValidateMenuPressed(menuControllerDevice);
		
		bool inputDown = menuMgr.IsMenuInputDownPressed(menuControllerDevice);
		bool inputUp = menuMgr.IsMenuInputUpPressed(menuControllerDevice);
		
		float inputVerticalWaitTime = m_MenuInputVerticalAxisWaitTime - _DeltaTime;
		if (inputVerticalWaitTime > 0.0f)
		{
			//m_InputVerticalDown = m_InputVerticalDown || inputVerticalDown;
			//m_InputVerticalUp = m_InputVerticalUp || inputVerticalUp;
			inputDown = false;
			inputUp = false;
		}
		else
		{
			//inputVerticalDown = m_InputVerticalDown || inputVerticalDown;
			//inputVerticalUp = m_InputVerticalUp || inputVerticalUp;
			//m_InputVerticalDown = false;
			//m_InputVerticalUp = false;
			float menuInputAxisLatency = menuMgr.GetMenuInputAxisLatency();
			inputVerticalWaitTime = (inputDown || inputUp)? menuInputAxisLatency : 0.0f;
		}
		m_MenuInputVerticalAxisWaitTime = inputVerticalWaitTime;
		
		bool disableFocus = mouseMoved || (buttonCount == 0);
		bool updateFocus = inputDown || inputUp;
		bool pressFocusButton = IsMenuFocusValid() && validateMenuPressed;
		
		if (disableFocus)
		{
			DisableMenuFocus();
		}
		else if (updateFocus)
		{
			UpdateMenuFocus(inputDown, inputUp);
		}
		else if (pressFocusButton)
		{
			MenuButton focusButton = m_MenuButtons[m_FocusOnButtonIndex];
			focusButton.m_TriggerDelegate();
		}
	}
	
	private void UpdateMenuPanels(float _DeltaTime)
	{
		m_CurrentPanelIndex = 0;
		
		if (IsUIEnable())
		{
			m_CurrentPanelIndex = 1;
		}
	}
	
	private void UpdateLevelTransitionRequest(float _DeltaTime)
	{
		bool nextLevelIsValid = m_LevelManager.IsValidLevelIndex(m_NextLevelIndex);
		if (nextLevelIsValid)
		{
			NetworkServer networkServer = m_NetworkManager.GetServer();
			if (networkServer == null)
			{
				Debug.Log("UpdateLevelTransitionRequest: No Network Server - initialization failed?");

				EndSequence();
			}
			else if (networkServer.IsServerStarted())
			{
				ServerEndSequence();
/*
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				InputDevice localPlayerInput = GetLocalPlayerInputDevice(0);
				
				if (m_PlayerManager.HasLocalPlayerJoined(localNetClient, localPlayerInput))
				{
					ServerEndSequence();
				}
				else if (m_PlayerManager.IsLocalPlayerJoining(localNetClient, localPlayerInput) == false)
				{
					bool isAuthority = m_NetworkManager.IsNetworkAuthorithy();
					//@FIXME handle the case where the RPC call to server got lost/discarded?
					m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, isAuthority);
				}
*/
			}
		}
/*
		else
		{
			NetworkClient networkClient = m_NetworkManager.GetClient();
			if (networkClient != null && networkClient.IsConnectedToServer())
			{
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				InputDevice localPlayerInput = GetLocalPlayerInputDevice(0);
				
				if (m_PlayerManager.HasLocalPlayerJoined(localNetClient, localPlayerInput))
				{
					ClientEndSequence();
				}
				else if (m_PlayerManager.IsLocalPlayerJoining(localNetClient, localPlayerInput) == false)
				{
					bool isAuthority = m_NetworkManager.IsNetworkAuthorithy();
					System.Diagnostics.Debug.Assert(isAuthority == false);
					//@FIXME handle the case where the RPC call to server got lost/discarded?
					m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, isAuthority);
				}
				else
				{
					//@TODO: add some time out?
				}
			}
		}
*/
	}
	
	private void DisableMenuFocus()
	{
		m_FocusOnButtonIndex = -1;
	}
	
	private bool IsMenuFocusValid()
	{
		int buttonCount = m_MenuButtons.Length;
		bool validButtonIndex = (0 <= m_FocusOnButtonIndex) && (m_FocusOnButtonIndex < buttonCount);
		
		return validButtonIndex;
	}
	
	private void UpdateMenuFocus(bool _InputDown, bool _InputUp)
	{
		int buttonCount = m_MenuButtons.Length;
		System.Diagnostics.Debug.Assert(buttonCount > 0);
		
		int focusButtonIndex = 0;
		
		bool validMenuFocus = IsMenuFocusValid();
		if (validMenuFocus)
		{
			bool inputPrev = _InputUp;
			bool inputNext = _InputDown;
			
			int listElementIndex = m_FocusOnButtonIndex;
			int listElementCount = buttonCount;
			
			focusButtonIndex = m_MenuManager.ComputeNewListElementIndex(listElementIndex, listElementCount, inputPrev, inputNext);
		}
		
		if (focusButtonIndex != m_FocusOnButtonIndex)
		{
			Debug.Log("Update Input: change focus from " + m_FocusOnButtonIndex.ToString() + " to " + focusButtonIndex.ToString());
			m_FocusOnButtonIndex = focusButtonIndex;
		}
	}
	
	private void BackToMainMenu()
	{
		//Debug.Log("Back To Main Menu");
		RequestLevelTransition(m_MainMenuSceneIndex);
	}
	
	private void RequestLevelTransition(int _LevelIndex)
	{
		//Debug.Log("Request Level Transition to " + _LevelIndex.ToString());
		
		m_NextLevelIndex = _LevelIndex;
	}

	private void SelectInputPressed()
	{
		MenuButton focusedButton = m_MenuButtons[m_FocusOnButtonIndex];
		string curFocusControlName = focusedButton.m_ControlName;

		int playerCount = GetLocalPlayerCount();
		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			string playerSelectInputButton = GetPlayerSelectInputButtonName(playerIndex);
			if (curFocusControlName.Equals(playerSelectInputButton))
			{
				InputDevice[] supportedInputDevices = m_InputManager.GetSupportedInputDevices();
				InputDevice playerDevice = GetLocalPlayerInputDevice(playerIndex);
				
				InputDevice availableInput = GetNextAvailableInputDevice(supportedInputDevices, playerDevice);
				if (m_InputManager.IsInputDeviceValid(availableInput))
				{
					SetLocalPlayerInputDevice(playerIndex, availableInput);
				}
			}
		}
	}		
	private void SelectClassPressed()
	{
		MenuButton focusedButton = m_MenuButtons[m_FocusOnButtonIndex];
		string curFocusControlName = focusedButton.m_ControlName;
		
		int playerCount = GetLocalPlayerCount();
		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			string playerSelectClassButton = GetPlayerClassButtonName(playerIndex);
			if (curFocusControlName.Equals(playerSelectClassButton))
			{
				PlayerClass curPlayerClass = GetLocalPlayerClass(playerIndex);
				PlayerClass newPlayerClass = GameHunt.GetNextPlayerClass(curPlayerClass);
				SetLocalPlayerClass(playerIndex, newPlayerClass);
			}
			else
			{
				//@HACK Forcing other player to switch class TOO
				PlayerClass curPlayerClass = GetLocalPlayerClass(playerIndex);
				PlayerClass newPlayerClass = GameHunt.GetNextPlayerClass(curPlayerClass);
				SetLocalPlayerClass(playerIndex, newPlayerClass);
			}
		}
	}
	
	private void StartButtonPressed()
	{
		List<Player> players = m_PlayerManager.GetPlayers();
		int playerCount = players.Count;
		
		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			Player player = players[playerIndex];
			InputDevice playerInput = GetLocalPlayerInputDevice(playerIndex);
			player.SetPlayerInput(playerInput);

			PlayerClass playerClass = GetLocalPlayerClass(playerIndex);
			player.SetPlayerClass(playerClass);
		}

		RequestLevelTransition(m_GameSceneIndex);
			
		DisableUI();
	}
	
	private void BackButtonPressed()
	{
		BackToMainMenu();

		DisableUI();
	}
	
	void OnGUI ()
	{
		if (m_BackgroundImage)
		{
			GuiManager.GUIDrawTextureOnScreen(m_BackgroundImage);
		}

		KeyCode currentEventKeyCode = Event.current.keyCode;
		if (currentEventKeyCode != KeyCode.None)
		{
			//Debug.Log("Current detected event with keycode: " + currentEventKeyCode.ToString());
		}
		
		//@NOTE: UI might have been disabled if focused button was pressed
		bool isUIEnable = IsUIEnable();
		if (isUIEnable)
		{
			bool validMenuFocus = IsMenuFocusValid();
			if (validMenuFocus)
			{
				string curFocusControlName = GUI.GetNameOfFocusedControl();
				int curFocusButtonIndex = -1;
				
				int buttonCount = m_MenuButtons.Length;
				for (int buttonIndex = 0; buttonIndex < buttonCount; ++buttonIndex)
				{
					MenuButton button = m_MenuButtons[buttonIndex];
					if (button.m_ControlName == curFocusControlName)
					{
						curFocusButtonIndex = buttonIndex;
						break;
					}
				}
				if (curFocusButtonIndex != m_FocusOnButtonIndex)
				{
					Debug.Log("OnGui: change focus from " + curFocusButtonIndex.ToString() + " to " + m_FocusOnButtonIndex.ToString());
					
					MenuButton focusButton = m_MenuButtons[m_FocusOnButtonIndex];
					string focusControlName = focusButton.m_ControlName;
					
					GUI.FocusControl(focusControlName);
				}
			}
			else
			{
				GUI.FocusControl("");
			}
			
			//@FIXME: make all panel code data driven / editable from editor
			
			int panelSlideOffsetX = -200;
			
			int panelWidth = 200;
			int panelHeight = 400;
			
			int centerPanelOffsetX = Screen.width / 2 - panelWidth / 2;
			int centerPanelOffsetY = Screen.height / 2 - panelHeight / 2;
			
			int mainPanelIndex = 1;
			if (m_CurrentPanelIndex >= mainPanelIndex)
			{
				int mainPanelSlideCount = m_CurrentPanelIndex - mainPanelIndex;
				int mainPanelOffsetX = centerPanelOffsetX + mainPanelSlideCount * panelSlideOffsetX;
				int mainPanelOffsetY = centerPanelOffsetY;
				Rect mainPanel = new Rect( mainPanelOffsetX, mainPanelOffsetY, panelWidth, panelHeight);
				
				GUI.Box(mainPanel, "<b>Player Select Menu</b>");
				int mainPanelTitleTextHeight = 30;
				
				int mainPanelButtonWidth = 160;
				int mainPanelButtonHeight = 30;
				int mainPanelButtonOffsetX = mainPanelOffsetX + (panelWidth - mainPanelButtonWidth) / 2;
				int mainPanelButtonOffsetY = mainPanelOffsetY + mainPanelTitleTextHeight;
				//int mainPanelPlayerInterspaceY = 60;
				int mainPanelButtonInterspaceY = 40;
				
				int buttonCount = m_MenuButtons.Length;
				for (int buttonIndex = 0; buttonIndex < buttonCount && isUIEnable; ++buttonIndex)
				{
					MenuButton menuButton = m_MenuButtons[buttonIndex];
					
					GUI.SetNextControlName(menuButton.m_ControlName);
					if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), menuButton.m_LabelName))
					{
						m_FocusOnButtonIndex = buttonIndex;

						menuButton.m_TriggerDelegate();
						//@NOTE: UI might have been disabled if button was pressed
						isUIEnable = IsUIEnable();
					}
					
					mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
				}
				
				/*				
				string createGameButtonLabel = "Create Game";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), createGameButtonLabel))
				{
					CreateGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
		
				string joinGameButtonLabel = "Join Game";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), joinGameButtonLabel))
				{
					JoinGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
				
				string editLevelButtonLabel = "Edit Level";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), editLevelButtonLabel))
				{
					EditLevelButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
				
				string aboutGameButtonLabel = "About";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), aboutGameButtonLabel))
				{
					AboutGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
		
				string exitGameButtonLabel = "Exit";
				if (GUI.Button(new Rect(mainPanelButtonOffsetX, mainPanelButtonOffsetY, mainPanelButtonWidth, mainPanelButtonHeight), exitGameButtonLabel))
				{
					QuitGameButtonPressed();
				}
				mainPanelButtonOffsetY += mainPanelButtonInterspaceY;
*/
			}
/*			
			int selectPanelIndex = 2;
			if (m_CurrentPanelIndex == selectPanelIndex)
			{
				int joinPanelSlideCount = m_CurrentPanelIndex - selectPanelIndex;
				int joinPanelOffsetX = centerPanelOffsetX + joinPanelSlideCount * panelSlideOffsetX;
				int joinPanelOffsetY = centerPanelOffsetY;
				Rect joinPanel = new Rect( joinPanelOffsetX, joinPanelOffsetY, panelWidth, panelHeight);
				
				GUI.Box(joinPanel, "\n<b>Players Select Screen:</b>");
				
				int joinPanelTitleHeight = 40;
				
				int joinPanelButtonHeight = 40;
				int joinPanelButtonWidth = 160;
				int joinPanelButtonOffsetX = joinPanelOffsetX + (panelWidth - joinPanelButtonWidth) / 2;
				int joinPanelButtonOffsetY = joinPanelOffsetY + joinPanelTitleHeight;
				int joinPanelButtonInterspaceY = 60;
				
				int playerCount = GetLocalPlayerCount();
				for (int playerIndex = 0; playerIndex < playerCount && isUIEnable; ++playerIndex)
				{
					GUI.Box(new Rect(joinPanelButtonOffsetX, joinPanelButtonOffsetY, joinPanelButtonWidth, joinPanelButtonHeight), string.Format( "Player %d:", playerIndex) );
					joinPanelButtonOffsetY += joinPanelButtonInterspaceY;
					
					MenuButton menuButton = m_MenuButtons[buttonIndex];
					
					GUI.SetNextControlName(menuButton.m_ControlName);
					if (GUI.Button(new Rect(joinPanelButtonOffsetX, joinPanelButtonOffsetY, joinPanelButtonWidth, joinPanelButtonHeight), menuButton.m_LabelName))
					{
						//
					}
					
					joinPanelButtonOffsetY += joinPanelButtonInterspaceY;
				}
			}
*/
		}
	}
}
