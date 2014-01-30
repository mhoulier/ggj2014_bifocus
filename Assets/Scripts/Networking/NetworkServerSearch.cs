using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class NetworkServerSearch : MonoBehaviour
{
	public delegate void ServerSearchEventDelegate();
	private ServerSearchEventDelegate m_SearchStoppedDelegate = null;
	
	public class ServerSearchData
	{
		public IPAddress m_ServerIpAddress;
		public int m_ServerConnectPort;
		public int m_ServerConnectionMaxCount;
		public string m_ServerName;
		
		public string m_ServerIpAddressString;
		
		public ServerSearchData(IPAddress _ServerIpAddress, int _ServerConnectPort, int _ServerConnectionMaxCount,  string _ServerName)
		{
			m_ServerIpAddress = _ServerIpAddress;
			m_ServerConnectPort = _ServerConnectPort;
			m_ServerConnectionMaxCount = _ServerConnectionMaxCount;
			m_ServerName = _ServerName;
			
			m_ServerIpAddressString = _ServerIpAddress.ToString();
		}
	}
	
	private List<ServerSearchData> m_ServerList = null;
	
	public List<ServerSearchData> GetServerList() { return m_ServerList; }
	
	private UdpClientState m_AdvertisingListeningClientLAN = null;
	private UdpClientState m_BroadcastClientLAN = null;
	
	private int m_BroadcastPortLAN = -1;
	
	private bool m_ListenForServerRequested = false;
	
	private void RequestListenForServer() { m_ListenForServerRequested = true; }
	
	private void CancelPendingListenForServerRequests() { m_ListenForServerRequested = false; }
	
	private void ProcessPendingListenForServerRequests()
	{
		if (m_ListenForServerRequested)
		{
			int randomPort = UnityEngine.Random.Range(15001,16000);
			ListenForServer(randomPort);
			FindServer(randomPort, m_BroadcastPortLAN);
		
			m_ListenForServerRequested = false;
		}
	}
	
	void Start()
	{
		InitSearch();
	}
	
	void Update()
	{
		//float deltaTime = Time.deltaTime;
		//@FIXME: probably wants to process requests in any case?
		//deltaTime should probably not be 0 in networked games ...
		
		ProcessPendingListenForServerRequests();
	}
	
	private void InitSearch()
	{
		m_ServerList = new List<NetworkServerSearch.ServerSearchData>();
	}
	
	private void AddServer(ServerSearchData _ServerData)
	{
		m_ServerList.Add(_ServerData);
	}
	
	private bool IsServerListed(IPAddress _IpAddress, int _ConnectPort)
	{
		bool isListed = false;
		foreach(ServerSearchData server in m_ServerList)
		{
			bool sameIpAddress = server.m_ServerIpAddress.Equals(_IpAddress);
			bool sameConnectPort = (server.m_ServerConnectPort == _ConnectPort);
			if (sameIpAddress && sameConnectPort)
			{
				isListed = true;
				break;
			}
		}
		
		return isListed;
	}
	
	private ServerSearchData BuildServerSearchData(IPAddress _IpAddress, int _ConnectPort, int _ConnectionMaxCount, string _PublicName)
	{	
		return new ServerSearchData(_IpAddress, _ConnectPort, _ConnectionMaxCount, _PublicName);
	}
	
	public void StartSearch(int _BroadcastPort, ServerSearchEventDelegate _SearchStoppedDelegate)
	{
		Debug.Log("Starting Server Search...");
		
		m_BroadcastPortLAN = _BroadcastPort;
		m_SearchStoppedDelegate = _SearchStoppedDelegate;
		RequestListenForServer();
	}
	
	public void StopSearch()
	{
		CancelPendingListenForServerRequests();
		StopListenForServer();
		StopFindServer();
		
		m_BroadcastPortLAN = -1;
		
		if (m_SearchStoppedDelegate != null)
		{
			m_SearchStoppedDelegate();
			m_SearchStoppedDelegate = null;
		}
	}
	
	private void ListenForServer(int _ListenPort)
	{
		System.Diagnostics.Debug.Assert(m_AdvertisingListeningClientLAN == null);
		
		// open a listening port on a random port
		// to receive a response back from server
		// using 0 doesn't seem to work reliably
		// so we'll just do it ourselves
		
		IPEndPoint ep1 = new IPEndPoint(IPAddress.Any, _ListenPort);
		UdpClient uc1 = new UdpClient(ep1);
		UdpClientState ucs1 = new UdpClientState(ep1, uc1);
		uc1.BeginReceive(new System.AsyncCallback(ListenForServerCallback), ucs1);
		
		m_AdvertisingListeningClientLAN = ucs1;
		
		Debug.Log("Finder advertising listener opened on port " + _ListenPort);
	}
	
	private void StopListenForServer()
	{
		// close the listener
		// this is needed to start a new search
		if (m_AdvertisingListeningClientLAN != null)
		{
			m_AdvertisingListeningClientLAN.u.Close();
			m_AdvertisingListeningClientLAN = null;
		}
		
		Debug.Log("Finder advertising listener closed");
	}
	
	public void ListenForServerCallback(System.IAsyncResult _AsyncResult)
	{
		// server has responded with its data
		
		UdpClientState ucs1 = (UdpClientState)(_AsyncResult.AsyncState);
		UdpClient uc1 = ucs1.u;
		IPEndPoint ep1 = ucs1.e;
		byte[] receivedBytes = uc1.EndReceive(_AsyncResult, ref ep1);
		
		IPAddress ipAddress = ep1.Address;
		NetworkServer.ServerAdvertisingData serverData = NetworkServer.ServerAdvertisingData.Desserialize(receivedBytes);
		int connectPort = serverData.m_ServerConnectPort;
		int connectionMaxCount = serverData.m_ServerConnectionMaxCount;
		string publicName = serverData.m_ServerName;
		
		bool isServerListed = IsServerListed(ipAddress, connectPort);
		if (isServerListed == false)
		{
			Debug.Log("Adding new server: " + ipAddress.ToString() + ": " + connectPort);
			ServerSearchData serverSearchData = BuildServerSearchData(ipAddress, connectPort, connectionMaxCount, publicName);
			AddServer(serverSearchData);
		}
		
		Debug.Log("Server responded to find server message over broadcast listener");
		
		// Important!
		// close the listening port
		// and re-open it just in
		// case another server responds
		
		System.Diagnostics.Debug.Assert(uc1 == m_AdvertisingListeningClientLAN.u);
		StopListenForServer();
		
		RequestListenForServer();		
	}
	
	private void FindServer(int _ListenPort, int _BroadcastPort)
	{
		System.Diagnostics.Debug.Assert(m_BroadcastClientLAN == null);
		
		// open a broadcast on known port and 
		// send own broadcast listener port to the LAN
		
		IPEndPoint ep2 = new IPEndPoint(IPAddress.Broadcast, _BroadcastPort);
		UdpClient uc2 = new UdpClient();
		// Important!
		// this is disabled by default
		// so we have to enable it
		uc2.EnableBroadcast = true;
		
		UdpClientState ucs2 = new UdpClientState(ep2, uc2);
		
		byte[] sendBytes = System.BitConverter.GetBytes(_ListenPort);
		uc2.BeginSend(sendBytes, sendBytes.Length, ep2, new System.AsyncCallback(FindServerCallback), ucs2);
		
		m_BroadcastClientLAN = ucs2;
		
		Debug.Log("Find server message sent on broadcast port " + _BroadcastPort);
	}
	
	private void StopFindServer()
	{
		if (m_BroadcastClientLAN != null)
		{
			m_BroadcastClientLAN.u.Close();
			m_BroadcastClientLAN = null;
		}
	}
	
	public void FindServerCallback(System.IAsyncResult _AsyncResult)
	{
		UdpClientState ucs1 = (UdpClientState)(_AsyncResult.AsyncState);
		UdpClient uc1 = ucs1.u;
		//IPEndPoint ep1 = ucs1.e;
		
		int bytesSent = uc1.EndSend(_AsyncResult);
		Debug.Log("Find Server done: " + bytesSent + " bytes sent.");
		
		System.Diagnostics.Debug.Assert(uc1 == m_BroadcastClientLAN.u);
		StopFindServer();		
	}
}
