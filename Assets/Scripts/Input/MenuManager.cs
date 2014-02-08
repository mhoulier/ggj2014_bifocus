using UnityEngine;
using System.Collections;

public enum MenuEvent
{
	E_MenuEventNone,
	E_MenuPlayLocalPressed,
	E_MenuPlayNetworkPressed,
	E_MenuIntroPressed,
	E_MenuAboutPressed,
	E_MenuQuitPressed,
	E_MenuStartPressed,
	E_MenuBackPressed,
	E_MenuSelectInputPressed,
	E_MenuSelectClassPressed,
};

public enum MenuActionType
{
	E_MenuActionNone,
	E_MenuValidate,
	E_MenuCancel,
	E_MenuPause,
};

public enum MenuInputPressedEvent
{
	E_MenuInputPressedNone,
	E_MenuInputPressedOnDown,
	E_MenuInputPressedOnUp,
};

public class MenuManager : MonoBehaviour
{
	private InputManager m_InputManager = null;

	[SerializeField]
	private MenuAction[] m_MenuActions;

	[SerializeField]
	private float m_MenuInputAxisLatency = 0.0f;
	[SerializeField]
	private float m_MenuInputAxisThreshold = 0.0f;
	
	//@NOTE: IMPORTANT - Unity3d joystick Y-Axis return POSITIVE values when pressed DOWN
	//Inverting vertical menu input by default
	[SerializeField]
	private bool m_MenuInputVerticalAxisJoystickInvert = true;
	
	public float GetMenuInputAxisLatency() { return m_MenuInputAxisLatency; }

	void Start()
	{
		bool menuInputInitialized = false;

		InputManager inputManager = (InputManager)FindObjectOfType( typeof(InputManager) );
		if (inputManager != null)
		{
			InitMenuActions(inputManager);

			m_InputManager = inputManager;
			menuInputInitialized = true;
		}
		else
		{
			Debug.Log("Scene is missing an instanced game object with an InputManager component!");
		}

		enabled = menuInputInitialized;
	}

	private void InitMenuActions(InputManager _InputManager)
	{
		foreach(MenuAction menuAction in m_MenuActions)
		{
			menuAction.InitMenuInputs(_InputManager);
		}
	}

	void Update()
	{
	
	}
/*
	private int GetMenuActionIndex(MenuActionType _MenuActionType)
	{
		int menuActionIndex = -1;

		const int actionCount = m_MenuActions.Length;
		for (int actionIndex = 0; actionIndex < actionCount; ++actionIndex)
		{
			MenuAction action = m_MenuActions[actionIndex];
			if (action.GetMenuActionType() == _MenuActionType)
			{
				menuActionIndex = actionIndex;
				break;
			}
		}

		return menuActionIndex;
	}
*/
	public bool IsMenuActionInputPressed(MenuActionType _MenuActionType)
	{
		InputManager inputMgr = m_InputManager;

		bool menuActionPressed = false;
		foreach(MenuAction action in m_MenuActions)
		{
			if (action.GetMenuActionType() == _MenuActionType)
			{
				menuActionPressed = action.IsInputPressed(inputMgr);
				break;
			}
		}

		return menuActionPressed;
	}

	public bool IsMenuActionInputPressed(MenuActionType _MenuActionType, InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;
		
		bool menuActionPressed = false;
		foreach(MenuAction action in m_MenuActions)
		{
			if (action.GetMenuActionType() == _MenuActionType)
			{
				menuActionPressed = action.IsInputPressed(inputMgr, _InputDevice);
				break;
			}
		}
		
		return menuActionPressed;
	}

	private bool IsMenuInputVerticalDown(float _InputVertical)
	{
		bool inputDown = (_InputVertical < -m_MenuInputAxisThreshold);
		if (inputDown)
		{
			//Debug.Log("Input Down: " + _InputVertical.ToString());
		}
		return inputDown;
	}
	
	private bool IsMenuInputVerticalUp(float _InputVertical)
	{
		bool inputUp = (_InputVertical > m_MenuInputAxisThreshold);
		if (inputUp)
		{
			//Debug.Log("Input Up: " + _InputVertical.ToString());
		}
		return inputUp;
	}
	
