using UnityEngine;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class UdpClientState
{
	public IPEndPoint e;
	public UdpClient u;
	
	public UdpClientState(IPEndPoint _EndPoint, UdpClient _UdpClient)
	{
		e = _EndPoint;
		u = _UdpClient;
	}
}

public class NetworkServer : MonoBehaviour
{
	public enum ServerState { E_ServerNone, E_ServerPendingStart, E_ServerStarted, E_ServerPendingStop, E_ServerStopped };
	private ServerState m_ServerState = ServerState.E_ServerNone;
	
	[System.Serializable]
	public class ServerAdvertisingData
	{
		public int m_ServerConnectionMaxCount;
		public int m_ServerConnectPort;
		public string m_ServerName;
		
		public ServerAdvertisingData()
		{
			m_ServerConnectionMaxCount = 0;
			m_ServerConnectPort = -1;
			m_ServerName = null;
		}
		
		public ServerAdvertisingData(int _ServerConnectionMaxCount, int _ServerConnectPort, string _ServerName)
		{
			m_ServerConnectionMaxCount = _ServerConnectionMaxCount;
			m_ServerConnectPort = _ServerConnectPort;
			m_ServerName = _ServerName;
		}
		
		//@TODO: add more server info, like max connections, current connections, game mode, ...
		
		public byte[] Serialize()
		{
	    	using (MemoryStream m = new MemoryStream())
			{
	        	using (BinaryWriter writer = new BinaryWriter(m))
				{
					writer.Write(m_ServerConnectionMaxCount);
	            	writer.Write(m_ServerConnectPort);
	            	writer.Write(m_ServerName);
	         	}
	         	return m.ToArray();
	      	}
	   	}
	
		public static ServerAdvertisingData Desserialize(byte[] data)
		{
			ServerAdvertisingData result = new ServerAdvertisingData();
			using (MemoryStream m = new MemoryStream(data))
			{
				using (BinaryReader reader = new BinaryReader(m))
				{
					result.m_ServerConnectionMaxCount = reader.ReadInt32();
					result.m_ServerConnectPort = reader.ReadInt32();
					result.m_ServerName = reader.ReadString();
				}
			}
			return result;
		}
	}
	
	public delegate void ServerEventDelegate();
	private ServerEventDelegate m_ServerStartedDelegate = null;
	private ServerEventDelegate m_ServerStoppedDelegate = null;
	public delegate void ServerConnectionDelegate(NetworkPlayer _Player);
	private ServerConnectionDelegate m_ClientConnectedDelegate = null;
	private ServerConnectionDelegate m_ClientDisconnectedDelegate = null;
	
	private string m_PublicServerName = null;
	
	public NetworkMode m_NetworkMode = NetworkMode.E_NetworkNone;
	private int m_ConnectionCountMax = 0;
	private int m_ConnectPort = -1;
	private bool m_IsUsingNatPunchThrough = false;
	
	//specific to LAN
	private int m_BroadcastListeningPortLAN = -1;
	private byte[] m_ServerAdvertisingDataLAN = null;
	
	private UdpClientState m_BroadcastListeningClientLAN = null;
	private UdpClientState m_AdvertisingClientLAN = null;
	
	private bool m_ListenForClientsRequested = false;
	
	private void RequestListenForClients() { m_ListenForClientsRequested = true; }
	
	private void CancelPendingListenForClientsRequests() { m_ListenForClientsRequested = false; }
	
	private void ProcessPendingListenForClientsRequests()
	{
		if (m_ListenForClientsRequested)
		{
			ListenForClients(m_BroadcastListeningPortLAN);
		
			m_ListenForClientsRequested = false;
		}
	}
	
	public bool IsServerStarted() { return m_ServerState == ServerState.E_ServerStarted; }
	private bool CanStartServer() { return m_ServerState == ServerState.E_ServerNone; }
	
	private void ChangeServerState(ServerState _NewServerState) { m_ServerState = _NewServerState; }
	
	void Start()
	{
		
	}
	
	void Update()
	{
		//float deltaTime = Time.deltaTime;
		//@FIXME: probably wants to process requests in any case?
		//deltaTime should probably not be 0 in networked games ...
		
		ProcessPendingListenForClientsRequests();
	}
	
