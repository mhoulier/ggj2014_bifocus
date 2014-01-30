using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
	[SerializeField]
	private PlayerClass m_PlayerClass = PlayerClass.E_ClassNone;

	public static PlayerSpawner FindPlayerSpawner( PlayerClass _PlayerClass )
	{
		PlayerSpawner matchingSpawner = null;
		
		Object[] spawners = FindObjectsOfType(typeof(PlayerSpawner));
		foreach ( Object spawner in spawners )
		{
			PlayerSpawner playerSpawner = spawner as PlayerSpawner;			
			if (playerSpawner.m_PlayerClass == _PlayerClass)
			{
				matchingSpawner = playerSpawner;
				break;
			}
		}
		
		return matchingSpawner;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
