using GlobalDefine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class MapDownloadUI : MonoBehaviour, IInstantiatable
{
	[SerializeField]
	private Button _bgCloseButton;
	[SerializeField]
	private Button _closeButton;
	[SerializeField]
	private Image _mapImage;
	[SerializeField]
	private TextMeshProUGUI _freeMemoryText;
	[SerializeField]
	private TextMeshProUGUI _mapMemoryText;
	[SerializeField]
	private Button _downloadButton;
	[SerializeField]
	private Slider _progressBar;

	private int _mapID;
	private Action _downloadCompleteAction;

	public void Init()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannot Find _bgCloseButton");

		if (_closeButton)
			_closeButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannot Find _closeButton");

		if(_mapImage == null)
			LogManager.LogError("Cannot Find _mapImage");

		if (_freeMemoryText == null)
			LogManager.LogError("Cannot Find _freeMemoryText");

		if (_mapMemoryText == null)
			LogManager.LogError("Cannot Find _mapMemoryText");

		if (_downloadButton)
			_downloadButton.onClick.AddListener(OnClickDownloadButton);
		else
			LogManager.LogError("Cannot Find _downloadButton");

		if(_progressBar == null)
			LogManager.LogError("Cannot Find _progressBar");

		gameObject.SetActive(false);
	}

	public void SettingMapDownloadUI(int mapID, Action downloadCompleteAction)
	{
		gameObject.SetActive(true);
		_mapID = mapID;
		_downloadCompleteAction = downloadCompleteAction;
		ResetUI();
		SettingMapImage();
		SettingMemoryText();
		App.Ins.PushBackButtonEvent(DisableUI);
	}

	private void ResetUI()
	{
		if (_downloadButton)
			_downloadButton.gameObject.SetActive(true);
		if (_progressBar)
			_progressBar.gameObject.SetActive(false);
	}

	private void SettingMapImage()
	{
		if (_mapImage)
			_mapImage.sprite = Resources.Load<Sprite>(string.Format(ResourcesPath.LOBBY_MAP_SPRITE_BASE, _mapID));
	}

	private void SettingMemoryText()
	{
		Addressables.GetDownloadSizeAsync(string.Format("{0}{1}", Define.MAP_RESOURCE_BASE, _mapID)).Completed += (handle) =>
		{
			float availableDisk = SimpleDiskUtils.DiskUtils.CheckAvailableSpace();
			float downSize = ((handle.Result / 1024f) / 1024f); //MB
			if(_freeMemoryText)
				_freeMemoryText.text = string.Format("{0:F2}{1}", availableDisk > 1024f ? availableDisk / 1024f : availableDisk, availableDisk > 1024f ? "GB" : "MB");
			if (_mapMemoryText)
				_mapMemoryText.text = string.Format("{0:F2}MB", downSize > 1024f ? downSize / 1024f : downSize, downSize > 1024f ? "GB" : "MB");
			Addressables.Release(handle);
		};
	}

	private void OnClickDownloadButton()
	{
		AudioManager.Ins.PlayClick();

		if (_downloadButton)
			_downloadButton.gameObject.SetActive(false);
		if (_progressBar)
		{
			_progressBar.gameObject.SetActive(true);
			_progressBar.value = 0;
		}

		var handle = Addressables.DownloadDependenciesAsync(string.Format("{0}{1}", Define.MAP_RESOURCE_BASE, _mapID));
		StartCoroutine(Co_MapDownload(handle));
		handle.Completed += (_handle) =>
		{
			StopCoroutine(Co_MapDownload(handle));
			if (_handle.Status == AsyncOperationStatus.Failed)
			{
				LogManager.LogError(string.Format("Map Download Error ID : {0}, Message : {1}", _mapID, _handle.OperationException.ToString()));
				App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "lobby_map_download_fail");
				ResetUI();
			}
			else if (_handle.IsValid())
			{
				try
				{
					if (gameObject.activeSelf)
						App.Ins.ActiveBackButtonEvent();
					_downloadCompleteAction?.Invoke();
				}
				catch(Exception e)
				{
					LogManager.LogError(e.ToString());
				}
			}

			Addressables.Release(handle);
			Addressables.Release(_handle);
		};
	}

	private IEnumerator Co_MapDownload(AsyncOperationHandle handle)
	{
		while (!handle.IsDone)
		{
			if (_progressBar)
				_progressBar.value = handle.GetDownloadStatus().Percent;

			yield return new WaitForEndOfFrame();
		}
	}

	private void OnClickCloseButton()
	{
		App.Ins.ActiveBackButtonEvent();
	}

	private void DisableUI()
	{
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.RemoveAllListeners();
		if (_closeButton)
			_closeButton.onClick.RemoveAllListeners();
		if (_downloadButton)
			_downloadButton.onClick.RemoveAllListeners();
	}
}
