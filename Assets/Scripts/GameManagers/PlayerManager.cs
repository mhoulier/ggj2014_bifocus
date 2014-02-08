#define DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNetworkData
{
	public NetworkPlayer m_NetClient;
	public NetworkViewID m_NetViewID;

	public PlayerNetworkData(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		m_NetClient = _NetClient;
		m_NetViewID = _NetViewID;
	}
}

public class Player
{
	private InputDevice m_LocalPlayerInput = InputDevice.E_InputDeviceNone;
	private PlayerNetworkData m_PlayerNetworkData = null;
	private GameObject m_PlayerInstance = null;
	
	public Player(InputDevice _LocalPlayerInput)
	{
		m_LocalPlayerInput = _LocalPlayerInput;
		m_PlayerNetworkData = null;
		m_PlayerInstance = null;
	}
	
	public void InitPlayerNetworkData(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		m_PlayerNetworkData = new PlayerNetworkData(_NetClient, _NetViewID);
	}

	public NetworkPlayer GetNetClient()
	{
		System.Diagnostics.Debug.Assert(m_PlayerNetworkData != null);
		return m_PlayerNetworkData.m_NetClient;
	}

	public NetworkViewID GetNetViewID()
	{
		System.Diagnostics.Debug.Assert(m_PlayerNetworkData != null);
		return m_PlayerNetworkData.m_NetViewID;
	}
	
	public InputDevice GetPlayerInput() { return m_LocalPlayerInput; }
	public void SetPlayerInput(InputDevice _NewPlayerInput) { m_LocalPlayerInput = _NewPlayerInput; }
	
	public GameObject GetPlayerInstance() { return m_PlayerInstance; }
	public void SetPlayerInstance(GameObject _PlayerInstance) { m_PlayerInstance = _PlayerInstance; }

	//@HACK storing player class in here  for now
	private PlayerClass m_PlayerClass = PlayerClass.E_ClassNone;

	public PlayerClass GetPlayerClass() { return m_PlayerClass; }
	public void SetPlayerClass(PlayerClass _PlayerClass) { m_PlayerClass = _PlayerClass; }
}

public class PlayerManager : MonoBehaviour
{
	[SerializeField]
	private int m_PlayerCountMax = 4;
	private int GetPlayerCountMax() { return m_PlayerCountMax; }

	private NetworkManager m_NetworkManager = null;

	private List<Player> m_Players = null;
	public List<Player> GetPlayers() { return m_Players; }

	private List<Player> m_PendingLocalPlayers = null;
	private List<InputDevice> m_LocalPlayerJoinRequests = null;
	private List<PlayerNetworkData> m_PlayerJoinAuthorityRequests = null;
	private List<PlayerNetworkData> m_PlayerJoinClientRequests = null;

	private bool m_RemoveAllPlayersRequested = false;

	private EndGameStatus m_EndGameStatus = EndGameStatus.E_EndGameNone;
	public EndGameStatus GetEndGameStatus()
	{
		return m_EndGameStatus;
	}
	public void SetEndGameStatus(EndGameStatus _EndGameStatus)
	{
		m_EndGameStatus = _EndGameStatus;
	}
	
	void Awake()
	{
		int initPlayerCount = GetPlayerCountMax();

		InitPlayers(initPlayerCount);
		InitRequests(initPlayerCount);
	}

	private void InitPlayers(int _InitPlayerCount)
	{
		m_Players = new List<Player>(_InitPlayerCount);
		m_PendingLocalPlayers = new List<Player>(_InitPlayerCount);
	}

	private void RemoveAllPlayers()
	{
		//@HACK: this won't handle pending/concurrent player joining
		m_Players.Clear();
		m_PendingLocalPlayers.Clear();
	}

	private void InitRequests(int _InitRequestCount)
	{
		m_RemoveAllPlayersRequested = false;

		m_LocalPlayerJoinRequests = new List<InputDevice>(_InitRequestCount);
		m_PlayerJoinAuthorityRequests = new List<PlayerNetworkData>(_InitRequestCount);
		m_PlayerJoinClientRequests = new List<PlayerNetworkData>(_InitRequestCount);
	}

