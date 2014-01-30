using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
	
	private int m_LastLevelPrefix = 0;
	private bool m_LoadingLevel = true;
	
	public bool IsLoading()
	{
		return m_LoadingLevel;
	}
	
	public int GetCurrentLevelIndex() { return Application.loadedLevel; }
	public int GetLevelCount() { return Application.levelCount; }
	
	public bool IsValidLevelIndex(int _levelIndex)
	{
		int levelCount = GetLevelCount();
		bool IsValidIndex = (0 <= _levelIndex) && (_levelIndex < levelCount);
		return IsValidIndex;
	}
	
	public void QuitGame()
	{
		Application.Quit();
	}
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		networkView.group = 1;
	}
	
	void Start()
	{
		
	}

	void OnLevelWasLoaded(int _level)
	{
		m_LoadingLevel = false;

		Debug.Log( string.Format("Loaded level {0} ...", _level) );
	}

	public void LoadLevel(int _level)
	{
		int levelCount = GetLevelCount();
		int levelToLoad = _level % levelCount;

		Debug.Log( string.Format("Loading level {0} ...", levelToLoad) );

		m_LoadingLevel = true;

		Application.LoadLevel(levelToLoad);
	}
	
	public void LoadNextLevel()
	{
		int currentLevelIndex = GetCurrentLevelIndex();
		int levelCount = GetLevelCount();
		int nextLevelIndex = (currentLevelIndex + 1) % levelCount;
		
		LoadLevel(nextLevelIndex);
	}

	public IEnumerator LoadLevelAsync(int _level, float _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);

		LoadLevel(_level);
	}

	public IEnumerator LoadNextLevelAsync(float _waitTimeInSeconds)
	{
		yield return new WaitForSeconds(_waitTimeInSeconds);

		LoadNextLevel();
	}
	
	public void ServerLoadLevel(int _level)
	{
		System.Diagnostics.Debug.Assert(Network.isServer);
		
		Network.RemoveRPCsInGroup(0);
		Network.RemoveRPCsInGroup(1);
		
		RPCMode loadLevelRPCMode = RPCMode.All; //RPCMode.AllBuffered;
		networkView.RPC( "LoadLevelRPC", loadLevelRPCMode, _level, m_LastLevelPrefix + 1);
	}
	
	public void ClientLoadLevelRequest(NetworkPlayer _LocalNetClient)
	{
		System.Diagnostics.Debug.Assert(Network.isClient);
		
		networkView.RPC( "ServerLoadLevelRequestRPC", RPCMode.Server, _LocalNetClient);
	}
	
	[RPC]
	public void ServerLoadLevelRequestRPC(NetworkPlayer _NetClient)
	{
		System.Diagnostics.Debug.Assert(Network.isServer);
		
		int currentLevelIndex = GetCurrentLevelIndex();
		networkView.RPC( "LoadLevelRPC", _NetClient, currentLevelIndex, m_LastLevelPrefix);
	}
	
	[RPC]
	public void LoadLevelRPC( int _level, int _levelPrefix )
	{
		StartCoroutine(NetworkLoadLevelAsync(_level, _levelPrefix));
	}
	
	public IEnumerator NetworkLoadLevelAsync(int _level, int _levelPrefix)
	{
		m_LastLevelPrefix = _levelPrefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);	

		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;

		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(_levelPrefix);
		
		yield return StartCoroutine(LoadLevelAsync(_level, 1.0f));
		//yield return new WaitForEndOfFrame();
		//LoadLevel(_level);

		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);

		//for (var go in FindObjectsOfType(GameObject))
		//	go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
	}

	void Update()
	{
	}
}
