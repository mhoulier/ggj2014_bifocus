using UnityEngine;
using System.Collections;

public class HighUpController : MonoBehaviour
{
	private InputManager m_InputManager = null;
	private InputDevice m_ControllerInput = InputDevice.E_InputDeviceNone;

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

	private CharacterController m_CharacterController = null;

//	private float m_InputForward = 0.0f;
//	private float m_InputRotationY = 0.0f;

	public void SetControllerInput(InputDevice _InputDevice) { m_ControllerInput = _InputDevice; }
	
	// Use this for initialization
	void Start()
	{
		InitInput ();

		m_CharacterController = GetComponent<CharacterController>();
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
			UpdateInputs(deltaTime);
		}
	}

	private void UpdateInputs(float _DeltaTime)
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
			//Debug.Log(string.Format("HighUp Character Mouse Input h: {0}", horizontalInput));
			
			rotationInput = horizontalInput * m_MouseSensitivityX;
		}
		else
		{
			float horizontalInput = m_InputManager.GetInputForAxis(m_ControllerInput, useHorizontal);
			//Debug.Log(string.Format("HighUp Character Input h: {0}", horizontalInput));
			
			rotationInput = horizontalInput;
		}
		
		if (isMouseAndKeyboard)
		{
			InputDevice keyboardInputDevice = m_InputManager.GetIncludedKeyboardInput(m_ControllerInput);
			
			forwardInput = m_InputManager.GetInputForAxis(keyboardInputDevice, !useHorizontal);
			Debug.Log(string.Format("HighUp Character Keyboard Input v: {0}", forwardInput));
		}
		else
		{
			forwardInput = m_InputManager.GetInputForAxis(m_ControllerInput, !useHorizontal);
			//Debug.Log(string.Format("HighUp Character Input v: {0}", forwardInput));
		}
		forwardInput = (invertForward)? -forwardInput : forwardInput;
		
		float rotation = rotationInput * m_RotationSpeed * _DeltaTime;
		//Vector3 prevForward = transform.forward;
		transform.Rotate(0.0f, rotation, 0.0f);
		
		float forwardDistance = forwardInput * m_ForwardSpeed * _DeltaTime;
		Vector3 move = forwardDistance * transform.forward;
		m_CharacterController.Move(move);
		
		//m_InputRotationY = rotationInput;
		//m_InputForward = forwardInput;
	}
/*	
	void FixedUpdate()
	{
		float deltaTime = Time.fixedDeltaTime;
		if (deltaTime >= 0.0f)
		{
			float forwardSpeed = m_InputForward * m_ForwardSpeed;
			Vector3 velocity = forwardSpeed * transform.forward;
			rigidbody.velocity = velocity;

			float rotationSpeedY = m_InputRotationY * m_RotationSpeed;
			Vector3 angularVelocity = new Vector3 (0.0f, rotationSpeedY, 0.0f);
			rigidbody.angularVelocity = angularVelocity;

			//rigidbody.AddTorque(new Vector3(0.0f, movement.x, 0.0f));
		}
	}
	*/
}