	private void ClearRequests()
	{
		m_RemoveAllPlayersRequested = false;
		
		m_LocalPlayerJoinRequests.Clear();
		m_PlayerJoinAuthorityRequests.Clear();
		m_PlayerJoinClientRequests.Clear();
	}
	
	void Start()
	{
		InitNetworking();
	}

	private void InitNetworking()
	{
		NetworkManager networkManager = (NetworkManager)FindObjectOfType( typeof(NetworkManager) );
		if (networkManager == null)
		{
			Debug.Log("GameManager prefab has no NetworkManager component: Only local games supported!");
		}

		m_NetworkManager = networkManager;
	}

	private bool IsNetworkGame()
	{
		bool isNetworkGame = (m_NetworkManager != null) && m_NetworkManager.IsNetworkGame();
		System.Diagnostics.Debug.Assert(isNetworkGame == false || m_NetworkManager.GetServer() != null || m_NetworkManager.GetClient() != null);

		return isNetworkGame;
	}

	private bool IsNetworkAuthority()
	{
		//@FIXME this doesn't support having a server for other purposes (ie. matchmaking, ...etc)
		bool isNetworkAuthority = (m_NetworkManager != null) && (m_NetworkManager.GetServer() != null);
		return isNetworkAuthority;
	}
	
	void Update()
	{
		//@FIXME: should it process requests when deltaTime is 0.0f?
		ProcessRequests();
	}

	private void ProcessRequests()
	{
		bool removeAllPlayers = m_RemoveAllPlayersRequested;
		if (removeAllPlayers)
		{
			RemoveAllPlayers();
		}
		else
		{
			bool isNetworkGame = IsNetworkGame();
			if (isNetworkGame)
			{
				//Network Game
				bool isNetworkAuthority = IsNetworkAuthority();
				foreach (InputDevice playerInput in m_LocalPlayerJoinRequests)
				{
					NetworkPlayerJoin(playerInput, isNetworkAuthority);
				}

				if (isNetworkAuthority)
				{
					foreach (PlayerNetworkData playerNetData in m_PlayerJoinAuthorityRequests)
					{
						PlayerJoinAuthority(playerNetData.m_NetClient, playerNetData.m_NetViewID);
					}
				}
				else
				{
					foreach (PlayerNetworkData playerNetData in m_PlayerJoinClientRequests)
					{
						PlayerJoinClient(playerNetData.m_NetClient, playerNetData.m_NetViewID);
					}
				}
			}
			else
			{
				//Local Game
				foreach (InputDevice _playerInput in m_LocalPlayerJoinRequests)
				{
					LocalPlayerJoin(_playerInput);
				}
			}
		}

		ClearRequests();
	}

	public void RequestRemoveAllPlayers()
	{
		m_RemoveAllPlayersRequested = true;
	}

	public void RequestLocalPlayerJoin(InputDevice _PlayerInput)
	{
		m_LocalPlayerJoinRequests.Add(_PlayerInput);
	}

	private void NetworkPlayerJoin(InputDevice _PlayerInput, bool _IsAuthority)
	{
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();
		NetworkViewID newPlayerNetViewID = Network.AllocateViewID();

		if (_IsAuthority)
		{
			Player joinedPlayer = LocalPlayerJoin(_PlayerInput);
			joinedPlayer.InitPlayerNetworkData(localNetClient, newPlayerNetViewID);
		}
		else
		{
			Player pendingPlayer = AddLocalPlayerPending(_PlayerInput);
			pendingPlayer.InitPlayerNetworkData(localNetClient, newPlayerNetViewID);

			networkView.RPC("RequestPlayerJoinAuthority", RPCMode.Server, localNetClient, newPlayerNetViewID);
		}
	}

