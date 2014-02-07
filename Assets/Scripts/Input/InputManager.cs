using UnityEngine;
using System.Collections;

public enum InputDevice
{
	E_InputDeviceNone,
	E_InputDeviceMouse,
	E_InputDeviceKeyboard,
	E_InputDeviceKeyboardAlternate,
	E_InputDeviceMouseAndKeyboard,
	E_InputDeviceMouseAndKeyboardAlternate,
	//@NOTE: number of gamepads should match InputManager::m_SupportedJoystickCountMax
	E_InputDeviceGamepad_1,
	E_InputDeviceGamepad_2,
	E_InputDeviceGamepad_3,
	E_InputDeviceGamepad_4,
	E_InputDeviceTouch,
};

public class InputManager : MonoBehaviour
{
	[SerializeField]
	private InputDevice[] m_SupportedInputDevices;
	public InputDevice[] GetSupportedInputDevices() { return m_SupportedInputDevices; }

	private InputDevice[] m_ValidInputDevices = null;
	public InputDevice[] GetValidInputDevices() { return m_ValidInputDevices; }

	private const int m_SupportedJoystickCountMax = 4;
	public int GetSupportedJoystickCountMax() { return m_SupportedJoystickCountMax; }

	public string GetJoystickButtonNameBase(int _JoystickIndex)
	{
		System.Diagnostics.Debug.Assert( (0 <= _JoystickIndex) && (_JoystickIndex < GetSupportedJoystickCountMax()) );

		//@NOTE: IMPORTANT - Unity3d joystick indices starts at 1
		int joystickNameStartIndex = 1;

		int joystickNameIndex = _JoystickIndex + joystickNameStartIndex;
		string joystickButtonNameBase = "Joystick" + joystickNameIndex.ToString() + "Button";

		return joystickButtonNameBase;
	}

	public string GetAnyJoystickButtonNameBase()
	{
		string joystickButtonNameBase = "JoystickButton";
		return joystickButtonNameBase;
	}

	public bool IsInputConnected(InputDevice _InputDevice)
	{
		bool inputConnected = false;

		//@HACK: this doesn't support devices connected / disconnected at run-time
		string[] connectedJoystickNames = Input.GetJoystickNames();
		int connectedJoystickCount = connectedJoystickNames.Length;

		//@FIXME: how to check if mouse / keyboard / touch are connected?
		switch (_InputDevice)
		{
		case InputDevice.E_InputDeviceMouse:
			inputConnected = true;
			break;
		case InputDevice.E_InputDeviceKeyboard:
			inputConnected = true;
			break;
		case InputDevice.E_InputDeviceKeyboardAlternate:
			inputConnected = true;
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboard:
			inputConnected = true;
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
			inputConnected = true;
			break;
		case InputDevice.E_InputDeviceGamepad_1:
			inputConnected = (connectedJoystickCount > 0);
			break;
		case InputDevice.E_InputDeviceGamepad_2:
			inputConnected = (connectedJoystickCount > 1);
			break;
		case InputDevice.E_InputDeviceGamepad_3:
			inputConnected = (connectedJoystickCount > 2);
			break;
		case InputDevice.E_InputDeviceGamepad_4:
			inputConnected = (connectedJoystickCount > 3);
			break;
		case InputDevice.E_InputDeviceTouch:
			inputConnected = true;
			break;
		}

		return inputConnected;
	}

	public bool IsInputDeviceValid(InputDevice _InputDevice)
	{
		InputDevice[] supportedDevices = GetSupportedInputDevices();

		bool valid = false;

		bool connected = IsInputConnected(_InputDevice);
		if (connected)
		{
			foreach(InputDevice supportedDevice in supportedDevices)
			{
				bool supported = IsInputIncludedIn(_InputDevice, supportedDevice);
				if (supported)
				{
					valid = true;
					break;
				}
			}
		}

		return valid;
	}

	public bool IsInputJoystick(InputDevice _InputDevice)
	{
		bool isJoystick = (_InputDevice == InputDevice.E_InputDeviceGamepad_1) || (_InputDevice == InputDevice.E_InputDeviceGamepad_2) ||
						(_InputDevice == InputDevice.E_InputDeviceGamepad_3) || (_InputDevice == InputDevice.E_InputDeviceGamepad_4);

		return isJoystick;
	}