	private bool IsMenuInputHorizontalLeft(float _InputHorizontal)
	{
		bool inputLeft = (_InputHorizontal < -m_MenuInputAxisThreshold);
		if (inputLeft)
		{
			//Debug.Log("Input Left: " + _InputHorizontal.ToString());
		}
		return inputLeft;
	}
	
	private bool IsMenuInputHorizontalRight(float _InputHorizontal)
	{
		bool inputRight = (_InputHorizontal > m_MenuInputAxisThreshold);
		if (inputRight)
		{
			//Debug.Log("Input Right: " + _InputHorizontal.ToString());
		}
		return inputRight;
	}
	
	public bool IsMenuInputDownPressed()
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = false;
		bool invertJoystickAxis = m_MenuInputVerticalAxisJoystickInvert;
		
		float inputVertical = 0.0f;

		InputDevice[] validDevices = inputMgr.GetValidInputDevices();
		foreach(InputDevice validDevice in validDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(validDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(validDevice) : validDevice;
			
			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				bool isJoystick = inputMgr.IsInputJoystick(validDevice);
				inputVertical = (isJoystick && invertJoystickAxis)? -inputAxis : inputAxis;
				break;
			}
		}
		
		bool inputDown = IsMenuInputVerticalDown(inputVertical);
		return inputDown;
	}
	
	public bool IsMenuInputDownPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = false;
		bool isJoystick = inputMgr.IsInputJoystick(_InputDevice);
		bool invertJoystickAxis = m_MenuInputVerticalAxisJoystickInvert;

		float inputVertical = inputMgr.GetInputForAxis(_InputDevice, horizontalAxis);
		inputVertical = (isJoystick && invertJoystickAxis) ? -inputVertical : inputVertical;
		
		bool inputDown = IsMenuInputVerticalDown(inputVertical);
		return inputDown;
	}
	
	public bool IsMenuInputUpPressed()
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = false;
		bool invertJoystickAxis = m_MenuInputVerticalAxisJoystickInvert;
		
		float inputVertical = 0.0f;
		
		InputDevice[] validDevices = inputMgr.GetValidInputDevices();
		foreach(InputDevice validDevice in validDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(validDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(validDevice) : validDevice;
			
			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				bool isJoystick = inputMgr.IsInputJoystick(validDevice);
				inputVertical = (isJoystick && invertJoystickAxis)? -inputAxis : inputAxis;
				break;
			}
		}
		
		bool inputUp = IsMenuInputVerticalUp(inputVertical);
		return inputUp;
	}
	
	public bool IsMenuInputUpPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = false;
		bool isJoystick = inputMgr.IsInputJoystick(_InputDevice);
		bool invertJoystickAxis = m_MenuInputVerticalAxisJoystickInvert;
		
		float inputVertical = inputMgr.GetInputForAxis(_InputDevice, horizontalAxis);
		inputVertical = (isJoystick && invertJoystickAxis)? -inputVertical : inputVertical;
		
		bool inputUp = IsMenuInputVerticalUp(inputVertical);
		return inputUp;
	}
	
	public bool IsMenuInputLeftPressed()
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = true;
		//bool invertJoystickAxis = false;
		
		float inputHorizontal = 0.0f;
		
		InputDevice[] validDevices = inputMgr.GetValidInputDevices();
		foreach(InputDevice validDevice in validDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(validDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(validDevice) : validDevice;
			
			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				//bool isJoystick = IsInputJoystick(supportedDevice);
				//inputHorizontal = (isJoystick && invertJoystickAxis)? -inputAxis : inputAxis;
				inputHorizontal = inputAxis;
				break;
			}
		}
		
		bool inputLeft = IsMenuInputHorizontalLeft(inputHorizontal);
		return inputLeft;
	}
	
	public bool IsMenuInputLeftPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = true;
		float inputHorizontal = inputMgr.GetInputForAxis(_InputDevice, horizontalAxis);
		bool inputLeft = IsMenuInputHorizontalLeft(inputHorizontal);
		
		return inputLeft;
	}
	
	public bool IsMenuInputRightPressed()
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = true;
		//bool invertJoystickAxis = false;
		
		float inputHorizontal = 0.0f;
		
		InputDevice[] validDevices = inputMgr.GetValidInputDevices();
		foreach(InputDevice validDevice in validDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(validDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(validDevice) : validDevice;

			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				//bool isJoystick = IsInputJoystick(supportedDevice);
				//inputHorizontal = (isJoystick && invertJoystickAxis)? -inputAxis : inputAxis;
				inputHorizontal = inputAxis;
				break;
			}
		}
		
		bool inputRight = IsMenuInputHorizontalRight(inputHorizontal);
		return inputRight;
	}
	
	public bool IsMenuInputRightPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool horizontalAxis = true;
		float inputHorizontal = inputMgr.GetInputForAxis(_InputDevice, horizontalAxis);
		bool inputRight = IsMenuInputHorizontalRight(inputHorizontal);
		
		return inputRight;
	}

	
	public InputDevice GetPlayerInputDeviceFromValidateMenuInput()
	{
		InputManager inputMgr = m_InputManager;

		InputDevice playerInputDevice = InputDevice.E_InputDeviceNone;

		InputDevice[] validDevices = inputMgr.GetValidInputDevices();
		foreach(InputDevice validDevice in validDevices)
		{
			bool validateMenuPressed = IsMenuActionInputPressed(MenuActionType.E_MenuValidate, validDevice);
			if (validateMenuPressed)
			{
				playerInputDevice = validDevice;
				Debug.Log(string.Format("Player Input Device is '{0}'", inputMgr.GetInputDeviceName(playerInputDevice)));
				
				break;
			}
		}
		
		if (playerInputDevice == InputDevice.E_InputDeviceNone)
		{
			Debug.Log("Player is using unsupported Input Device to validate menu");
		}
		
		return playerInputDevice;
	}
	
	public static int ComputeNewListElementIndex(int _ListElementIndex, int _ListElementCount, bool _InputPrevious, bool _InputNext)
	{
		int elementIndex = _ListElementIndex;
		int elementCount = _ListElementCount;
		System.Diagnostics.Debug.Assert(elementCount > 0);
		
		if (_InputNext)
		{
			elementIndex = (elementIndex + 1) % elementCount;
		}
		
		if (_InputPrevious)
		{
			System.Diagnostics.Debug.Assert(elementIndex < elementCount);
			//@NOTE: modulo operator can return negative values
			elementIndex = (elementIndex == 0)? elementCount - 1 : elementIndex - 1;
		}
		
		return elementIndex;
	}
	
	public static int ComputeNewGridElementIndex(int _GridElementIndex, int _GridElementCount, int _GridColumnCount, bool _InputPreviousRow, bool _InputNextRow, bool _InputPreviousColumn, bool _InputNextColumn)
	{
		int elementIndex = _GridElementIndex;
		int elementCount = _GridElementCount;
		int columnCount = _GridColumnCount;
		int fullRowCount = elementCount / columnCount;
		bool hasNonFullRowCount = (elementCount > (fullRowCount * columnCount));
		int rowCount = (hasNonFullRowCount)? fullRowCount+1 : fullRowCount;
		
		int elementColumnIndex = elementIndex % columnCount;
		int elementRowIndex = elementIndex / columnCount;
		
		if (_InputPreviousRow)
		{
			System.Diagnostics.Debug.Assert(elementRowIndex < rowCount);
			
			elementRowIndex = (elementRowIndex == 0)? rowCount - 1 : elementRowIndex - 1;
			
			int elementIndexTemp = elementRowIndex * columnCount + elementColumnIndex;
			if (elementIndexTemp >= elementCount)	
			{
				//@NOTE: scrolled up past the last element
				// can happen if last row isn't full
				// so scrolling up to previous row
				elementRowIndex = elementRowIndex - 1;
			}
		}
		
		if (_InputNextRow)
		{
			elementRowIndex = (elementRowIndex + 1) % rowCount;
			int elementIndexTemp = elementRowIndex * columnCount + elementColumnIndex;
			if (elementIndexTemp >= elementCount)	
			{
				//@NOTE: scrolled down past the last element
				// can happen if last row isn't full
				// so scrolling back to first row
				elementRowIndex = 0;
			}
		}
		
		if (_InputPreviousColumn)
		{
			System.Diagnostics.Debug.Assert(elementColumnIndex < columnCount);
			
			elementColumnIndex = (elementColumnIndex == 0)? columnCount - 1 : elementColumnIndex - 1;
			
			int elementIndexTemp = elementRowIndex * columnCount + elementColumnIndex;
			if (elementIndexTemp >= elementCount)	
			{
				//@NOTE: scrolled left past the last element
				// can happen if last row isn't full
				// so scrolling left to last element
				int gridLastElementIndex = elementCount - 1;
				elementColumnIndex = gridLastElementIndex % columnCount;
			}
		}
		
		if (_InputNextColumn)
		{
			elementColumnIndex = (elementColumnIndex + 1) % columnCount;
			int elementIndexTemp = elementRowIndex * columnCount + elementColumnIndex;
			if (elementIndexTemp >= elementCount)	
			{
				//@NOTE: scrolled right past the last element
				// can happen if last row isn't full
				// so scrolling back to first column
				elementColumnIndex = 0;
			}
		}
		
		int newElementIndex = elementRowIndex * columnCount + elementColumnIndex;
		System.Diagnostics.Debug.Assert( (0 <= newElementIndex) && (newElementIndex < elementCount) );
		
		return newElementIndex;
	}
}

