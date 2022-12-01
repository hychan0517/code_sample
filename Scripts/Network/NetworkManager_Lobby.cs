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

	/// <summary> ��Ʈ��ũ ���� �ʱ�ȭ </summary>
	public void InitNetwork()
	{
		//TODO : ��� ��Ʈ��ũ ���� Ȯ��
		if (PhotonNetwork.IsConnected == false)
		{
			//��Ʈ��ũ ���� ����
			LogManager.StartLog("Connect Network");
			//��� Ŭ���̾�Ʈ�� ������ Ŭ���̾�Ʈ ���� ����ȭ�ϵ��� ����
			//true�� ������ ���¿��� ������ Ŭ���̾�Ʈ�� PhotonNetwork.LoadLevel()ȣ��� ��� Ŭ���̾�Ʈ���� ������ ������ �ڵ������� �ε��
			//(������ Ŭ���̾�Ʈ�� �� ��ε� �� ȣ���Ͽ� ���� Ŭ���̾�Ʈ�� ����� ������Ʈ �ϴ� ������� �����. ������ = ��ε�, Ŭ���̾�Ʈ = ����ȭ)
			PhotonNetwork.AutomaticallySyncScene = true;

			//���� ���� �� �������´� ���� ����
			PhotonNetwork.GameVersion = _appVersion;

			//���� �����ͷ� ������ ����
			PhotonNetwork.ConnectUsingSettings();
		}
		else
		{
			//�̹� ����� ��Ʈ��ũ ����
			//�ΰ��ӿ��� ���͵� ������ ����Ǿ�����
			LogManager.DebugLog("PhotonNetwork already conneted");

			//TODO : InRoom���µ� Ȯ���Ұ��� ���ؾ���
		}
	}

	public void JoinRandomOrCreateRoom(int stageID)
	{
		PhotonNetwork.NickName = App.Ins.userDataTable.nickName;
		_stageID = stageID;
		if (PhotonNetwork.IsConnected && _isConnectedToMaster)
		{
			LogManager.StartLog("Join Room");
			//Join���� ���� �õ��ϰ� ���н� Createȣ���� �� ����
			//Join, Create�� �ɼǼ����� �� ����
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
		//Photon��Ʈ��ũ�� �� �̸��� RoomFor1~4�� ����Ͽ� ���� �ο����� �����
		//�߱��ϴ� ������ �ƴ����� �����°ɷ�..
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
	/// <summary> ���� ���� ���� </summary>
	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		//�ΰ��ӿ��� Disconnect�Ϸ� �� �κ�� ���ƿ� �� �ֵ��� �����ؾ���
		LogManager.NetworkLog("OnConnectedToMaster() was called by PUN.");
		LogManager.EndLog("Connect Network");
		_isConnectedToMaster = true;
	}

	/// <summary> �� ���� ���� </summary>
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

	/// <summary> ���������� ��Ʈ��ũ ���� ���� </summary>
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
