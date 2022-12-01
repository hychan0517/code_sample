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

	/// <summary> ������ Ŭ���̾�Ʈ �ε� �� ��ü Ŭ���̾�Ʈ �����ε� </summary>
	private void LoadArena()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			LogManager.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
		}
		else
		{
			LogManager.NetworkLog(string.Format("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount));
			//�Ʒ� �̸����� ���ε带 ȣ����
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
			//������ Ŭ���̾�Ʈ�� ����ȭ ȣ���� �� ����
			//PhotonNetwork.automaticallySyncScene�� true�� �����صξ��� ������ �� ���� ��� Ŭ���̾�Ʈ �����ε带 Photon�� ����
			LoadArena();
		}
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName); // seen when other disconnects


		if (PhotonNetwork.IsMasterClient)
		{
			LogManager.NetworkLog(string.Format("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient)); // called before OnPlayerLeftRoom
			//������ Ŭ���̾�Ʈ�� ����ȭ ȣ���� �� ����
			//PhotonNetwork.automaticallySyncScene�� true�� �����صξ��� ������ �� ���� ��� Ŭ���̾�Ʈ �����ε带 Photon�� ����
			LoadArena();
		}
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		LogManager.NetworkLog("OnLeftRoom");
		//TODO : LeftRoom�Ϸ���� ���, �ൿ���� �ε�, Left���н�..?

		if (App.Ins != null)
		{
			//Destroy ����ó��
			App.Ins.GoToLobby();
		}
	}
	#endregion
	//=================================================================================================================Callback
}
