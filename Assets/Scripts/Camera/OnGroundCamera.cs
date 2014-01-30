using UnityEngine;
using System.Collections;

public class OnGroundCamera : MonoBehaviour
{
	private Vector3 m_OriginOffset = Vector3.zero;
	//private bool m_UseFollowPositionMask = false;
	//private Vector3 m_FollowPositionMask = Vector3.forward;
	
	private Transform m_FollowTransform = null;
	
	void Start()
	{
		//SetFollowTarget(GameObject.FindWithTag("Player").transform);
		enabled = (m_FollowTransform != null);
	}
	
	public void SetFollowOrigin(Transform _FollowOrigin, Vector3 _OriginOffset)
	{
		m_FollowTransform = _FollowOrigin;
		m_OriginOffset = _OriginOffset;

		enabled = (_FollowOrigin != null);
	}
	
	void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (deltaTime > 0.0f)
		{
			UpdateCamera();
		}
	}

	private void UpdateCamera()
	{
		// make sure the camera is looking the right way!
		transform.rotation = m_FollowTransform.rotation;
		
		//Vector3 followPos = (m_UseFollowPositionMask)? Vector3.Scale(m_FollowTransform.position, m_FollowPositionMask) : m_FollowTransform.position;
		transform.position = m_FollowTransform.position + transform.TransformDirection(m_OriginOffset);
	}
}
