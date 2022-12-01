using GlobalDefine;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class MapSlot : MonoBehaviour
{
	[SerializeField]
	private Button _slotButton;
	[SerializeField]
	private Button _downloadButton;
	[SerializeField]
	private GameObject _lockState;
	[SerializeField]
	private GameObject _selectState;
	[SerializeField]
	private int _mapID;
	private Action<int> _slotClickCallback;

	public void InitUIObject(Action<int> slotClickCallback)
	{
		if (_slotButton)
		{
			_slotButton.onClick.AddListener(OnClickSlotButton);
		}
		else
		{
			LogManager.LogError("Cannot Find _slotButton");
		}

		if (_downloadButton)
			_downloadButton.onClick.AddListener(OnClickDownloadButton);
		else
			LogManager.LogError("Cannot Find _downloadButton");

		if (_lockState == null)
			LogManager.LogError("Cannot Find _lockState");

		if (_selectState == null)
			LogManager.LogError("Cannot Find _selectState");

		_slotClickCallback = slotClickCallback;
	}

	public void SettingMapSlot()
	{
		SetSelectState(false);
		SettingDownloadState(true);

		Addressables.GetDownloadSizeAsync(string.Format("{0}{1}", Define.MAP_RESOURCE_BASE, _mapID)).Completed += (handle) =>
		{
			//TODO : 비동기관리
			if (handle.Result == 0)
			{
				SettingDownloadState(false);
			}
			else
			{
				SettingDownloadState(true);
				float downSize = ((handle.Result / 1024f) / 1024f); //MB
			}
			Addressables.Release(handle);
		};
	}

	public void SetSelectState(bool isSelect)
	{
		if (_selectState)
			_selectState.SetActive(isSelect);
	}

	private void SettingDownloadState(bool isAble)
	{
		if (_lockState)
			_lockState.SetActive(isAble);
	}

	private void OnClickDownloadButton()
	{
		AudioManager.Ins.PlayClick();
		App_Lobby.Ins.lobbyUI.GetMapDownloadUI().SettingMapDownloadUI(_mapID, OnCompleteDownload);
	}

	private void OnCompleteDownload()
	{
		if (App.Ins.GetNowSceneState() == eSceneType.Lobby)
		{
			App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "lobby_map_download_complete");
			App_Lobby.Ins.lobbyUI.UpdateMapList();
		}
	}

	private void OnClickSlotButton()
	{
		AudioManager.Ins.PlayClick();
		_slotClickCallback?.Invoke(_mapID);
	}

	private void OnDestroy()
	{
		if (_slotButton)
			_slotButton.onClick.RemoveAllListeners();
		if (_downloadButton)
			_downloadButton.onClick.RemoveAllListeners();
	}
}
