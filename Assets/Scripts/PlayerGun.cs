//#define DEBUG_FIRE_GUN_RAYCAST

using UnityEngine;
using System.Collections;

public enum CharacterActionType {
	E_ActionNone,
	E_ActionFireGun,
};

public class PlayerGun : MonoBehaviour
{
	[System.Serializable]
	public class PlayerAction
	{
		public CharacterActionType m_ActionType = CharacterActionType.E_ActionFireGun;
		public string m_ActionButton;
		public bool m_ActionOnButtonDown = true;
		public bool m_ActionOnButtonUp = false;
		public bool m_ActionOnButtonHold = false;
	}
	
	[SerializeField]
	private PlayerAction m_FireGunAction = null;

	[SerializeField]
	private string m_FireGunAnimTrigger = null;

	[SerializeField]
	private GameObject m_GunPrefab = null;
	
	[SerializeField]
	private Vector3 m_OriginOffset = Vector3.zero;
//	[SerializeField]
//	private Quaternion m_OriginRotation = Quaternion.identity;

	[SerializeField]
	private float m_FireGunRange = 0.0f;

	[SerializeField]
	private AudioClip m_FireGunSound;

	private GameObject m_Gun = null;
	private Animator m_GunAnimator = null;

	private Transform m_PlayerViewOrigin = null;
	
#if DEBUG_FIRE_GUN_RAYCAST
	//Debug raycast
	[SerializeField]
	private float m_RayLifeTime = 30.0f;
	[SerializeField]
	private int m_DebugRayBacklogSize = 20;

	private int m_LastDebugRayBacklogIndex = -1;

	private class RayBacklogEntry
	{
		public Vector3 rayStart = Vector3.zero;
		public Vector3 rayDir = Vector3.zero;
		public float rayLength = 0.0f;
		public float lifeTime = 0.0f;
		public bool hasHit = false;
		public bool hasHitNPC = false;
	}
	
	private RayBacklogEntry[] m_DebugRayBacklog = null;
#endif

	// Use this for initialization
	void Start()
	{
		if (m_GunPrefab != null)
		{
			Vector3 originPos = transform.position + gameObject.transform.TransformDirection(m_OriginOffset);
			Quaternion originRot = transform.rotation;
			m_Gun = Instantiate(m_GunPrefab, originPos, originRot) as GameObject;
			m_Gun.transform.parent = gameObject.transform;

			m_GunAnimator = m_Gun.GetComponent<Animator>();
		}

		PlayerViewCone playerViewCone = GetComponent<PlayerViewCone>();
		m_PlayerViewOrigin = playerViewCone.GetOriginTransform();

#if DEBUG_FIRE_GUN_RAYCAST
		m_DebugRayBacklog = new RayBacklogEntry[m_DebugRayBacklogSize];
		for (int rayIndex = 0; rayIndex < m_DebugRayBacklogSize; ++rayIndex)
		{
			m_DebugRayBacklog[rayIndex] = new RayBacklogEntry();
		}
#endif
	}
	
	// Update is called once per frame
	void Update()
	{
		float deltaTime = Time.deltaTime;

		if (deltaTime > 0.0f)
		{
			UpdateInput();
		}

#if DEBUG_FIRE_GUN_RAYCAST
		DrawDebugRayBacklog(deltaTime);
#endif
	}

	private void UpdateInput()
	{
		PlayerAction action = m_FireGunAction;
		string actionButton = action.m_ActionButton;
		
		bool onButtonDown = action.m_ActionOnButtonDown;
		bool onButtonUp = action.m_ActionOnButtonUp;
		bool onButtonHold = action.m_ActionOnButtonHold;
		
		bool validButton = (actionButton.Length > 0);
		bool inputDown = validButton && onButtonDown && (Input.GetButtonDown(actionButton));
		bool inputUp = validButton && onButtonUp && (Input.GetButtonUp(actionButton));
		bool inputHold = validButton && onButtonHold && (Input.GetButton(actionButton));
		
		bool isValidInput = (inputDown || inputUp || inputHold);
		bool isValidAnimTrigger = (m_GunAnimator != null) && (m_FireGunAnimTrigger.Length > 0);
		
		if (isValidInput && isValidAnimTrigger)
		{
			m_GunAnimator.SetTrigger(m_FireGunAnimTrigger);

			audio.PlayOneShot(m_FireGunSound);

			FireGun();
		}
	}

