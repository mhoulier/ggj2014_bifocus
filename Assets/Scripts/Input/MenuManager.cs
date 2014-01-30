using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
	private InputManager m_InputManager = null;

	[SerializeField]
	private KeyCode m_KeyToPressToValidateMenu = KeyCode.Return;
	[SerializeField]
	private KeyCode m_AlternateKeyToPressToValidateMenu = KeyCode.Space;
	[SerializeField]
	private int m_JoystickButtonIndexToValidateMenu = 0;
	
	[SerializeField]
	private KeyCode m_KeyToPressToCancelMenu = KeyCode.Escape;
	[SerializeField]
	private int m_JoystickButtonIndexToCancelMenu = 1;

	[SerializeField]
	private KeyCode m_KeyToPressToPauseGame = KeyCode.Space;
	[SerializeField]
	private int m_JoystickButtonIndexToPauseGame = 1;
	
	[SerializeField]
	private float m_MenuInputAxisLatency = 0.0f;
	[SerializeField]
	private float m_MenuInputAxisThreshold = 0.0f;
	
	//@NOTE: IMPORTANT - Unity3d joystick Y-Axis return POSITIVE values when pressed DOWN
	//Inverting vertical menu input by default
	[SerializeField]
	private bool m_MenuInputVerticalAxisJoystickInvert = true;
	
	public float GetMenuInputAxisLatency() { return m_MenuInputAxisLatency; }

	private KeyCode m_AnyJoystickButtonToValidateMenu = KeyCode.None;
	private KeyCode[] m_JoystickButtonToValidateMenu = null;
	private KeyCode m_AnyJoystickButtonToCancelMenu = KeyCode.None;
	private KeyCode[] m_JoystickButtonToCancelMenu = null;
	private KeyCode m_AnyJoystickButtonToPauseGame = KeyCode.None;
	private KeyCode[] m_JoystickButtonToPauseGame = null;

	void Start()
	{
		bool menuInputInitialized = false;

		InputManager inputManager = (InputManager)FindObjectOfType( typeof(InputManager) );
		if (inputManager != null)
		{
			InitMenuInput(inputManager);

			m_InputManager = inputManager;
			menuInputInitialized = true;
		}
		else
		{
			Debug.Log("Scene is missing an instanced game object with an InputManager component!");
		}

		enabled = menuInputInitialized;
	}
	
	private void InitMenuInput(InputManager _InputManager)
	{
		string anyJoystickButtonNameBase = _InputManager.GetAnyJoystickButtonNameBase();

		string joystickValidateButtonIndexString = m_JoystickButtonIndexToValidateMenu.ToString();
		
		string anyJoystickValidateButtonName = anyJoystickButtonNameBase + joystickValidateButtonIndexString;
		KeyCode anyJoystickValidateButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), anyJoystickValidateButtonName);
		m_AnyJoystickButtonToValidateMenu = anyJoystickValidateButton;
		
		string joystickCancelButtonIndexString = m_JoystickButtonIndexToCancelMenu.ToString();
		
		string anyJoystickCancelButtonName = anyJoystickButtonNameBase + joystickCancelButtonIndexString;
		KeyCode anyJoystickCancelButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), anyJoystickCancelButtonName);
		m_AnyJoystickButtonToCancelMenu = anyJoystickCancelButton;

		string joystickPauseButtonIndexString = m_JoystickButtonIndexToPauseGame.ToString();
		
		string anyJoystickPauseButtonName = anyJoystickButtonNameBase + joystickPauseButtonIndexString;
		KeyCode anyJoystickPauseButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), anyJoystickPauseButtonName);
		m_AnyJoystickButtonToPauseGame = anyJoystickPauseButton;
		
		int joystickCount = _InputManager.GetSupportedJoystickCount();
		m_JoystickButtonToValidateMenu = new KeyCode[joystickCount];
		m_JoystickButtonToCancelMenu = new KeyCode[joystickCount];
		m_JoystickButtonToPauseGame = new KeyCode[joystickCount];
		
		for (int joystickIndex = 0; joystickIndex < joystickCount; ++joystickIndex)
		{
			string joystickButtonNameBase = _InputManager.GetJoystickButtonNameBase(joystickIndex);
			
			string joystickValidateButtonName = joystickButtonNameBase + joystickValidateButtonIndexString;
			KeyCode joystickValidateButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), joystickValidateButtonName);
			m_JoystickButtonToValidateMenu[joystickIndex] = joystickValidateButton;
			
			string joystickCancelButtonName = joystickButtonNameBase + joystickCancelButtonIndexString;
			KeyCode joystickCancelButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), joystickCancelButtonName);
			m_JoystickButtonToCancelMenu[joystickIndex] = joystickCancelButton;

			string joystickPauseButtonName = joystickButtonNameBase + joystickPauseButtonIndexString;
			KeyCode joystickPauseButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), joystickPauseButtonName);
			m_JoystickButtonToPauseGame[joystickIndex] = joystickPauseButton;
		}
	}

	void Update()
	{
	
	}

	public bool IsValidateMenuPressed()
	{
		InputManager inputMgr = m_InputManager;

		//bool supportMouse = IsInputDeviceValid(InputDevice.E_InputDeviceMouse);
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
		
		//bool mouseClicked = supportMouse && IsValidateMenuPressedForMouse();
		bool keyboardPressed = (supportKeyboard && IsValidateMenuPressedForKeyboard())
			|| (supportKeyboardAlt && IsValidateMenuPressedForKeyboardAlternate());
		bool joystickPressed = supportJoystick && IsValidateMenuPressedForAnyJoystick();
		bool validateMenuPressed = /*mouseClicked ||*/ keyboardPressed || joystickPressed;
		
		return validateMenuPressed;
	}
	
	public bool IsValidateMenuPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool validateMenuPressed = false;

		bool validDevice = inputMgr.IsInputDeviceValid(_InputDevice);
		if (validDevice)
		{
			switch (_InputDevice)
			{
			case InputDevice.E_InputDeviceMouse:
				validateMenuPressed = IsValidateMenuPressedForMouse();
				break;
			case InputDevice.E_InputDeviceKeyboard:
				validateMenuPressed = IsValidateMenuPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceKeyboardAlternate:
				validateMenuPressed = IsValidateMenuPressedForKeyboardAlternate();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboard:
				validateMenuPressed = IsValidateMenuPressedForMouse() || IsValidateMenuPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
				validateMenuPressed = IsValidateMenuPressedForMouse() || IsValidateMenuPressedForKeyboardAlternate();
				break;
			case InputDevice.E_InputDeviceGamepad_1:
				validateMenuPressed = IsValidateMenuPressedForJoystick(0);
				break;
			case InputDevice.E_InputDeviceGamepad_2:
				validateMenuPressed = IsValidateMenuPressedForJoystick(1);
				break;
			case InputDevice.E_InputDeviceGamepad_3:
				validateMenuPressed = IsValidateMenuPressedForJoystick(2);
				break;
			case InputDevice.E_InputDeviceGamepad_4:
				validateMenuPressed = IsValidateMenuPressedForJoystick(3);
				break;	
			case InputDevice.E_InputDeviceTouch:
				//@TODO
				break;
			}
		}
		
		return validateMenuPressed;
	}
	
	private bool IsValidateMenuPressedForMouse()
	{
		bool mouseClicked = Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2);
		return mouseClicked;
	}
	
	private bool IsValidateMenuPressedForKeyboard()
	{
		bool keyboardPressed = Input.GetKeyDown(m_KeyToPressToValidateMenu);
		return keyboardPressed;
	}
	
	private bool IsValidateMenuPressedForKeyboardAlternate()
	{
		bool keyboardPressed = Input.GetKeyDown(m_AlternateKeyToPressToValidateMenu);
		return keyboardPressed;
	}
	
	private bool IsValidateMenuPressedForAnyJoystick()
	{
		bool joystickPressed = Input.GetKeyDown(m_AnyJoystickButtonToValidateMenu);
		return joystickPressed;
	}
	
	private bool IsValidateMenuPressedForJoystick(int _JoystickIndex)
	{
		KeyCode joystickKey = m_JoystickButtonToValidateMenu[_JoystickIndex];
		bool joystickPressed = Input.GetKeyDown(joystickKey);
		return joystickPressed;
	}
	
	public bool IsCancelMenuPressed()
	{
		InputManager inputMgr = m_InputManager;

		//bool supportMouse = IsInputDeviceValid(InputDevice.E_InputDeviceMouse);
		bool supportKeyboard = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceKeyboard)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboard);
		//bool supportKeyboardAlt = IsInputDeviceValid(InputDevice.E_InputDeviceKeyboardAlternate);
		bool supportJoystick = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_1)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_2)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_3)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_4);
		
		//@HACK: not including mouse clicks which will be handled automatically when clicking on Unity3d GUI button
		//@FIXME: need to find a proper solution for handling menu inputs in a common way for both mouse and other input devices
		//@TODO: add alternate keyboard cancel key?
		
		//bool mouseClicked = supportMouse && IsCancelMenuPressedForMouse();
		bool keyboardPressed = supportKeyboard && IsCancelMenuPressedForKeyboard();
		bool joystickPressed = supportJoystick && IsCancelMenuPressedForAnyJoystick();
		bool cancelMenuPressed = /*mouseClicked ||*/ keyboardPressed || joystickPressed;
		
		return cancelMenuPressed;
	}
	
	public bool IsCancelMenuPressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;

		bool cancelMenuPressed = false;
		
		bool validDevice = inputMgr.IsInputDeviceValid(_InputDevice);	
		if (validDevice)
		{
			switch (_InputDevice)
			{
			case InputDevice.E_InputDeviceMouse:
				//@TODO: do we want cancel button on mouse?
				break;
			case InputDevice.E_InputDeviceKeyboard:
				cancelMenuPressed = IsCancelMenuPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceKeyboardAlternate:
				//@TODO: should match E_InputDeviceMouseAndKeyboardAlternate
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboard:
				cancelMenuPressed = IsCancelMenuPressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
				//@TODO: should match E_InputDeviceKeyboardAlternate
				break;
			case InputDevice.E_InputDeviceGamepad_1:
				cancelMenuPressed = IsCancelMenuPressedForJoystick(0);
				break;
			case InputDevice.E_InputDeviceGamepad_2:
				cancelMenuPressed = IsCancelMenuPressedForJoystick(1);
				break;
			case InputDevice.E_InputDeviceGamepad_3:
				cancelMenuPressed = IsCancelMenuPressedForJoystick(2);
				break;
			case InputDevice.E_InputDeviceGamepad_4:
				cancelMenuPressed = IsCancelMenuPressedForJoystick(3);
				break;	
			case InputDevice.E_InputDeviceTouch:
				//@TODO
				break;
			}
		}
		
		return cancelMenuPressed;
	}
	
	private bool IsCancelMenuPressedForKeyboard()
	{
		bool keyboardPressed = Input.GetKeyDown(m_KeyToPressToCancelMenu);
		return keyboardPressed;
	}
	
	private bool IsCancelMenuPressedForAnyJoystick()
	{
		bool joystickPressed = Input.GetKeyDown(m_AnyJoystickButtonToCancelMenu);
		return joystickPressed;
	}
	
	private bool IsCancelMenuPressedForJoystick(int _JoystickIndex)
	{
		KeyCode joystickKey = m_JoystickButtonToCancelMenu[_JoystickIndex];
		bool joystickPressed = Input.GetKeyDown(joystickKey);
		return joystickPressed;
	}
