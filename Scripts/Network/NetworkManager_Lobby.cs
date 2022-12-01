using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class NetworkManager_Lobby : MonoBehaviourPunCallbacks
{
	#region SINGLETON
	static NetworkManager_Lobby _instance = null;
	public static NetworkManager_Lobby Ins
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(NetworkManager_Lobby)) as NetworkManager_Lobby;
				if (_instance == null)
				{
					_instance = new GameObject("NetworkManager_Lobby", typeof(NetworkManager_Lobby)).GetComponent<NetworkManager_Lobby>();
				}
			}
			return _instance;
		}
	}
	#endregion

	[SerializeField]
	private string _appVersion = "1";
	[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
	private byte _maxUserCount = 3;
	private bool _isConnectedToMaster = false;
	private int _stageID;

	/// <summary> 네트워크 설정 초기화 </summary>
	public void InitNetwork()
	{
		//TODO : 기기 네트워크 상태 확인
		if (PhotonNetwork.IsConnected == false)
		{
			//네트워크 연결 시작
			LogManager.StartLog("Connect Network");
			//모든 클라이언트가 마스터 클라이언트 씬과 동기화하도록 설정
			//true로 설정한 상태에서 마스터 클라이언트가 PhotonNetwork.LoadLevel()호출시 모든 클라이언트들은 동일한 레벨을 자동적으로 로드됨
			//(마스터 클라이언트느 씬 재로드 후 호출하여 하위 클라이언트의 장면을 업데이트 하는 방식으로 예상됨. 마스터 = 재로드, 클라이언트 = 동기화)
			PhotonNetwork.AutomaticallySyncScene = true;

			//버전 셋팅 후 버전에맞는 서버 연결
			PhotonNetwork.GameVersion = _appVersion;

			//에셋 데이터로 서버에 연결
			PhotonNetwork.ConnectUsingSettings();
		}
		else
		{
			//이미 연결된 네트워크 있음
			//인게임에서 나와도 서버는 연결되어있음
			LogManager.DebugLog("PhotonNetwork already conneted");

			//TODO : InRoom상태도 확인할건지 정해야함
		}
	}

	public void JoinRandomOrCreateRoom(int stageID)
	{
		PhotonNetwork.NickName = App.Ins.userDataTable.nickName;
		_stageID = stageID;
		if (PhotonNetwork.IsConnected && _isConnectedToMaster)
		{
			LogManager.StartLog("Join Room");
			//Join으로 먼저 시도하고 실패시 Create호출할 수 있음
			//Join, Create의 옵션설정할 수 있음
			PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, null, null, null, new RoomOptions { MaxPlayers = _maxUserCount });
		}
		else
		{
			App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "error_network_error");
			LogManager.LogError("Not Connected Network");
		}
	}

	public void GameStart()
	{
		//Photon네트워크는 씬 이름을 RoomFor1~4로 명시하여 방의 인원수를 명시함
		//추구하는 방향은 아니지만 따르는걸로..
		LogManager.NoticeLog("Game Start");
		App.Ins.SetSceneState(GlobalDefine.eSceneType.InGameLoading);
		SceneLoadManager.GetInstance().LoadScene(string.Format("Room for {0}", PhotonNetwork.CurrentRoom.PlayerCount));
	}

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}


	//Callback=================================================================================================================
	#region Callback
	/// <summary> 서버 연결 성공 </summary>
	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		//인게임에서 Disconnect완료 후 로비로 돌아올 수 있도록 설계해야함
		LogManager.NetworkLog("OnConnectedToMaster() was called by PUN.");
		LogManager.EndLog("Connect Network");
		_isConnectedToMaster = true;
	}

	/// <summary> 방 연결 성공 </summary>
	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		LogManager.NetworkLog(string.Format("OnJoinedRoom() called by PUN: {0}", PhotonNetwork.CurrentRoom.Name));
		LogManager.EndLog("Join Room");
		App_Lobby.Ins.lobbyUI.GetGameRoomUI().SettingGameRoomUI(_stageID);
	}

	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
		LogManager.NetworkLog(string.Format("OnCreateRoom() called by PUN: {0}", PhotonNetwork.CurrentRoom.Name));
		LogManager.EndLog("Create Room");
		App_Lobby.Ins.lobbyUI.GetGameRoomUI().SettingGameRoomUI(_stageID);
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		base.OnJoinRandomFailed(returnCode, message);
		LogManager.LogError(string.Format("OnCreateRoomFailed Code : {0} Mssage : {1}", returnCode, message));
		PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _maxUserCount });
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		base.OnJoinRoomFailed(returnCode, message);
		LogManager.LogError(string.Format("OnCreateRoomFailed Code : {0} Mssage : {1}", returnCode, message));
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		base.OnCreateRoomFailed(returnCode, message);
		LogManager.LogError(string.Format("OnCreateRoomFailed Code : {0} Mssage : {1}", returnCode, message));
	}

	/// <summary> 에러로인한 네트워크 연결 끊김 </summary>
	public override void OnDisconnected(DisconnectCause cause)
	{
		base.OnDisconnected(cause);
		LogManager.LogError(string.Format("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause));
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		LogManager.NetworkLog(string.Format("OnPlayerEnteredRoom() {0}", newPlayer.NickName));
		App_Lobby.Ins.lobbyUI.GetGameRoomUI().CreateJoinUser(newPlayer.NickName);
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		LogManager.NetworkLog(string.Format("OnPlayerEnteredRoom() {0}", otherPlayer.NickName));
		App_Lobby.Ins.lobbyUI.GetGameRoomUI().LeftUser(otherPlayer.NickName);
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		LogManager.NetworkLog("OnLeftRoom()");
	}
	#endregion
	//=================================================================================================================Callback
}
