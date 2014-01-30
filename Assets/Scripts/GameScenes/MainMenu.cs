using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

//public enum MenuState { E_MenuNone, E_MenuMainPanel, E_MenuJoinPanel, E_MenuStartingGame, E_MenuConnectingGame, E_MenuTransitioningToLevel, E_MenuQuitingGame,};

public delegate void ButtonTriggerDelegate();

public class MenuButton
{
	public string m_ControlName = null;
	public string m_LabelName = null;
	public ButtonTriggerDelegate m_TriggerDelegate = null;
	
	public MenuButton(string _ControlName, string _LabelName, ButtonTriggerDelegate _TriggerDelegate)
	{
		m_ControlName = _ControlName;
		m_LabelName = _LabelName;
		m_TriggerDelegate = _TriggerDelegate;
	}
}

public class MainMenu : MonoBehaviour
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
	private int m_SelectSceneIndex = 0;
	[SerializeField]
	private int m_CreditSceneIndex = 0;

	[SerializeField]
	private Texture2D m_BackgroundImage = null;
	[SerializeField]
	private bool m_CenterMenu = true;
	[SerializeField]
	private int m_CenterOffsetX = 0;
	[SerializeField]
	private int m_CenterOffsetY = 0;
	[SerializeField]
	private int m_PanelWidth = 0;
	[SerializeField]
	private int m_PanelHeight = 0;
	[SerializeField]
	private int m_PanelButtonWidth = 0;
	[SerializeField]
	private int m_PanelButtonHeight = 0;
	[SerializeField]
	private string m_PanelTitle = "";
	
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

	[SerializeField]
	private int m_PlayerCount = 2;
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

	private InputDevice GetAvailableInputDevice(InputDevice[] _SupportedInputDevices)
	{
		InputDevice availableInput = InputDevice.E_InputDeviceNone;
		foreach (InputDevice supportedInput in _SupportedInputDevices)
		{
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
		
		return availableInput;
	}
	
	private MenuButton[] m_MenuButtons = null;
	
	//@HACK temporary display solution for server list
	private string[] m_DisplayServerList = null;
	private Vector2 m_ScrollPositionServerSelect = Vector2.zero;
	public int m_SelectedDisplayedServerIndex = -1;
	public float m_ServerListUpdateTimeIntervalInSeconds = 2.0f;
	private float m_TimeSinceLastServerListUpdate = 0.0f;
	
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
		m_PlayerManager.RemoveAllPlayers();
	}

	private void InitMenu()
	{
		int playerCount = GetLocalPlayerCount();
		m_PlayersInputDevice = new InputDevice[playerCount];

		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			SetLocalPlayerInputDevice(playerIndex, InputDevice.E_InputDeviceNone);
		}

		//@HACK: this should be data driven, specified from Unity editor
		int mainMenuButtonCount = 3;

		int menuButtonCount = mainMenuButtonCount;
		MenuButton[] menu = new MenuButton[menuButtonCount];
		menu[0] = new MenuButton("PlayButton", "Play Game", PlayButtonPressed);
		menu[1] = new MenuButton("AboutButton", "Intro", AboutGameButtonPressed);
		menu[2] = new MenuButton("QuitButton", "Exit", QuitGameButtonPressed);
		
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
		
		bool mouseMoved = inputMgr.IsMouseMoved();
		
		bool validateMenuPressed = menuMgr.IsValidateMenuPressed();
		
		bool inputDown = menuMgr.IsMenuInputDownPressed();
		bool inputUp = menuMgr.IsMenuInputUpPressed();
		
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
			
			if (m_NetworkManager)
			{
				UpdateServerList(_DeltaTime);
			}
			
			if (m_DisplayServerList != null)
			{
				++m_CurrentPanelIndex;
			}
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
				//Debug.Log("UpdateLevelTransitionRequest: No Network Server - initialization failed?");
				EndSequence();
			}
			else if (networkServer.IsServerStarted())
			{
				NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
				InputDevice localPlayerInput = GetLocalPlayerInputDevice(0);

				bool isAuthority = m_NetworkManager.IsNetworkAuthorithy();
				
				if (m_PlayerManager.HasLocalPlayerJoined(localNetClient, localPlayerInput))
				{
					InputDevice[] supportedInputDevices = m_InputManager.GetSupportedInputDevices();
					
					int playerCount = GetLocalPlayerCount();
					for (int playerIndex = 0; playerIndex < playerCount-1; ++playerIndex)
					{
						InputDevice availableInput = GetAvailableInputDevice(supportedInputDevices);
						m_PlayerManager.AddLocalPlayerToJoin(localNetClient, availableInput, isAuthority);
					}

					ServerEndSequence();
				}
				else if (m_PlayerManager.IsLocalPlayerJoining(localNetClient, localPlayerInput) == false)
				{
					//@FIXME handle the case where the RPC call to server got lost/discarded?
					m_PlayerManager.AddLocalPlayerToJoin(localNetClient, localPlayerInput, isAuthority);
				}
			}
		}
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
			//Debug.Log("Update Input: change focus from " + m_FocusOnButtonIndex.ToString() + " to " + focusButtonIndex.ToString());
			m_FocusOnButtonIndex = focusButtonIndex;
		}
	}
	
	private void QuitGame()
	{
		//Debug.Log("Quit Game");
		
		m_LevelManager.QuitGame();
	}
	
	private void RequestLevelTransition(int _LevelIndex)
	{
		//Debug.Log("Request Level Transition to " + _LevelIndex.ToString());
		
		m_NextLevelIndex = _LevelIndex;
	}
	
	
	
	private void PlayButtonPressed()
	{
		if (m_NetworkManager != null)
		{
			m_NetworkManager.StopServerSearch();
			m_NetworkManager.StartServer(null, null);
		}
		
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			SetLocalPlayerInputDevice(0, playerInputDevice);
			
			RequestLevelTransition(m_SelectSceneIndex);
			
			DisableUI();
		}
	}
	
	private void JoinGameButtonPressed()
	{
		if (m_NetworkManager != null)
		{
			m_NetworkManager.StartServerSearch();
			m_TimeSinceLastServerListUpdate = m_ServerListUpdateTimeIntervalInSeconds;
		}
	}
	
	private void AboutGameButtonPressed()
	{
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			SetLocalPlayerInputDevice(0, playerInputDevice);
			
			RequestLevelTransition(m_CreditSceneIndex);
			
			DisableUI();
		}
	}
	
	private void QuitGameButtonPressed()
	{
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			QuitGame();
			
			DisableUI();
		}
	}
	
	private void ConnectGameButtonPressed()
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		System.Diagnostics.Debug.Assert(m_SelectedDisplayedServerIndex >= 0);
		
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
			if (networkServerSearch != null)
			{
				//@FIXME: technically getting an updated list at that point can be a problem because the selected server index might be affected
				List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
				int serverCount = serverList.Count;
				if (m_SelectedDisplayedServerIndex < serverCount)
				{
					SetLocalPlayerInputDevice(0, playerInputDevice);
					
					NetworkServerSearch.ServerSearchData server = serverList[m_SelectedDisplayedServerIndex];
					string serverIpAddress = server.m_ServerIpAddressString;
					int connectPort = server.m_ServerConnectPort;
					
					m_NetworkManager.StopServerSearch();
					m_NetworkManager.ConnectToServer(serverIpAddress, connectPort);
					
					DisableUI();
				}
			}
		}
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
					//Debug.Log("OnGui: change focus from " + curFocusButtonIndex.ToString() + " to " + m_FocusOnButtonIndex.ToString());
					
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
			
			int panelSlideOffsetX = -m_PanelWidth;
			
			int panelWidth = m_PanelWidth;
			int panelHeight = m_PanelHeight;
			
			int centerPanelOffsetX = (m_CenterMenu)? Screen.width / 2 - panelWidth / 2 : m_CenterOffsetX;
			int centerPanelOffsetY = (m_CenterMenu)? Screen.height / 2 - panelHeight / 2 : m_CenterOffsetY;
			
			int mainPanelIndex = 1;
			if (m_CurrentPanelIndex >= mainPanelIndex)
			{
				int mainPanelSlideCount = m_CurrentPanelIndex - mainPanelIndex;
				int mainPanelOffsetX = centerPanelOffsetX + mainPanelSlideCount * panelSlideOffsetX;
				int mainPanelOffsetY = centerPanelOffsetY;
				Rect mainPanel = new Rect( mainPanelOffsetX, mainPanelOffsetY, panelWidth, panelHeight);

				string panelTitle = string.Format("\n<b>{0}</b>", m_PanelTitle);
				GUI.Box(mainPanel, panelTitle);
				int mainPanelTitleTextHeight = 80;
				
				int mainPanelButtonWidth = m_PanelButtonWidth;
				int mainPanelButtonHeight = m_PanelButtonHeight;
				int mainPanelButtonOffsetX = mainPanelOffsetX + (panelWidth - mainPanelButtonWidth) / 2;
				int mainPanelButtonOffsetY = mainPanelOffsetY + mainPanelTitleTextHeight;
				int mainPanelButtonInterspaceY = 60;
				
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
			//@HACK temporary server list display solution
			int joinPanelIndex = 2;
			if (m_CurrentPanelIndex >= joinPanelIndex)
			{
				int joinPanelSlideCount = m_CurrentPanelIndex - joinPanelIndex;
				int joinPanelOffsetX = centerPanelOffsetX + joinPanelSlideCount * panelSlideOffsetX;
				int joinPanelOffsetY = centerPanelOffsetY;
				Rect joinPanel = new Rect( joinPanelOffsetX, joinPanelOffsetY, panelWidth, panelHeight);
				
				GUI.Box(joinPanel, "\n<b>Existing servers:</b>");
				
				int joinPanelTitleHeight = 40;
				
				int joinPanelButtonHeight = 40;
				int joinPanelButtonWidth = 160;
				int joinPanelButtonOffsetX = joinPanelOffsetX + (panelWidth - joinPanelButtonWidth) / 2;
				
				System.Diagnostics.Debug.Assert(m_DisplayServerList != null);
				int serverCount = m_DisplayServerList.Length;
				if (serverCount > 0)
				{
					int joinPanelServerListWidth = 180;
					int joinPanelServerListHeight = 300;
					
					int joinPanelServerListOffsetX = joinPanelOffsetX + (panelWidth - joinPanelServerListWidth) / 2;
					int joinPanelServerListOffsetY = joinPanelOffsetY + joinPanelTitleHeight;
					
					DrawServerList(joinPanelServerListOffsetX, joinPanelServerListOffsetY, joinPanelServerListWidth, joinPanelServerListHeight);
					
					if (m_SelectedDisplayedServerIndex >= 0 && m_SelectedDisplayedServerIndex < serverCount)
					{
						int joinPanelConnectGameButtonOffsetY = joinPanelOffsetY + joinPanelTitleHeight + joinPanelServerListHeight;
						
						string connectGameButtonLabel = "Connect Game";
						if (GUI.Button(new Rect(joinPanelButtonOffsetX, joinPanelConnectGameButtonOffsetY, joinPanelButtonWidth, joinPanelButtonHeight), connectGameButtonLabel))
						{
							ConnectGameButtonPressed();
						}
					}
				}
			}
*/
		}
	}
	
	private void UpdateServerList(float _DeltaTime)
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		NetworkServerSearch networkServerSearch = m_NetworkManager.GetServerSearch();
		
		if (networkServerSearch != null)
		{
			m_TimeSinceLastServerListUpdate += _DeltaTime;
			if ( m_TimeSinceLastServerListUpdate >= m_ServerListUpdateTimeIntervalInSeconds)
			{
				List<NetworkServerSearch.ServerSearchData> serverList = networkServerSearch.GetServerList();
				if (serverList != null)
				{
					m_DisplayServerList = serverList.Select(x => x.m_ServerName).ToArray();
				}
				else
				{
					string[] emptyList = {};
					m_DisplayServerList = emptyList;
				}
				
				m_TimeSinceLastServerListUpdate = 0.0f;
			}
		}
		else
		{
			m_TimeSinceLastServerListUpdate = m_ServerListUpdateTimeIntervalInSeconds;
			m_DisplayServerList = null;
		}
	}
	
	private void DrawServerList(int _PanelOffsetX, int _PanelOffsetY, int _PanelWidth, int _PanelHeight)
	{
		//@HACK temporary display solution
		
		Rect scrollViewRect = new Rect(_PanelOffsetX, _PanelOffsetY, _PanelWidth, _PanelHeight);//Screen.width / 2 + 120, Screen.height / 2 - 200, 200, 400);
		Rect scrollAreaRect = new Rect (0, 0, _PanelWidth, _PanelHeight);//0, 0, 200, 400);
		Vector2 scrollPos = m_ScrollPositionServerSelect;
		bool alwaysShowHorizontalScrollBar = false;
		bool alwaysShowVerticalScrollBar = false;
		
		scrollPos = GUI.BeginScrollView(scrollViewRect, scrollPos, scrollAreaRect, alwaysShowHorizontalScrollBar, alwaysShowVerticalScrollBar);
		
			m_SelectedDisplayedServerIndex = GUILayout.SelectionGrid(m_SelectedDisplayedServerIndex, m_DisplayServerList, 1);
		
		GUI.EndScrollView();
	}
}