[System.Serializable]
public class MenuAction
{
	[SerializeField]
	private MenuActionType m_MenuActionType = MenuActionType.E_MenuActionNone;
	public MenuActionType GetMenuActionType() { return m_MenuActionType; }
	
	[SerializeField]
	private int m_MouseButtonIndexToPress = 0;
	[SerializeField]
	private MenuInputPressedEvent m_MouseButtonPressedEvent = MenuInputPressedEvent.E_MenuInputPressedOnDown;
	[SerializeField]
	private KeyCode m_KeyToPress = KeyCode.None;
	[SerializeField]
	private MenuInputPressedEvent m_KeyPressedEvent = MenuInputPressedEvent.E_MenuInputPressedOnDown;
	[SerializeField]
	private KeyCode m_AlternateKeyToPress = KeyCode.None;
	[SerializeField]
	private MenuInputPressedEvent m_AlternateKeyPressedEvent = MenuInputPressedEvent.E_MenuInputPressedOnDown;
	[SerializeField]
	private int m_JoystickButtonIndexToPress = 0;
	[SerializeField]
	private MenuInputPressedEvent m_JoystickButtonPressedEvent = MenuInputPressedEvent.E_MenuInputPressedOnDown;
	
	private KeyCode m_AnyJoystickButtonToPress = KeyCode.None;
	private KeyCode[] m_JoystickButtonToPress = null;
	
