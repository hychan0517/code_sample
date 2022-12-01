using DG.Tweening;
using GlobalDefine;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
	private enum eLobbyUIPageType
	{
		Login,
		Main,
		RoomList,
	}

	[SerializeField]
	private RectTransform _movablePanel;

	[Header("Button")]
	[SerializeField]
	private Button _tutorialModeButton;
	[SerializeField]
	private Button _practiceModeButton;
	[SerializeField]
	private Button _singlePlayButton;
	[SerializeField]
	private Button _multiPlayButton;
	[SerializeField]
	private Button _optionButton;

	[Header("UI")]
	[SerializeField]
	private GameRoomUI _gameRoomUI;
	[SerializeField]
	private LoginUI _loginUI;
	[SerializeField]
	private RoomListUI _roomListUI;
	[SerializeField]
	private OptionUI _optionUI;
	[SerializeField]
	private MapDownloadUI _mapDownloadUI;

	private Tween _slideTween;
	

	//Init=====================================================================================================================
	#region Init
	public void InitUIObject()
	{
		if(_movablePanel == null)
			LogManager.LogError("Cannt Find _movablePanel");

		if (_tutorialModeButton == null)
			LogManager.LogError("Cannt Find _tutorialModeButton");
		else
			_tutorialModeButton.onClick.AddListener(OnClickTutorialButton);

		if (_practiceModeButton == null)
			LogManager.LogError("Cannt Find _practiceModeButton");
		else
			_practiceModeButton.onClick.AddListener(OnClickPracticeModeButton);

		if (_singlePlayButton == null)
			LogManager.LogError("Cannt Find _singlePlayButton");
		else
			_singlePlayButton.onClick.AddListener(OnClickSinglePlayButton);

		if (_multiPlayButton == null)
			LogManager.LogError("Cannt Find _multiPlayButton");
		else
			_multiPlayButton.onClick.AddListener(OnClickMultiplayButton);

		if (_optionButton == null)
			LogManager.LogError("Cannt Find _optionButton");
		else
			_optionButton.onClick.AddListener(OnClickOptionButton);

		if (_gameRoomUI)
			_gameRoomUI.InitUIObject();
		else
			LogManager.LogError("Cannt Find _gameRoomUI");

		if (_loginUI)
			_loginUI.InitUIObject();
		else
			LogManager.LogError("Cannt Find _loginUI");

		if (_roomListUI)
			_roomListUI.InitUIObject();
		else
			LogManager.LogError("Cannt Find _roomListUI");
	}
	#endregion
	//=====================================================================================================================Init


	//UI Func==================================================================================================================
	#region UI Func
	public void SettingLobbyUI()
	{
		if(App.Ins.userDataTable._isLoginComplete)
		{
			SettingMainUIWithoutAnimation();
		}
		else
		{
			SettingLoginUI();
		}
	}

	public void SettingLoginUI()
	{
		if (_loginUI)
			_loginUI.SettingLoginUI();
		SetActiveOptionButton(true);
		SettingLobbyUIPage(eLobbyUIPageType.Login);
		App.Ins.PushBackButtonEvent(App.Ins.globalUI.OnClickExitButtonToBack);
	}

	public void SettingMainUI()
	{
		SetActiveOptionButton(true);
		SettingLobbyUIPage(eLobbyUIPageType.Main);
		App.Ins.PushBackButtonEvent(SettingLoginUI);
	}

	/// <summary> 인게임에서 로비로입장 </summary>
	public void SettingMainUIWithoutAnimation()
	{
		App.Ins.PushBackButtonEvent(App.Ins.globalUI.OnClickExitButtonToBack);
		SetActiveOptionButton(true);
		SettingLobbyUIPage(eLobbyUIPageType.Main, false);
		App.Ins.PushBackButtonEvent(SettingLoginUI);
	}

	public void SettingRoomListPage()
	{
		if (_roomListUI)
			_roomListUI.SettingRoomListUI();
		SetActiveOptionButton(false);
		SettingLobbyUIPage(eLobbyUIPageType.RoomList);
		App.Ins.PushBackButtonEvent(SettingMainUI);
	}

	public void UpdateMapList()
	{
		if (_roomListUI)
			_roomListUI.SettingMapList();
	}

	private void SettingLobbyUIPage(eLobbyUIPageType lobbyUIPageType, bool isAnimation = true)
	{
		if (_slideTween != null && _slideTween.active)
			_slideTween.Kill();

		int index = (int)lobbyUIPageType;
		if(_movablePanel)
		{
			if (App.Ins.optionDataTable.isTweenAnimation && isAnimation)
				_slideTween = _movablePanel.DOAnchorPosX(-_movablePanel.rect.width * index, App.Ins.tweenDurationTime);
			else
				_movablePanel.anchoredPosition = new Vector2(-_movablePanel.rect.width * index, _movablePanel.anchoredPosition.y);
		}
	}

	private void SetActiveOptionButton(bool isActive)
	{
		if (_optionButton)
			_optionButton.gameObject.SetActive(isActive);
	}
	#endregion
	//==================================================================================================================UI Func


	//Getter===================================================================================================================
	#region Getter
	private OptionUI GetOptionUI()
	{
		if(_optionUI == null)
		{
			UnityUtility.InstantiateUI(ref _optionUI, ResourcesPath.LOBBY_OPTION_UI, App_Lobby.Ins.mainCanvas.transform, 7);
		}

		return _optionUI;
	}

	public GameRoomUI GetGameRoomUI()
	{
		return _gameRoomUI;
	}

	public MapDownloadUI GetMapDownloadUI()
	{
		if (_mapDownloadUI == null)
		{
			UnityUtility.InstantiateUI(ref _mapDownloadUI, ResourcesPath.LOBBY_MAP_DOWNLOAD_UI, App_Lobby.Ins.mainCanvas.transform, 6);
		}

		return _mapDownloadUI;
	}
	#endregion
	//===================================================================================================================Getter


	//Setter===================================================================================================================
	#region Setter
	#endregion
	//===================================================================================================================Setter


	//UI Event=================================================================================================================
	#region UI Event
	private void OnClickSinglePlayButton()
	{
		AudioManager.Ins.PlayClick();
	}

	private void OnClickPracticeModeButton()
	{
		AudioManager.Ins.PlayClick();
	}

	private void OnClickTutorialButton()
	{
		AudioManager.Ins.PlayClick();
	}

	private void OnClickMultiplayButton()
	{
		AudioManager.Ins.PlayClick();
		SettingRoomListPage();
		NetworkManager_Lobby.Ins.InitNetwork();
	}

	private void OnClickOptionButton()
	{
		AudioManager.Ins.PlayClick();
		if (GetOptionUI())
			_optionUI.SettingOptionUI();
	}
	#endregion
	//=================================================================================================================UI Event

	private void OnDestroy()
	{
		if (_tutorialModeButton)
			_tutorialModeButton.onClick.RemoveAllListeners();

		if (_practiceModeButton)
			_practiceModeButton.onClick.RemoveAllListeners();

		if (_singlePlayButton)
			_singlePlayButton.onClick.RemoveAllListeners();

		if (_multiPlayButton)
			_multiPlayButton.onClick.RemoveAllListeners();

		if (_optionButton)
			_optionButton.onClick.RemoveAllListeners();
	}
}