	private Player LocalPlayerJoin(InputDevice _PlayerInput)
	{
		Player joiningPlayer = new Player(_PlayerInput);
		m_Players.Add(joiningPlayer);
		
		Debug.Log("Local player joined (input: " + joiningPlayer.GetPlayerInput().ToString() + ")");

		return joiningPlayer;
	}

#if DEBUG_ADD_LOCAL_PLAYERS_FOR_TEST
	public Player DebugLocalPlayerJoin(InputDevice _PlayerInput)
	{
		Player joiningPlayer = new Player(_PlayerInput);
		m_Players.Add(joiningPlayer);
		
		Debug.Log("Debug Local player joined (input: " + joiningPlayer.GetPlayerInput().ToString() + ")");
		
		return joiningPlayer;
	}
#endif

	private Player AddLocalPlayerPending(InputDevice _PlayerInput)
	{
		Player pendingPlayer = new Player(_PlayerInput);
		m_PendingLocalPlayers.Add(pendingPlayer);

		Debug.Log("Local player pending (input: " + pendingPlayer.GetPlayerInput().ToString() + ")");

		return pendingPlayer;
	}

	private void RemotePlayerJoin(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Player joiningPlayer = new Player(InputDevice.E_InputDeviceNone);
		joiningPlayer.InitPlayerNetworkData (_NetClient, _NetViewID);

		m_Players.Add(joiningPlayer);
		
		Debug.Log("Remote player joined");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
	}

	[RPC]
	public void RequestPlayerJoinAuthority(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Debug.Log("Player join request on Authority");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());

		PlayerNetworkData playerJoinAuthorityRequest = new PlayerNetworkData(_NetClient, _NetViewID);
		m_PlayerJoinAuthorityRequests.Add(playerJoinAuthorityRequest);
	}

	private void PlayerJoinAuthority(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		RemotePlayerJoin(_NetClient, _NetViewID);
		
		networkView.RPC("RequestPlayerJoinClient", RPCMode.Others, _NetClient, _NetViewID);
	}

	[RPC]
	public void RequestPlayerJoinClient(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		Debug.Log("Player join request on Client");
		Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
		
		PlayerNetworkData playerJoinClientRequest = new PlayerNetworkData(_NetClient, _NetViewID);
		m_PlayerJoinClientRequests.Add(playerJoinClientRequest);
	}

	private void PlayerJoinClient(NetworkPlayer _NetClient, NetworkViewID _NetViewID)
	{
		System.Diagnostics.Debug.Assert(m_NetworkManager != null);
		NetworkPlayer localNetClient = m_NetworkManager.GetLocalNetClient();

		bool isLocalPlayer = (localNetClient == _NetClient);
		if (isLocalPlayer)
		{
			Player joiningPlayer = null;
			foreach(Player pendingPlayer in m_PendingLocalPlayers)
			{
				if (pendingPlayer.GetNetViewID() == _NetViewID)
				{
					System.Diagnostics.Debug.Assert(joiningPlayer.GetNetClient() == _NetClient);
					
					joiningPlayer = pendingPlayer;
					break;
				}
			}
			System.Diagnostics.Debug.Assert(joiningPlayer != null);
			
			if (joiningPlayer != null)
			{
				InputDevice playerInput = joiningPlayer.GetPlayerInput();
				Player joinedPlayer = LocalPlayerJoin(playerInput);

				joinedPlayer.InitPlayerNetworkData(_NetClient, _NetViewID);
				Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());

				m_PendingLocalPlayers.Remove(joiningPlayer);
			}
			else
			{
				Debug.Log("Local player couldn't join: missing from the local players pending list!");
				Debug.Log("IP: "+ _NetClient.ipAddress + ", port:" + _NetClient.port + ", NetViewID: " + _NetViewID.ToString());
			}
		}
		else
		{
			RemotePlayerJoin(_NetClient, _NetViewID);
		}
	}


	private Player FindPendingLocalPlayer(NetworkViewID _NetViewID)
	{
		Player playerFound = null;
		foreach (Player player in m_PendingLocalPlayers)
		{
			if (player.GetNetViewID() == _NetViewID)
			{
				playerFound = player;
				break;
			}
		}
		
		return playerFound;
	}
}