	private void FireGun()
	{
		RaycastHit hit = new RaycastHit();
		int layerNpc = LayerMask.NameToLayer("npcLayer");
		int layerNpcMask = 1 << layerNpc;

		Vector3 rayStart = m_PlayerViewOrigin.position;
		Vector3 rayDir = m_PlayerViewOrigin.forward;
		float rayLength = m_FireGunRange;

#if DEBUG_FIRE_GUN_RAYCAST
		Vector3 rayEnd = rayStart + rayLength * rayDir;
		Debug.DrawLine(rayStart, rayEnd, Color.yellow);

		bool hasHit = false;
		bool hasHitNPC = false;
#endif

		if (Physics.Raycast(rayStart, rayDir, out hit, rayLength, layerNpcMask))
		{
#if DEBUG_FIRE_GUN_RAYCAST	
			hasHit = true;
#endif
			GameObject hitObject = hit.transform.gameObject;
			if (hitObject.tag == "npc")
			{
#if DEBUG_FIRE_GUN_RAYCAST
				Debug.Log(string.Format("NPC {0} has been shot!", hitObject.ToString()));
				hasHitNPC = true;
#endif
				// get the NPC game object
				NpcController npcCtrl = hit.transform.gameObject.GetComponent<NpcController>();
				npcCtrl.OnShoot();
			}
		}

#if DEBUG_FIRE_GUN_RAYCAST
		int debugRayBacklogIndex = (m_LastDebugRayBacklogIndex + 1) % m_DebugRayBacklogSize;
		m_DebugRayBacklog[debugRayBacklogIndex].lifeTime = m_RayLifeTime;
		m_DebugRayBacklog[debugRayBacklogIndex].rayStart = rayStart;
		m_DebugRayBacklog[debugRayBacklogIndex].rayDir = rayDir;
		m_DebugRayBacklog[debugRayBacklogIndex].rayLength = rayLength;
		m_DebugRayBacklog[debugRayBacklogIndex].hasHit = hasHit;
		m_DebugRayBacklog[debugRayBacklogIndex].hasHitNPC = hasHitNPC;

		m_LastDebugRayBacklogIndex = debugRayBacklogIndex;
#endif
	}
	
#if DEBUG_FIRE_GUN_RAYCAST
	private void DrawDebugRayBacklog(float _DeltaTime)
	{
		for (int rayIndex = m_LastDebugRayBacklogIndex; rayIndex >= 0; --rayIndex)
		{
			RayBacklogEntry ray = m_DebugRayBacklog[rayIndex];
			ray.lifeTime -= _DeltaTime;

			if (ray.lifeTime <= 0.0f)
			{
				break;
			}
			else
			{
				Vector3 rayStart = ray.rayStart;
				Vector3 rayEnd = rayStart + ray.rayLength * ray.rayDir;

				Color rayColor = Color.green;
				if (ray.hasHitNPC)
				{
					rayColor = Color.red;
				}
				else if (ray.hasHit)
				{
					rayColor = Color.black;
				}

				float rayLifeRatio = ray.lifeTime / m_RayLifeTime;
				rayColor.a *= rayLifeRatio;
				
				Debug.DrawLine(ray.rayStart, rayEnd, rayColor);
			}
		}

		for (int rayIndex = m_DebugRayBacklogSize-1; rayIndex > m_LastDebugRayBacklogIndex ; --rayIndex)
		{
			RayBacklogEntry ray = m_DebugRayBacklog[rayIndex];
			ray.lifeTime -= _DeltaTime;
			
			if (ray.lifeTime <= 0.0f)
			{
				break;
			}
			else
			{
				Vector3 rayStart = ray.rayStart;
				Vector3 rayEnd = rayStart + ray.rayLength * ray.rayDir;
				
				Color rayColor = Color.green;
				if (ray.hasHitNPC)
				{
					rayColor = Color.red;
				}
				else if (ray.hasHit)
				{
					rayColor = Color.black;
				}
				
				float rayLifeRatio = ray.lifeTime / m_RayLifeTime;
				rayColor.a *= rayLifeRatio;
				
				Debug.DrawLine(ray.rayStart, rayEnd, rayColor);
			}
		}
	}
#endif
}
