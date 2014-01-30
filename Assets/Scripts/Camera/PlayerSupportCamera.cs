using UnityEngine;
using System.Collections;

public class PlayerSupportCamera : MonoBehaviour
{
	[SerializeField]
	private PlayerClass m_SupportedPlayerClass = PlayerClass.E_ClassNone;

	[SerializeField]
	private Vector3 m_CameraOriginOffset = Vector3.zero;
	
	public Vector3 GetCameraOriginOffset() { return m_CameraOriginOffset; }

	public bool IsPlayerSupported(PlayerClass _PlayerClass)
	{
		bool isSupported = (m_SupportedPlayerClass == _PlayerClass);
		return isSupported;
	}

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}
}
