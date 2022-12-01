using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviour
{
	[SerializeField]
	public RectTransform canvasRectTransform;
	[SerializeField]
	public SendBirdUI sendBirdUI;
	[SerializeField]
	public MessageBoxUI messageBoxUI;

	[Header("Button")]
	[SerializeField]
	private Button _backButton;
	[SerializeField]
	private Button _exitButton;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void InitUIObject()
	{
		if(canvasRectTransform == null)
			LogManager.LogError("Cannot Find canvasRectTransform");

		if (sendBirdUI)
			sendBirdUI.InitUIObject();
		else
			LogManager.LogError("Cannot Find _sendBirdUI");

		if (messageBoxUI)
			messageBoxUI.InitUIObject();
		else
			LogManager.LogError("Cannot Find _messageBoxUI");

		if (_backButton)
			_backButton.onClick.AddListener(OnClickBackButton);
		else
			LogManager.LogError("Cannt Find _backButton");

		if (_exitButton)
			_exitButton.onClick.AddListener(OnClickExitButton);
		else
			LogManager.LogError("Cannt Find _exitButton");

		App.Ins.PushBackButtonEvent(OnClickExitButtonToBack);
	}

	private void OnClickBackButton()
	{
		App.Ins.ActiveBackButtonEvent();
	}

	private void OnClickExitButton()
	{
		AudioManager.Ins.PlayClick();
		App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultTwoButton, null, QuitApplication, null, "check_quit_application_message");
	}

	public void OnClickExitButtonToBack()
	{
		App.Ins.PushBackButtonEvent(OnClickExitButtonToBack);
		App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultTwoButton, null, QuitApplication, null, "check_quit_application_message");
	}

	public void OnClickGameExitButtonToBack()
	{
		App.Ins.PushBackButtonEvent(OnClickGameExitButtonToBack);
		App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultTwoButton, null, OnClickGameExitYes, null, "check_exit_game_go_to_lobby_message");
	}

	private void OnClickGameExitYes()
	{
		if (App.Ins.GetNowSceneState() == GlobalDefine.eSceneType.InGame)
			NetworkManager_InGame.Ins.LeaveRoom();
		else
			LogManager.LogError("OnClickGameExitYes Scene Error");
	}

	private void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	private void OnDestroy()
	{
		if (_backButton)
			_backButton.onClick.RemoveAllListeners();

		if (_exitButton)
			_exitButton.onClick.RemoveAllListeners();
	}
}