	public bool IsInputMouseAndKeyboard(InputDevice _InputDevice)
	{
		bool isMouseAndKeyboard = (_InputDevice == InputDevice.E_InputDeviceMouseAndKeyboard) || (_InputDevice == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
		return isMouseAndKeyboard;
	}

	public InputDevice GetIncludedKeyboardInput(InputDevice _DeviceIncludingKeyboardInput)
	{
		InputDevice keyboardInput = InputDevice.E_InputDeviceNone;

		bool isMouseAndKeyboard = IsInputMouseAndKeyboard(_DeviceIncludingKeyboardInput);
		if (isMouseAndKeyboard)
		{
			keyboardInput = (_DeviceIncludingKeyboardInput == InputDevice.E_InputDeviceMouseAndKeyboard)? InputDevice.E_InputDeviceKeyboard : InputDevice.E_InputDeviceKeyboardAlternate;
		}
		else
		{
			Debug.Log("GetKeyboardInputIncluded() should be called only for Mouse+Keyboard input device.");
			Debug.Log(string.Format("'{0}' doesn't include a keyboard.", GetInputDeviceName(_DeviceIncludingKeyboardInput)));
		}

		return keyboardInput;
	}

	public bool AreInputConflicting(InputDevice _InputDevice1, InputDevice _InputDevice2)
	{
		bool areConflicting = false;

		switch (_InputDevice1)
		{
		case InputDevice.E_InputDeviceMouse:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceMouse)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboard)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceKeyboard:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceKeyboard)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboard);
			break;
		case InputDevice.E_InputDeviceKeyboardAlternate:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceKeyboardAlternate)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboard:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceMouse)
				|| (_InputDevice2 == InputDevice.E_InputDeviceKeyboard)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboard)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceMouse)
				|| (_InputDevice2 == InputDevice.E_InputDeviceKeyboardAlternate)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboard)
				|| (_InputDevice2 == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceGamepad_1:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceGamepad_1);
			break;
		case InputDevice.E_InputDeviceGamepad_2:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceGamepad_2);
			break;
		case InputDevice.E_InputDeviceGamepad_3:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceGamepad_3);
			break;
		case InputDevice.E_InputDeviceGamepad_4:
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceGamepad_4);
			break;
		case InputDevice.E_InputDeviceTouch:
			//@FIXME: this might not be always true since Touch devices could possibly support multiple users
			//also a local device might have multiple touch devices
			areConflicting = (_InputDevice2 == InputDevice.E_InputDeviceTouch);
			break;
		}

		return areConflicting;
	}

	public bool IsInputIncludedIn(InputDevice _InputDevice, InputDevice _IncludingInputDevice)
	{
		bool isIncluding = false;
		
		switch (_IncludingInputDevice)
		{
		case InputDevice.E_InputDeviceMouse:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceMouse);
			break;
		case InputDevice.E_InputDeviceKeyboard:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceKeyboard);
			break;
		case InputDevice.E_InputDeviceKeyboardAlternate:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboard:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceMouse)
				|| (_InputDevice == InputDevice.E_InputDeviceKeyboard)
				|| (_InputDevice == InputDevice.E_InputDeviceMouseAndKeyboard);
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceMouse)
				|| (_InputDevice == InputDevice.E_InputDeviceKeyboardAlternate)
				|| (_InputDevice == InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
			break;
		case InputDevice.E_InputDeviceGamepad_1:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceGamepad_1);
			break;
		case InputDevice.E_InputDeviceGamepad_2:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceGamepad_2);
			break;
		case InputDevice.E_InputDeviceGamepad_3:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceGamepad_3);
			break;
		case InputDevice.E_InputDeviceGamepad_4:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceGamepad_4);
			break;
		case InputDevice.E_InputDeviceTouch:
			isIncluding = (_InputDevice == InputDevice.E_InputDeviceTouch);
			break;
		}
		
		return isIncluding;
	}
	
	void Start()
	{
		string[] connectedJoystickNames = Input.GetJoystickNames();
		int joystickCount = connectedJoystickNames.Length;
		for (int joystickIndex = 0; joystickIndex < joystickCount; ++joystickIndex)
		{
			string joystickName = connectedJoystickNames[joystickIndex];
			Debug.Log(string.Format("Joystick {0}: {1}", joystickIndex, joystickName));
		}

		UpdateValidInputList();
	}
	
	void Update()
	{
	
	}

	private void UpdateValidInputList()
	{
		InputDevice[] supportedDevices = GetSupportedInputDevices();
		int validDeviceCount = 0;
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			if (IsInputDeviceValid(supportedDevice))
			{
				++validDeviceCount;
			}
		}

		Debug.Log("Updating valid input device list");

		m_ValidInputDevices = new InputDevice[validDeviceCount];
		int currentValidIndex = 0;
		foreach(InputDevice supportedDevice in supportedDevices)
		{
			if (IsInputDeviceValid(supportedDevice))
			{
				m_ValidInputDevices[currentValidIndex] = supportedDevice;
				Debug.Log(string.Format("Valid input device {0}: {1}", currentValidIndex, GetInputDeviceName(supportedDevice)));

				++currentValidIndex;
			}
		}
	}
	
	public string GetInputDeviceName(InputDevice _InputDevice)
	{
		string inputDeviceName = "";
		switch (_InputDevice)
		{
		case InputDevice.E_InputDeviceMouse:
			inputDeviceName = "Mouse";
			break;
		case InputDevice.E_InputDeviceKeyboard:
			inputDeviceName = "Keyboard";
			break;
		case InputDevice.E_InputDeviceKeyboardAlternate:
			inputDeviceName = "KeyboardAlt";
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboard:
			inputDeviceName = "Mouse Keyboard";
			break;
		case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
			inputDeviceName = "Mouse KeyboardAlt";
			break;
		case InputDevice.E_InputDeviceGamepad_1:
			inputDeviceName = "Joystick 1";
			break;
		case InputDevice.E_InputDeviceGamepad_2:
			inputDeviceName = "Joystick 2";
			break;
		case InputDevice.E_InputDeviceGamepad_3:
			inputDeviceName = "Joystick 3";
			break;
		case InputDevice.E_InputDeviceGamepad_4:
			inputDeviceName = "Joystick 4";
			break;
		case InputDevice.E_InputDeviceTouch:
			inputDeviceName = "Touch";
			//@TODO
			break;
		}
		
		return inputDeviceName;
	}
	
	public float GetInputForAxis(InputDevice _InputDevice, bool _UseHorizontalAxis)
	{
		float inputAxis = 0.0f;
		
		bool validDevice = IsInputDeviceValid(_InputDevice);
		if (validDevice)
		{
			string inputDeviceName = GetInputDeviceName(_InputDevice);
			string inputAxisName = _UseHorizontalAxis? "Horizontal " : "Vertical ";
			inputAxisName += inputDeviceName;
			
			switch (_InputDevice)
			{
			case InputDevice.E_InputDeviceMouse:
				inputAxisName = _UseHorizontalAxis? "Mouse X" : "Mouse Y";
				inputAxis = Input.GetAxis(inputAxisName);
				break;
			case InputDevice.E_InputDeviceKeyboard:
				inputAxis = Input.GetAxis(inputAxisName);
				break;
			case InputDevice.E_InputDeviceKeyboardAlternate:
				inputAxis = Input.GetAxis(inputAxisName);
				break;
			case InputDevice.E_InputDeviceMouseAndKeyboard:
			case InputDevice.E_InputDeviceMouseAndKeyboardAlternate:
				//@NOTE: GetInputForAxis() for "Mouse & Keyboard" are ambiguous
				Debug.Log("GetInputForAxis() calls should be specified seperately either to Mouse or Keyboard/Alt");
				break;
			case InputDevice.E_InputDeviceGamepad_1:
			case InputDevice.E_InputDeviceGamepad_2:
			case InputDevice.E_InputDeviceGamepad_3:
			case InputDevice.E_InputDeviceGamepad_4:
				inputAxis = Input.GetAxis(inputAxisName);
				break;
			case InputDevice.E_InputDeviceTouch:
				//@TODO: handle gesture or don't support menu input down??
				break;
			}
		}

		return inputAxis;
	}
	
	public bool IsMouseMoved()
	{
		bool supportMouse = IsInputDeviceValid(InputDevice.E_InputDeviceMouse)
			|| IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboard)
			|| IsInputDeviceValid(InputDevice.E_InputDeviceMouseAndKeyboardAlternate);
		bool mouseMoved =  supportMouse && ((Input.GetAxis("Mouse X") != 0.0f) || (Input.GetAxis("Mouse Y") != 0.0f));
		return mouseMoved;
	}
}