	public void StartServer(NetworkMode _NetworkMode, int _ConnectionCountMax, int _ConnectPort, string _ServerName,
		ServerEventDelegate _ServerStartedDelegate, ServerEventDelegate _ServerStoppedDelegate,
		ServerConnectionDelegate _ClientConnectedDelegate, ServerConnectionDelegate _ClientDisconnectedDelegate)
	{
		Debug.Log("Starting Server...");
		
		System.Diagnostics.Debug.Assert(Network.peerType == NetworkPeerType.Disconnected);
		System.Diagnostics.Debug.Assert(CanStartServer());
		
		bool useNatPunchThrough = (_NetworkMode == NetworkMode.E_NetworkOnlineOnly) && !Network.HavePublicAddress();
		
		m_PublicServerName = _ServerName;
			
		m_NetworkMode = _NetworkMode;
		m_ConnectionCountMax = _ConnectionCountMax;
		m_ConnectPort = _ConnectPort;
		m_IsUsingNatPunchThrough = useNatPunchThrough;
		
		m_ServerStartedDelegate = _ServerStartedDelegate;
		m_ServerStoppedDelegate = _ServerStoppedDelegate;
		m_ClientConnectedDelegate = _ClientConnectedDelegate;
		m_ClientDisconnectedDelegate = _ClientDisconnectedDelegate;
		
		ChangeServerState(ServerState.E_ServerPendingStart);
		
		NetworkConnectionError initServerResult = Network.InitializeServer(_ConnectionCountMax, _ConnectPort, useNatPunchThrough);
		if (initServerResult != NetworkConnectionError.NoError)
		{
			Debug.Log("Init Server failed:" + initServerResult + " (port: " + _ConnectPort + ")" );
			
			//@TODO reset member variables
		}
	}
	
	void OnServerInitialized()
	{
        Debug.Log("Server initialized");
		System.Diagnostics.Debug.Assert(Network.isServer);
		System.Diagnostics.Debug.Assert(m_ServerState == ServerState.E_ServerPendingStart);
		
		ChangeServerState(ServerState.E_ServerStarted);
		
		if (m_ServerStartedDelegate != null)
		{
			m_ServerStartedDelegate();
			m_ServerStartedDelegate = null;
		}
    }
	
	public void StopServer(int _DisconnectTimeOutInMilliseconds)
	{
		Debug.Log("Stopping Server...");
		
		ChangeServerState(ServerState.E_ServerPendingStop);
		
		m_ServerStartedDelegate = null;
		m_ServerStoppedDelegate = null;
		m_ClientConnectedDelegate = null;
		m_ClientDisconnectedDelegate = null;
		
		StopServerAdvertising();
		
		//@FIXME: nothing provided in Unity3d Network API to specifically stop the server
		Network.Disconnect(_DisconnectTimeOutInMilliseconds);
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection _NetDisconnectionInfo)
	{
		Debug.Log("Server connection disconnected: " + _NetDisconnectionInfo.ToString());
		Debug.Log("Current network peer type is " + Network.peerType.ToString());
		
		m_ServerStartedDelegate = null;
		m_ServerStoppedDelegate = null;
		m_ClientConnectedDelegate = null;
		m_ClientDisconnectedDelegate = null;
		
		StopServerAdvertising();
		
		ChangeServerState(ServerState.E_ServerStopped);
		
		if (m_ServerStoppedDelegate != null)
		{
			m_ServerStoppedDelegate();
			m_ServerStoppedDelegate = null;
		}
    }
	
	void OnPlayerConnected(NetworkPlayer _NetPlayer) 
	{
		Debug.Log("New client connected: " + _NetPlayer.ipAddress + ":" + _NetPlayer.port);
		
		if (m_ClientConnectedDelegate != null)
		{
			m_ClientConnectedDelegate(_NetPlayer);
			m_ClientConnectedDelegate = null;
		}
    }
	
	void OnPlayerDisconnected(NetworkPlayer _NetPlayer) 
	{
		Debug.Log("Client disconnected: " + _NetPlayer.ipAddress + ":" + _NetPlayer.port);
		
		if (m_ClientDisconnectedDelegate != null)
		{
			m_ClientDisconnectedDelegate(_NetPlayer);
			m_ClientDisconnectedDelegate = null;
		}
		
        Network.RemoveRPCs(_NetPlayer);
        Network.DestroyPlayerObjects(_NetPlayer);
    }
	
	public void StartServerAdvertising(int _AdvertisingPort)
	{
		if (IsServerStarted() == false)
		{
			Debug.Log("Start Server advertising failed (port " + _AdvertisingPort + "): Server hasn't started yet, is in state " + m_ServerState.ToString());
		}
		else if (m_NetworkMode == NetworkMode.E_NetworkLanOnly)
		{
			if (m_ServerAdvertisingDataLAN != null)
			{
				Debug.Log("Start Server advertising failed (port " + _AdvertisingPort + "): Server advertising already started on port " + m_BroadcastListeningPortLAN);
			}
			else if (_AdvertisingPort == m_ConnectPort)
			{
				Debug.Log("Start Server advertising failed (port " + _AdvertisingPort + "): port already used for client connections.");
			}
			else
			{
				Debug.Log("Starting Server advertising on port " + _AdvertisingPort + " ...");
				m_BroadcastListeningPortLAN = _AdvertisingPort;
				
				ServerAdvertisingData serverAdvertData = new ServerAdvertisingData(m_ConnectionCountMax, m_ConnectPort, m_PublicServerName);
				m_ServerAdvertisingDataLAN = serverAdvertData.Serialize();
				
				RequestListenForClients();
			}
		}
		else
		{
			System.Diagnostics.Debug.Assert(m_NetworkMode == NetworkMode.E_NetworkOnlineOnly);
			//@TODO do online server registration to master server
		}
	}
	