	public void InitMenuInputs(InputManager _InputManager)
	{
		//@TODO: add checks for m_KeyToPress and m_AlternateKeyToPress being valid keyboard keys
		
		string anyJoystickButtonNameBase = _InputManager.GetAnyJoystickButtonNameBase();
		
		string joystickButtonIndexString = m_JoystickButtonIndexToPress.ToString();
		
		string anyJoystickValidateButtonName = anyJoystickButtonNameBase + joystickButtonIndexString;
		KeyCode anyJoystickValidateButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), anyJoystickValidateButtonName);
		m_AnyJoystickButtonToPress = anyJoystickValidateButton;
		
		int joystickCount = _InputManager.GetSupportedJoystickCountMax();
		m_JoystickButtonToPress = new KeyCode[joystickCount];
		
		for (int joystickIndex = 0; joystickIndex < joystickCount; ++joystickIndex)
		{
			string joystickButtonNameBase = _InputManager.GetJoystickButtonNameBase(joystickIndex);
			
			string joystickValidateButtonName = joystickButtonNameBase + joystickButtonIndexString;
			KeyCode joystickValidateButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), joystickValidateButtonName);
			m_JoystickButtonToPress[joystickIndex] = joystickValidateButton;
		}
	}
	
	public bool IsInputPressed(InputManager _InputManager)
	{
		InputManager inputMgr = _InputManager;
		
		//bool supportMouse = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceMouse);
		bool supportKeyboard = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceKeyboard)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboard);
		
		bool supportKeyboardAlt = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceKeyboardAlternate)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
		
		bool supportJoystick = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_1)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_2)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_3)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_4);
		
		//@HACK: not including mouse clicks which will be handled automatically when clicking on Unity3d GUI button
		//@FIXME: need to find a proper solution for handling menu inputs in a common way for both mouse and other input devices
		
		//bool mouseClicked = supportMouse && IsInputPressedForMouse();
		bool keyboardPressed = (supportKeyboard && IsInputPressedForKeyboard())
			|| (supportKeyboardAlt && IsInputPressedForKeyboardAlternate());
		bool joystickPressed = supportJoystick && IsInputPressedForAnyJoystick();
		bool validateMenuPressed = /*mouseClicked ||*/ keyboardPressed || joystickPressed;
		
		return validateMenuPressed;
	}
	
	public bool IsInputPressed(InputManager _InputManager, InputDevice _InputDevice)
	{
		InputManager inputMgr = _InputManager;
		
		bool inputPressed = false;
		
		bool validDevice = inputMgr.IsInputDeviceValid(_InputDevice);
		if (validDevice)
		{
			switch (_InputDevice)
			{
			case InputDevice.E_InputDeviceMouse:
				inputPressed = IsInputPressedForMouse();
				break;
			case InputDevice.E_InputDeviceKeyboard:
				inputPressed = IsInputPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceKeyboardAlternate:
				inputPressed = IsInputPressedForKeyboardAlternate();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboard:
				inputPressed = IsInputPressedForMouse() || IsInputPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
				inputPressed = IsInputPressedForMouse() || IsInputPressedForKeyboardAlternate();
				break;
			case InputDevice.E_InputDeviceGamepad_1:
				inputPressed = IsInputPressedForJoystick(0);
				break;
			case InputDevice.E_InputDeviceGamepad_2:
				inputPressed = IsInputPressedForJoystick(1);
				break;
			case InputDevice.E_InputDeviceGamepad_3:
				inputPressed = IsInputPressedForJoystick(2);
				break;
			case InputDevice.E_InputDeviceGamepad_4:
				inputPressed = IsInputPressedForJoystick(3);
				break;
			case InputDevice.E_InputDeviceTouch:
				//@TODO
				break;
			}
		}
		
		return inputPressed;
	}
	
	private bool IsInputDownForMouse() { return Input.GetMouseButtonDown(m_MouseButtonIndexToPress); }
	private bool IsInputUpForMouse() { return Input.GetMouseButtonUp(m_MouseButtonIndexToPress); }
	private bool IsInputPressedForMouse()
	{
		bool pressedOnDown = (m_MouseButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnDown);
		bool pressedOnUp = (m_MouseButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnUp);
		bool mouseClicked = (pressedOnDown && IsInputDownForMouse()) || (pressedOnUp && IsInputUpForMouse());
		return mouseClicked;
	}
	
	private bool IsInputDownForKeyboard() { return Input.GetKeyDown(m_KeyToPress); }
	private bool IsInputUpForKeyboard() { return Input.GetKeyUp(m_KeyToPress); }
	private bool IsInputPressedForKeyboard()
	{
		bool pressedOnDown = (m_KeyPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnDown);
		bool pressedOnUp = (m_KeyPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnUp);
		bool keyboardPressed = (pressedOnDown && IsInputDownForKeyboard()) || (pressedOnUp && IsInputUpForKeyboard());
		return keyboardPressed;
	}
	
	private bool IsInputDownForKeyboardAlternate() { return Input.GetKeyDown(m_AlternateKeyToPress); }
	private bool IsInputUpForKeyboardAlternate() { return Input.GetKeyUp(m_AlternateKeyToPress); }
	private bool IsInputPressedForKeyboardAlternate()
	{
		bool pressedOnDown = (m_AlternateKeyPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnDown);
		bool pressedOnUp = (m_AlternateKeyPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnUp);
		bool keyboardPressed = (pressedOnDown && IsInputDownForKeyboardAlternate()) || (pressedOnUp && IsInputUpForKeyboardAlternate());
		return keyboardPressed;
	}
	
	private bool IsInputDownForAnyJoystick() { return Input.GetKeyDown(m_AnyJoystickButtonToPress); }
	private bool IsInputUpForAnyJoystick() { return Input.GetKeyUp(m_AnyJoystickButtonToPress); }
	private bool IsInputPressedForAnyJoystick()
	{
		bool pressedOnDown = (m_JoystickButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnDown);
		bool pressedOnUp = (m_JoystickButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnUp);
		bool joystickPressed = (pressedOnDown && IsInputDownForAnyJoystick()) || (pressedOnUp && IsInputUpForAnyJoystick());
		return joystickPressed;
	}
	
	private bool IsInputDownForJoystick(int _JoystickIndex)
	{
		KeyCode joystickKey = m_JoystickButtonToPress[_JoystickIndex];
		return Input.GetKeyDown(joystickKey);
	}
	private bool IsInputUpForJoystick(int _JoystickIndex)
	{
		KeyCode joystickKey = m_JoystickButtonToPress[_JoystickIndex];
		return Input.GetKeyUp(joystickKey);
	}
	private bool IsInputPressedForJoystick(int _JoystickIndex)
	{
		bool pressedOnDown = (m_JoystickButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnDown);
		bool pressedOnUp = (m_JoystickButtonPressedEvent == MenuInputPressedEvent.E_MenuInputPressedOnUp);
		bool joystickPressed = (pressedOnDown && IsInputDownForJoystick(_JoystickIndex)) || (pressedOnUp && IsInputUpForJoystick(_JoystickIndex));
		return joystickPressed;
	}
}

