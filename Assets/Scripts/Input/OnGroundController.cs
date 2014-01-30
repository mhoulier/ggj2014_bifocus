using UnityEngine;
using System.Collections;

public class OnGroundController : MonoBehaviour
{
	private InputManager m_InputManager = null;
	private InputDevice m_ControllerInput = InputDevice.E_InputDeviceNone;

	public void SetControllerInput(InputDevice _InputDevice) { m_ControllerInput = _InputDevice; }

	[SerializeField]
	private bool m_InvertForwardAxisForJoystick = false;
//	[SerializeField]
//	private bool m_InvertUpAxisForMouseAndJoystick = false;

	[SerializeField]
	private float m_ForwardSpeed = 10.0f;
	[SerializeField]
	private float m_RotationSpeed = 100.0f;

	[SerializeField]
	private float m_MouseSensitivityX = 15.0f;
//	[SerializeField]
//	private float m_MouseSensitivityY = 15.0f;
//	[SerializeField]
//	private float m_MouseMinimumY = -60.0f;
//	[SerializeField]
//	private float m_MouseMaximumY = 60.0f;

//	private float m_MouseRotationY = 0.0f;

	private CharacterController m_CharacterController = null;
	
	//private Vector3 m_LookDirection = Vector3.forward;
	//public Vector3 GetLookDirection() { return m_LookDirection; }

	
	// Use this for initialization
	void Start()
	{
		m_CharacterController = GetComponent<CharacterController>();

		InitInput ();
	}

	private void InitInput()
	{
		InputManager inputManager = (InputManager)FindObjectOfType( typeof(InputManager) );
		if (inputManager == null)
		{
			Debug.Log("GameManager prefab is missing a InputManager component!");
		}

		m_InputManager = inputManager;
	}
	
	// Update is called once per frame
	void Update()
	{
		float deltaTime = Time.deltaTime;

		if (deltaTime > 0.0f)
		{
			UpdateInput(deltaTime);
		}
	}

	private void UpdateInput(float _DeltaTime)
	{
		bool isMouseAndKeyboard = m_InputManager.IsInputMouseAndKeyboard(m_ControllerInput);
		bool isJoystick = m_InputManager.IsInputJoystick(m_ControllerInput);

		float rotationInput = 0.0f;
		bool useHorizontal = true;

		float forwardInput = 0.0f;
		bool invertForward = isJoystick && m_InvertForwardAxisForJoystick;

		if (isMouseAndKeyboard)
		{
			float horizontalInput = m_InputManager.GetInputForAxis(InputDevice.E_InputDeviceMouse, useHorizontal);
			//Debug.Log(string.Format("Ground Character Mouse Input h: {0}", horizontalInput));

			//float verticalInput = m_InputManager.GetInputForAxis(m_ControllerInput, !useHorizontal);
			//verticalInput = (m_InvertVerticalAxis)? -verticalInput : verticalInput;

			//m_MouseRotationY += verticalInput * m_MouseSensitivityY;
			//m_MouseRotationY = Mathf.Clamp (m_MouseRotationY, m_MouseMinimumY, m_MouseMaximumY);
			
			//transform.localEulerAngles = new Vector3(-m_MouseRotationY, transform.localEulerAngles.y, 0);

			rotationInput = horizontalInput * m_MouseSensitivityX;
		}
		else
		{
			float horizontalInput = m_InputManager.GetInputForAxis(m_ControllerInput, useHorizontal);
			//Debug.Log(string.Format("Ground Character Input h: {0}", horizontalInput));

			rotationInput = horizontalInput;
		}

		if (isMouseAndKeyboard)
		{
			InputDevice keyboardInputDevice = m_InputManager.GetIncludedKeyboardInput(m_ControllerInput);

			forwardInput = m_InputManager.GetInputForAxis(keyboardInputDevice, !useHorizontal);
			//Debug.Log(string.Format("Ground Character Keyboard Input v: {0}", forwardInput));
		}
		else
		{
			forwardInput = m_InputManager.GetInputForAxis(m_ControllerInput, !useHorizontal);
			//Debug.Log(string.Format("Ground Character Input v: {0}", forwardInput));
		}
		forwardInput = (invertForward)? -forwardInput : forwardInput;

		float rotation = rotationInput * m_RotationSpeed * _DeltaTime;
		//Vector3 prevForward = transform.forward;
		transform.Rotate(0.0f, rotation, 0.0f);

		float forwardDistance = forwardInput * m_ForwardSpeed * _DeltaTime;
		Vector3 move = forwardDistance * transform.forward;
		m_CharacterController.Move(move);
	}
}