	public void StopServerAdvertising()
	{
		if (m_NetworkMode == NetworkMode.E_NetworkLanOnly)
		{
			CancelPendingListenForClientsRequests();
			StopListenForClients();
			StopRespondClient();
		
			m_ServerAdvertisingDataLAN = null;
			m_BroadcastListeningPortLAN = -1;
		}
		else
		{
			System.Diagnostics.Debug.Assert(m_NetworkMode == NetworkMode.E_NetworkOnlineOnly);
			//@TODO do online server registration to master server
		}
	}
	
	//specific to LAN
	
	private void ListenForClients(int _ListenPort)
	{
		System.Diagnostics.Debug.Assert(m_BroadcastListeningClientLAN == null);
		
		// open a listening port on known port
		// to listen for any clients
		
		IPEndPoint ep1 = new IPEndPoint(IPAddress.Any, _ListenPort);
		UdpClient uc1 = new UdpClient(ep1);
		UdpClientState ucs1 = new UdpClientState(ep1, uc1); 

		uc1.BeginReceive(new System.AsyncCallback(ListenForClientsCallback), ucs1);
		
		m_BroadcastListeningClientLAN = ucs1;
		
		Debug.Log("Server listening for clients on port: " + _ListenPort);
	}
	
	private void StopListenForClients()
	{
		if (m_BroadcastListeningClientLAN != null)
		{
			m_BroadcastListeningClientLAN.u.Close();
			m_BroadcastListeningClientLAN = null;
		}
		
		Debug.Log("Server listening for clients stopped.");
	}
	
	public void ListenForClientsCallback(System.IAsyncResult _AsyncResult)
	{
		// we received a broadcast from a client
		
		Debug.Log("Client message received on server listening port.");
		UdpClientState ucs1 = (UdpClientState)(_AsyncResult.AsyncState);
		UdpClient uc1 = ucs1.u;
		IPEndPoint ep1 = ucs1.e;
		byte[] receivedBytes = uc1.EndReceive(_AsyncResult, ref ep1);
		int clientPort = System.BitConverter.ToInt32(receivedBytes,0);
		
		Debug.Log("Client is listening for reply on broadcast port " + clientPort.ToString());
		
		// send a response back to the client on the port
		// they sent us
		
		RespondClient(ep1.Address, clientPort, m_ServerAdvertisingDataLAN);
		
		// Important!
		// close and re-open the broadcast listening port
		// so that another async operation can start
		
		System.Diagnostics.Debug.Assert(uc1 == m_BroadcastListeningClientLAN.u);
		StopListenForClients();
		
		RequestListenForClients();
	}
	
	private void RespondClient(IPAddress _ClientAddress, int _ClientPort, byte[] _DataToSend)
	{
		System.Diagnostics.Debug.Assert(m_AdvertisingClientLAN == null);
		
		IPEndPoint ep2 = new IPEndPoint(_ClientAddress, _ClientPort);
		UdpClient uc2 = new UdpClient();	//@FIXME: doesn't need to pass EndPoint in constructor?
		UdpClientState ucs2 = new UdpClientState(ep2, uc2);
		uc2.BeginSend(_DataToSend, _DataToSend.Length, ep2, new System.AsyncCallback(RespondClientCallback), ucs2);
		
		m_AdvertisingClientLAN = ucs2;
	}
	
	private void StopRespondClient()
	{
		if (m_AdvertisingClientLAN != null)
		{
			m_AdvertisingClientLAN.u.Close();
			m_AdvertisingClientLAN = null;
		}
	}
	
	public void RespondClientCallback(System.IAsyncResult _AsyncResult)
	{
		UdpClientState ucs1 = (UdpClientState)(_AsyncResult.AsyncState);
		UdpClient uc1 = ucs1.u;
		//IPEndPoint ep1 = ucs1.e;
		
		int bytesSent = uc1.EndSend(_AsyncResult);
		
		Debug.Log("Client Response done: " + bytesSent + " bytes sent.");
		
		System.Diagnostics.Debug.Assert(uc1 == m_AdvertisingClientLAN.u);
		StopRespondClient();	
	}
}
