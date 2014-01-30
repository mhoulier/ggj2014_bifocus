using UnityEngine;
using System.Collections;

public class HighUpCamera : MonoBehaviour
{
	public float m_DistanceAway = 0.0f;    // distance from the back of the craft
	public float m_DistanceUp = 0.0f;      // distance above the craft
	public float m_SmoothFactor = 1.0f;    // how smooth the camera movement is
	
	public bool m_UseLookDir = false;
	public bool m_UseFollowPositionMask = false;
	public Vector3 m_LookDir = Vector3.forward;
	public Vector3 m_FollowPositionMask = Vector3.forward;
	
	private Vector3 m_TargetPosition;   // the position the camera is trying to be in
	
	private Transform m_FollowTransform = null;
	
	void Start()
	{
		//SetFollowTarget(GameObject.FindWithTag("Player").transform);
		enabled = (m_FollowTransform != null);
	}
	
	public void SetFollowTarget(Transform _FollowTarget)
	{
		m_FollowTransform = _FollowTarget;
		enabled = (_FollowTarget != null);
	}
	
	void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (deltaTime > 0.0f)
		{
			UpdateCamera(deltaTime);
		}
	}

	private void UpdateCamera(float _DeltaTime)
	{
		// make sure the camera is looking the right way!
		if (m_UseLookDir)
		{
			transform.rotation = Quaternion.LookRotation(m_LookDir, m_FollowTransform.forward);
		}
		else
		{
			transform.LookAt(m_FollowTransform);
		}
		
		Vector3 lookDir = (m_UseLookDir)? m_LookDir : m_FollowTransform.forward;
		Vector3 followPos = (m_UseFollowPositionMask)? Vector3.Scale(m_FollowTransform.position, m_FollowPositionMask) : m_FollowTransform.position;
		
		// setting the target position to be the correct offset from the hovercraft
		m_TargetPosition = followPos + m_DistanceUp * Vector3.up  - m_DistanceAway * lookDir;
		
		// making a smooth transition between it's current position and the position it wants to be in
		transform.position = Vector3.Lerp(transform.position, m_TargetPosition, _DeltaTime * m_SmoothFactor);
	}
}
