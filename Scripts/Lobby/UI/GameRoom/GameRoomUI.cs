using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRoomUI : MonoBehaviour
{
	[SerializeField]
	private Button _bgCloseButton;
	[SerializeField]
	private Button _closeButton;
	[SerializeField]
	private GameObject _scrollContent;
	[SerializeField]
	private GameObject _userSlotSample;
	[SerializeField]
	private Button _gameStartButton;

	private int _stageID;

	//Init=====================================================================================================================
	#region Init
	public void InitUIObject()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.AddListener(App.Ins.ActiveBackButtonEvent);
		else
			LogManager.LogError("Cannot Find _bgCloseButton");

		if (_closeButton)
			_closeButton.onClick.AddListener(App.Ins.ActiveBackButtonEvent);
		else
			LogManager.LogError("Cannot Find _closeButton");

		if (_scrollContent == null)
			LogManager.LogError("Cannot Find _scrollContent");

		if(_userSlotSample == null)
			LogManager.LogError("Cannot Find _userSlotSample");

		if (_gameStartButton)
			_gameStartButton.onClick.AddListener(OnClickGameStartButton);
		else
			LogManager.LogError("Cannot Find _gameStartButton");

		gameObject.SetActive(false);
	}
	#endregion
	//=====================================================================================================================Init


	public void SettingGameRoomUI(int stageID)
	{
		gameObject.SetActive(true);
		_stageID = stageID;
		DataBridgeManager.Ins.SetStageID(_stageID);
		if (_scrollContent)
			UnityUtility.DestroyChildObject(_scrollContent);
		CreateJoinedUser();
		SettingMasterState();
		App.Ins.PushBackButtonEvent(OnClickCloseButton);
	}

	/// <summary> 이미 접속한 유저 슬롯 생성 </summary>
	private void CreateJoinedUser()
	{
		Dictionary<int, Photon.Realtime.Player> players = PhotonNetwork.CurrentRoom.Players;
		//TODO : 정렬
		foreach (KeyValuePair<int, Photon.Realtime.Player> i in players)
		{
			if (i.Value.IsMasterClient)
			{
				CreateUserSlor(i.Value.NickName, i.Value.IsMasterClient);
				break;
			}
		}

		foreach (KeyValuePair<int, Photon.Realtime.Player> i in players)
		{
			if (i.Value.IsMasterClient == false)
			{
				CreateUserSlor(i.Value.NickName, i.Value.IsMasterClient);
				break;
			}
		}
	}

	private void SettingMasterState()
	{
		_gameStartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
	}

	public void CreateJoinUser(string name)
	{
		CreateUserSlor(name, false);
	}

	public void LeftUser(string nickName)
	{
		GameObject[] childs = _scrollContent.gameObject.GetChildsObject();
		for (int i = 0; i < childs.Length; ++i)
		{
			GameRoomUI_UserSlot slot = childs[i].GetComponent<GameRoomUI_UserSlot>();
			if(slot && slot.GetUsetName() == nickName)
			{
				Destroy(slot);
				return;
			}
		}
	}

	private void CreateUserSlor(string name, bool isMaster)
	{
		if (_scrollContent && _userSlotSample)
		{
			GameObject instantiate = Instantiate(_userSlotSample, _scrollContent.transform);
			GameRoomUI_UserSlot slot = instantiate.GetComponent<GameRoomUI_UserSlot>();
			slot.InitUIObject();
			slot.SettingSlot(name, isMaster);
		}
		else
		{
			LogManager.LogError("GameRoomUI Component Error");
		}
	}


	//UI Event===================================================================================================================
	#region UI Event
	private void OnClickCloseButton()
	{
		NetworkManager_Lobby.Ins.LeaveRoom();
		gameObject.SetActive(false);
	}

	private void OnClickGameStartButton()
	{
		//TODO : 스테이지 셋팅
		AudioManager.Ins.PlayClick();
		NetworkManager_Lobby.Ins.GameStart();
	}
	#endregion
	//=================================================================================================================UI Event


	private void OnDestroy()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.RemoveAllListeners();
		if (_closeButton)
			_closeButton.onClick.RemoveAllListeners();
		if (_gameStartButton)
			_gameStartButton.onClick.RemoveAllListeners();
	}
}
