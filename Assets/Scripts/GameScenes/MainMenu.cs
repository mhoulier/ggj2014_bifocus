using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using System.Linq;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	private InputManager m_InputManager = null;
	private MenuManager m_MenuManager = null;
	private PlayerManager m_PlayerManager = null;
	private NetworkManager m_NetworkManager = null;

	private bool IsNetworkGameSupported() { return (m_NetworkManager != null); }
	
	private bool m_IsActiveSequence = false;
	private bool IsSequenceActive() { return m_IsActiveSequence; }
	
	private int m_NextLevelIndex = -1;

	[SerializeField]
	private int m_SelectSceneIndex = 0;
	[SerializeField]
	private int m_IntroSceneIndex = 0;

	[SerializeField]
	private Texture2D m_BackgroundImage = null;
	[SerializeField]
	private bool m_CenterMenu = true;
	[SerializeField]
	private int m_CenterOffsetX = 0;
	[SerializeField]
	private int m_CenterOffsetY = 0;

	[SerializeField]
	private MenuPanel[] m_MenuPanels = null;

	[SerializeField]
	private int m_MainPanelIndex = 0;

	private List<MenuPanel> m_ActiveMenuPanels = null;
	
	private bool m_enableUI = false;
	private bool IsUIEnable() { return m_enableUI; }
	private void EnableUI() { m_enableUI = true; }
	private void DisableUI(){ m_enableUI = false; }

	private int m_FocusOnButtonIndex = -1;
	
	private float m_MenuInputVerticalAxisWaitTime = 0.0f;
	//private bool m_InputVerticalDown = false;
	//private bool m_InputVerticalUp = false;

	[SerializeField]
	private int m_LocalPlayerCount = 2;
	private int GetLocalPlayerCount(){ return m_LocalPlayerCount; }

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

		NetworkManager networkManager = (NetworkManager)FindObjectOfType( typeof(NetworkManager) );
		if (networkManager == null)
		{
			Debug.Log("GameManager prefab has no NetworkManager component: Only local games supported!");
		}
		
		m_LevelManager = levelManager;
		m_InputManager = inputManager;
		m_MenuManager = menuManager;
		m_PlayerManager = playerManager;
		m_NetworkManager = networkManager;
		
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
		m_PlayerManager.RequestRemoveAllPlayers();
	}

	private MenuEventDelegate ResolveMenuEventDelegate(MenuEvent _MenuEvent)
	{
		MenuEventDelegate menuEventDelegate = null;
		switch (_MenuEvent)
		{
		case MenuEvent.E_MenuPlayLocalPressed:
			menuEventDelegate = PlayLocalGameButtonPressed;
			break;
		case MenuEvent.E_MenuIntroPressed:
			menuEventDelegate = PlayIntroButtonPressed;
			break;
		case MenuEvent.E_MenuQuitPressed:
			menuEventDelegate = QuitGameButtonPressed;
			break;
		default:
			Debug.Log(string.Format("MainMenu doesn't support menu event '{0}'",  _MenuEvent.ToString()));
			break;
		}

		return menuEventDelegate;
	}

	private void InitMenu()
	{
		int menuPanelCount = m_MenuPanels.Length;
		if (menuPanelCount > m_MainPanelIndex)
		{
			MenuPanel mainPanel = m_MenuPanels[m_MainPanelIndex];

			foreach (MenuPanel panel in m_MenuPanels)
			{
				MenuButton[] buttons = panel.GetMenuButtons();
				foreach (MenuButton button in buttons)
				{
					MenuEventDelegate buttonDelegate = ResolveMenuEventDelegate(button.GetMenuEventToTrigger());
					button.InitTriggerDelegate(buttonDelegate);
				}
			}

			m_ActiveMenuPanels = new List<MenuPanel>(menuPanelCount);
			m_ActiveMenuPanels.Add(mainPanel);
		}

		int playerCount = GetLocalPlayerCount();
		m_PlayersInputDevice = new InputDevice[playerCount];

		for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
		{
			SetLocalPlayerInputDevice(playerIndex, InputDevice.E_InputDeviceNone);
		}
	}
	
	private void EndSequence()
	{
		//@NOTE: only use EndSequence() when playing a local game, otherwise consider server / client versions
		
		m_IsActiveSequence = false;
		
		m_LevelManager.LoadLevel(m_NextLevelIndex);
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

		int activePanelCount = m_ActiveMenuPanels.Count;
		System.Diagnostics.Debug.Assert(activePanelCount > 0);
		MenuPanel mainPanel = m_ActiveMenuPanels[activePanelCount-1];
		
		MenuButton[] buttons = mainPanel.GetMenuButtons();
		int buttonCount = buttons.Length;
		
		bool mouseMoved = inputMgr.IsMouseMoved();
		
		bool validateMenuPressed = menuMgr.IsMenuActionInputPressed(MenuActionType.E_MenuValidate);
		
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
		bool pressFocusButton = IsMenuFocusValid(buttons) && validateMenuPressed;
		
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
			MenuButton focusButton = buttons[m_FocusOnButtonIndex];
			focusButton.TriggerDelegate();
		}
	}
	
	private void UpdateMenuPanels(float _DeltaTime)
	{
		if (IsUIEnable())
		{
		}
	}

	private void UpdateLevelTransitionRequest(float _DeltaTime)
	{
		bool nextLevelIsValid = m_LevelManager.IsValidLevelIndex(m_NextLevelIndex);
		if ( nextLevelIsValid )
		{
			//@TODO: Wait for pending local players

			EndSequence();
		}
	}
	
	private void DisableMenuFocus()
	{
		m_FocusOnButtonIndex = -1;
	}
	
	private bool IsMenuFocusValid(MenuButton[] _MenuButtons)
	{
		int buttonCount = _MenuButtons.Length;
		bool validButtonIndex = (0 <= m_FocusOnButtonIndex) && (m_FocusOnButtonIndex < buttonCount);
		
		return validButtonIndex;
	}
	
	private void UpdateMenuFocus(bool _InputDown, bool _InputUp)
	{
		int activePanelCount = m_ActiveMenuPanels.Count;
		System.Diagnostics.Debug.Assert(activePanelCount > 0);
		MenuPanel mainPanel = m_ActiveMenuPanels[activePanelCount-1];
		
		MenuButton[] buttons = mainPanel.GetMenuButtons();
		
		int buttonCount = buttons.Length;
		System.Diagnostics.Debug.Assert(buttonCount > 0);
		
		int focusButtonIndex = 0;

		bool validMenuFocus = IsMenuFocusValid(buttons);
		if (validMenuFocus)
		{
			bool inputPrev = _InputUp;
			bool inputNext = _InputDown;
			
			int listElementIndex = m_FocusOnButtonIndex;
			int listElementCount = buttonCount;

			focusButtonIndex = MenuManager.ComputeNewListElementIndex(listElementIndex, listElementCount, inputPrev, inputNext);
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
	
	private void PlayLocalGameButtonPressed(MenuEvent _MenuEvent)
	{
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		int playerCount = GetLocalPlayerCount();

		if (validInputDevice && playerCount > 0)
		{
			//Play a local game
			bool networkGameSupported = IsNetworkGameSupported();
			if (networkGameSupported)
			{
				m_NetworkManager.SetNetworkMode(NetworkMode.E_NetworkNone);
			}

			SetLocalPlayerInputDevice(0, playerInputDevice);
			m_PlayerManager.RequestLocalPlayerJoin(playerInputDevice);
			
			InputDevice[] validDevices = m_InputManager.GetValidInputDevices();
			for (int playerIndex = 1; playerIndex < playerCount; ++playerIndex)
			{
				InputDevice availableDevice = GetAvailableInputDevice(validDevices);
				m_PlayerManager.RequestLocalPlayerJoin(availableDevice);
			}
			
			RequestLevelTransition(m_SelectSceneIndex);
			
			DisableUI();
		}
	}
	
	private void PlayIntroButtonPressed(MenuEvent _MenuEvent)
	{
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			SetLocalPlayerInputDevice(0, playerInputDevice);
			
			RequestLevelTransition(m_IntroSceneIndex);
			
			DisableUI();
		}
	}
	
	private void QuitGameButtonPressed(MenuEvent _MenuEvent)
	{
		InputDevice playerInputDevice = m_MenuManager.GetPlayerInputDeviceFromValidateMenuInput();
		bool validInputDevice = m_InputManager.IsInputDeviceValid(playerInputDevice);
		if (validInputDevice)
		{
			QuitGame();
			
			DisableUI();
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
			int activePanelCount = m_ActiveMenuPanels.Count;
			if (activePanelCount > 0)
			{
				int lastActivePanelIndex = activePanelCount-1;
				//@NOTE: last active panel as the focus and only last panel button are active
				MenuPanel topActivePanel = m_ActiveMenuPanels[lastActivePanelIndex];
				MenuButton[] activeButtons = topActivePanel.GetMenuButtons();

				bool validMenuFocus = IsMenuFocusValid(activeButtons);
				if (validMenuFocus)
				{
					string curFocusControlName = GUI.GetNameOfFocusedControl();
					int curFocusButtonIndex = -1;

					int buttonCount = activeButtons.Length;
					for (int buttonIndex = 0; buttonIndex < buttonCount; ++buttonIndex)
					{
						MenuButton button = activeButtons[buttonIndex];
						if (button.GetControlName() == curFocusControlName)
						{
							curFocusButtonIndex = buttonIndex;
							break;
						}
					}
					if (curFocusButtonIndex != m_FocusOnButtonIndex)
					{
						//Debug.Log("OnGui: change focus from " + curFocusButtonIndex.ToString() + " to " + m_FocusOnButtonIndex.ToString());
						
						MenuButton focusButton = activeButtons[m_FocusOnButtonIndex];
						string focusControlName = focusButton.GetControlName();
						
						GUI.FocusControl(focusControlName);
					}
				}
				else
				{
					GUI.FocusControl("");
				}

				MenuPanel mainPanel = m_MenuPanels[m_MainPanelIndex];
				int panelOffsetX = (m_CenterMenu)? Screen.width / 2 - mainPanel.m_PanelWidth / 2 + topActivePanel.m_PanelWidth : m_CenterOffsetX + topActivePanel.m_PanelWidth;
				int panelOffsetY = (m_CenterMenu)? Screen.height / 2 - mainPanel.m_PanelHeight / 2 : m_CenterOffsetY;

				for (int activePanelIndex = lastActivePanelIndex; activePanelIndex >= 0 ; --activePanelIndex)
				{
					MenuPanel currentPanel = m_ActiveMenuPanels[activePanelIndex];

					int panelWidth = currentPanel.m_PanelWidth;
					int panelHeight = currentPanel.m_PanelHeight;

					panelOffsetX -= panelWidth;

					Rect panelRect = new Rect( panelOffsetX, panelOffsetY, panelWidth, panelHeight);
					
					string title = string.Format("\n<b>{0}</b>", currentPanel.m_PanelTitle);
					GUI.Box(panelRect, title);

					int titleTextHeight = currentPanel.m_PanelTitleTextHeight;
					
					int buttonWidth = currentPanel.m_PanelButtonWidth;
					int buttonHeight = currentPanel.m_PanelButtonHeight;
					int buttonOffsetX = panelOffsetX + (panelWidth - buttonWidth) / 2;
					int buttonOffsetY = panelOffsetY + titleTextHeight;

					int buttonInterspaceY = currentPanel.m_PanelButtonInterspaceY;

					MenuButton[] buttons = currentPanel.GetMenuButtons();
					int buttonCount = buttons.Length;
					for (int buttonIndex = 0; buttonIndex < buttonCount && isUIEnable; ++buttonIndex)
					{
						MenuButton button = buttons[buttonIndex];
						
						GUI.SetNextControlName(button.GetControlName());
						if (GUI.Button(new Rect(buttonOffsetX, buttonOffsetY, buttonWidth, buttonHeight), button.GetLabelName()))
						{
							//@NOTE: last active panel as the focus and only last panel button are active
							if (activePanelIndex == lastActivePanelIndex)
							{
								m_FocusOnButtonIndex = buttonIndex;
								
								button.TriggerDelegate();
								//@NOTE: UI might have been disabled if button was pressed
								isUIEnable = IsUIEnable();
							}
						}
						
						buttonOffsetY += buttonInterspaceY;
					}
				}
			}
		}
	}
}
