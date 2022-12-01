using Photon.Pun;
using UnityEngine;

public class NetworkManager_InGame : MonoBehaviourPunCallbacks
{
	#region SINGLETON
	static NetworkManager_InGame _instance = null;
	public static NetworkManager_InGame Ins
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(NetworkManager_InGame)) as NetworkManager_InGame;
				if (_instance == null)
				{
					_instance = new GameObject("NetworkManager_InGame", typeof(NetworkManager_InGame)).GetComponent<NetworkManager_InGame>();
				}
			}
			return _instance;
		}
	}
	#endregion

	/// <summary> 마스터 클라이언트 로드 후 전체 클라이언트 레벨로드 </summary>
	private void LoadArena()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			LogManager.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
		}
		else
		{
			LogManager.NetworkLog(string.Format("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount));
			//아래 이름으로 씬로드를 호출함
			PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
		}
	}

	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	//Callback=================================================================================================================
	#region Callback
	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		LogManager.NetworkLog(string.Format("OnPlayerEnteredRoom() {0}", newPlayer.NickName));
		if (!PhotonNetwork.IsMasterClient)
		{
			//마스터 클라이언트만 동기화 호출할 수 있음
			//PhotonNetwork.automaticallySyncScene를 true로 설정해두었기 때문에 룸 안의 모든 클라이언트 레벨로드를 Photon이 실행
			LoadArena();
		}
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName); // seen when other disconnects


		if (PhotonNetwork.IsMasterClient)
		{
			LogManager.NetworkLog(string.Format("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient)); // called before OnPlayerLeftRoom
			//마스터 클라이언트만 동기화 호출할 수 있음
			//PhotonNetwork.automaticallySyncScene를 true로 설정해두었기 때문에 룸 안의 모든 클라이언트 레벨로드를 Photon이 실행
			LoadArena();
		}
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		LogManager.NetworkLog("OnLeftRoom");
		//TODO : LeftRoom완료까지 대기, 행동제한 로딩, Left실패시..?

		if (App.Ins != null)
		{
			//Destroy 예외처리
			App.Ins.GoToLobby();
		}
	}
	#endregion
	//=================================================================================================================Callback
}
