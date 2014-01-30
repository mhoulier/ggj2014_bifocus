using UnityEngine;
using System.Collections;

public enum NetworkMode { E_NetworkNone, E_NetworkLanOnly, E_NetworkOnlineOnly, };

public class NetworkManager : MonoBehaviour
{
	public NetworkMode m_NetworkMode = NetworkMode.E_NetworkNone;
	public int m_ConnectionCountMax = 4;
	public int m_ConnectPort = 4231;
	public int m_AdvertisingPort = 6534;
	public int m_DisconnectTimeOutInMilliseconds = 200;
	public bool m_AdvertiseServer = true;
	
	public string m_ServerName = "MyGameServer";

	void Start()
	{
	
	}
	
	void Update()
	{
	
	}
	
	public NetworkPlayer GetLocalNetClient() { return Network.player; }
	
	public bool IsNetworkAuthorithy() { return (Network.isServer || (Network.isClient == false)); }
	
	public void StartServer(NetworkServer.ServerConnectionDelegate _ClientConnectedDelegate, NetworkServer.ServerConnectionDelegate _ClientDisconnectedDelegate)
	{
		System.Diagnostics.Debug.Assert( GetServer() == null );
		
		if (m_NetworkMode != NetworkMode.E_NetworkNone)
		{
			NetworkServer server = gameObject.AddComponent<NetworkServer>();
			server.StartServer(m_NetworkMode, m_ConnectionCountMax, m_ConnectPort, m_ServerName,
				OnServerStarted, OnServerStopped, _ClientConnectedDelegate, _ClientDisconnectedDelegate);
		}
	}
	
	public void OnServerStarted()
	{
		if (m_AdvertiseServer)
		{
			NetworkServer server = GetServer();
			System.Diagnostics.Debug.Assert(server != null);
			
			server.StartServerAdvertising(m_AdvertisingPort);
		}
	}
	
	public void StopServer()
	{
		NetworkServer server = GetServer();
		if (server)
		{
			server.StopServer(m_DisconnectTimeOutInMilliseconds);
		}
		else
		{
			Debug.Log("Stop Server failed: game manager object has no NetworkServer component.");	
		}
	}
	
	public void OnServerStopped()
	{
		NetworkServer server = GetServer();
		Destroy(server);
	}
	
	public NetworkServer GetServer()
	{
		NetworkServer server = gameObject.GetComponent<NetworkServer>();
		return server;
	}
	
	public void StartServerSearch()
	{
		System.Diagnostics.Debug.Assert( GetServerSearch() == null );
		
		if (m_NetworkMode != NetworkMode.E_NetworkNone)
		{
			NetworkServerSearch serverFinder = gameObject.AddComponent<NetworkServerSearch>();
			serverFinder.StartSearch(m_AdvertisingPort, OnServerSearchStopped);
		}
	}
	
	public void StopServerSearch()
	{
		NetworkServerSearch serverFinder = GetServerSearch();
		if (serverFinder)
		{
			serverFinder.StopSearch();
		}
		else
		{
			Debug.Log("Stop Server Search failed: game manager object has no NetworkFinder component.");	
		}
	}
	
	public void OnServerSearchStopped()
	{
		NetworkServerSearch serverFinder = GetServerSearch();
		Destroy(serverFinder);
	}
	
	public NetworkServerSearch GetServerSearch()
	{
		NetworkServerSearch serverFinder = gameObject.GetComponent<NetworkServerSearch>();
		return serverFinder;
	}
	
	public void ConnectToServer(string _ServerIpAddress, int _ConnectPort)
	{
		System.Diagnostics.Debug.Assert( GetClient() == null );
		
		if (m_NetworkMode != NetworkMode.E_NetworkNone)
		{
			NetworkClient client = gameObject.AddComponent<NetworkClient>();
			client.ConnectToServer(_ServerIpAddress, _ConnectPort, OnClientConnected, OnClientDisconnected);
		}
	}
	
	public void OnClientConnected()
	{
	}
	
	public void OnClientDisconnected()
	{
		NetworkClient client = GetClient();
		Destroy(client);
		
		//@FIXME: load fallback 'menu level' or sthg if client is in a different level
	}
	
	public NetworkClient GetClient()
	{
		NetworkClient client = gameObject.GetComponent<NetworkClient>();
		return client;
	}
}
