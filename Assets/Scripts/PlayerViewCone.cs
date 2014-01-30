using UnityEngine;
using System.Collections;

public class PlayerViewCone : MonoBehaviour
{
	[SerializeField]
	private GameObject m_ViewConePrefab = null;

	[SerializeField]
	private Vector3 m_PlayerViewOriginOffset = Vector3.zero;
	
	public Transform GetOriginTransform() { return m_ViewCone.transform; }

	private GameObject m_ViewCone = null;

	// Use this for initialization
	void Start()
	{
		if (m_ViewConePrefab != null)
		{
			Vector3 originPos = transform.position + gameObject.transform.TransformDirection(m_PlayerViewOriginOffset);
			Quaternion originRot = transform.rotation;
			m_ViewCone = Instantiate(m_ViewConePrefab, originPos, originRot) as GameObject;
			m_ViewCone.transform.parent = gameObject.transform;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}
}