public delegate void MenuEventDelegate(MenuEvent _MenuEvent);

[System.Serializable]
public class MenuButton
{
	[SerializeField]
	private string m_ControlName = null;
	[SerializeField]
	private string m_LabelName = null;
	[SerializeField]
	private MenuEvent m_MenuEventToTrigger = MenuEvent.E_MenuEventNone;
	
	private MenuEventDelegate m_TriggerDelegate = null;
	
	public MenuButton(string _ControlName, string _LabelName, MenuEvent _MenuEventToTrigger, MenuEventDelegate _TriggerDelegate)
	{
		m_ControlName = _ControlName;
		m_LabelName = _LabelName;
		m_MenuEventToTrigger = _MenuEventToTrigger;
		m_TriggerDelegate = _TriggerDelegate;
	}
	
	public string GetControlName() { return m_ControlName; }
	public string GetLabelName() { return m_LabelName; }
	public MenuEvent GetMenuEventToTrigger() { return m_MenuEventToTrigger; }
	
	public void InitTriggerDelegate(MenuEventDelegate _TriggerDelegate) { m_TriggerDelegate = _TriggerDelegate; }
	public void TriggerDelegate() { m_TriggerDelegate(m_MenuEventToTrigger); }
}

[System.Serializable]
public class MenuPanel
{
	[SerializeField]
	public int m_PanelWidth = 0;
	[SerializeField]
	public int m_PanelHeight = 0;
	[SerializeField]
	public int m_PanelButtonWidth = 0;
	[SerializeField]
	public int m_PanelButtonHeight = 0;
	[SerializeField]
	public int m_PanelButtonInterspaceY = 0;
	[SerializeField]
	public string m_PanelTitle = "";
	[SerializeField]
	public int m_PanelTitleTextHeight = 0;
	
	[SerializeField]
	private MenuButton[] m_MenuButtons = null;
	
	public MenuButton[] GetMenuButtons() { return m_MenuButtons; }
}