/*
	public bool IsPauseGamePressed()
	{
		InputManager inputMgr = m_InputManager;
		
		//bool supportMouse = IsInputDeviceValid(InputDevice.E_InputDeviceMouse);
		bool supportKeyboard = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceKeyboard)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboard);
		//bool supportKeyboardAlt = IsInputDeviceValid(InputDevice.E_InputDeviceKeyboardAlternate);
		bool supportJoystick = inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_1)
			|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_2)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_3)
				|| inputMgr.IsInputDeviceValid(InputDevice.E_InputDeviceGamepad_4);
		
		//@HACK: not including mouse clicks which will be handled automatically when clicking on Unity3d GUI button
		//@FIXME: need to find a proper solution for handling menu inputs in a common way for both mouse and other input devices
		//@TODO: add alternate keyboard cancel key?
		
		//bool mouseClicked = supportMouse && IsCancelMenuPressedForMouse();
		bool keyboardPressed = supportKeyboard && IsCancelMenuPressedForKeyboard();
		bool joystickPressed = supportJoystick && IsCancelMenuPressedForAnyJoystick();
		bool cancelMenuPressed = mouseClicked || keyboardPressed || joystickPressed;
		
		return cancelMenuPressed;
	}
*/	
	public bool IsPauseGamePressed(InputDevice _InputDevice)
	{
		InputManager inputMgr = m_InputManager;
		
		bool pauseGamePressed = false;
		
		bool validDevice = inputMgr.IsInputDeviceValid(_InputDevice);	
		if (validDevice)
		{
			switch (_InputDevice)
			{
			case InputDevice.E_InputDeviceMouse:
				//@TODO: do we want cancel button on mouse?
				break;
			case InputDevice.E_InputDeviceKeyboard:
				pauseGamePressed = IsPauseGamePressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceKeyboardAlternate:
				//@TODO: should match E_InputDeviceMouseAndKeyboardAlternate
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboard:
				pauseGamePressed = IsPauseGamePressedForKeyboard();
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
				//@TODO: should match E_InputDeviceKeyboardAlternate
				break;
			case InputDevice.E_InputDeviceGamepad_1:
				pauseGamePressed = IsPauseGamePressedForJoystick(0);
				break;
			case InputDevice.E_InputDeviceGamepad_2:
				pauseGamePressed = IsPauseGamePressedForJoystick(1);
				break;
			case InputDevice.E_InputDeviceGamepad_3:
				pauseGamePressed = IsPauseGamePressedForJoystick(2);
				break;
			case InputDevice.E_InputDeviceGamepad_4:
				pauseGamePressed = IsPauseGamePressedForJoystick(3);
				break;	
			case InputDevice.E_InputDeviceTouch:
				//@TODO
				break;
			}
		}
		
		return pauseGamePressed;
	}
	
	private bool IsPauseGamePressedForKeyboard()
	{
		bool keyboardPressed = Input.GetKeyDown(m_KeyToPressToPauseGame);
		return keyboardPressed;
	}
	
	private bool IsPauseGamePressedForAnyJoystick()
	{
		bool joystickPressed = Input.GetKeyDown(m_AnyJoystickButtonToPauseGame);
		return joystickPressed;
	}
	
	private bool IsPauseGamePressedForJoystick(int _JoystickIndex)
	{
		KeyCode joystickKey = m_JoystickButtonToPauseGame[_JoystickIndex];
		bool joystickPressed = Input.GetKeyDown(joystickKey);
		return joystickPressed;
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

		InputDevice[] supportedDevices = inputMgr.GetSupportedInputDevices();
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(supportedDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(supportedDevice) : supportedDevice;
			
			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				bool isJoystick = inputMgr.IsInputJoystick(supportedDevice);
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
		
		InputDevice[] supportedDevices = inputMgr.GetSupportedInputDevices();
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(supportedDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(supportedDevice) : supportedDevice;
			
			float inputAxis = inputMgr.GetInputForAxis(menuInputDevice, horizontalAxis);
			if (inputAxis != 0.0f)
			{
				bool isJoystick = inputMgr.IsInputJoystick(supportedDevice);
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
		
		InputDevice[] supportedDevices = inputMgr.GetSupportedInputDevices();
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(supportedDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(supportedDevice) : supportedDevice;
			
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
		
		InputDevice[] supportedDevices = inputMgr.GetSupportedInputDevices();
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			bool isMouseAndKeyboard = inputMgr.IsInputMouseAndKeyboard(supportedDevice);
			InputDevice menuInputDevice = (isMouseAndKeyboard)? inputMgr.GetIncludedKeyboardInput(supportedDevice) : supportedDevice;

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

		InputDevice[] supportedDevices = inputMgr.GetSupportedInputDevices();
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			bool validateMenuPressed = IsValidateMenuPressed(supportedDevice);
			if (validateMenuPressed)
			{
				playerInputDevice = supportedDevice;
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
	
	public int ComputeNewListElementIndex(int _ListElementIndex, int _ListElementCount, bool _InputPrevious, bool _InputNext)
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
	
	public int ComputeNewGridElementIndex(int _GridElementIndex, int _GridElementCount, int _GridColumnCount, bool _InputPreviousRow, bool _InputNextRow, bool _InputPreviousColumn, bool _InputNextColumn)
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
