using GlobalDefine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameUI : MonoBehaviour, IInstantiatable
{
	[SerializeField]
	private Button _bgCloseButton;
	[SerializeField]
	private Button _closeButton;
	[SerializeField]
	private TMP_InputField _nicknameInputField;
	[SerializeField]
	private Button _okButton;

	private Action<string, bool> _createNicknameCallback;
	private bool _isKakao;

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

		if(_nicknameInputField == null)
			LogManager.LogError("Cannot Find _nicknameInputField");

		if (_okButton)
			_okButton.onClick.AddListener(OnClickOKButton);
		else
			LogManager.LogError("Cannot Find _okButton");
	}
	
	public void SettingNicknameUI(Action<string, bool> createNicknameCallback, bool isKakao)
	{
		gameObject.SetActive(true);

		_createNicknameCallback = createNicknameCallback;
		_isKakao = isKakao;
		if (_nicknameInputField)
			_nicknameInputField.text = string.Empty;

		App.Ins.PushBackButtonEvent(OnClickBackButton);
	}

	private void OnClickOKButton()
	{
		AudioManager.Ins.PlayClick();
		gameObject.SetActive(false);
		App.Ins.DeleteLastBackButtonEvent(OnClickBackButton);
		if (_nicknameInputField)
		{
			string repairedString = App.Ins.badWordDataManager.GetRepairWord(_nicknameInputField.text);
			if (repairedString == _nicknameInputField.text)
			{
				_createNicknameCallback?.Invoke(_nicknameInputField.text, _isKakao);
			}
			else
			{
				App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "error_create_nickname_bad_word");
			}
		}
	}

	private void OnClickCloseButton()
	{
		_createNicknameCallback?.Invoke(string.Empty, _isKakao);
		App.Ins.ActiveBackButtonEvent();
	}

	private void OnClickBackButton()
	{
		gameObject.SetActive(false);
		_createNicknameCallback?.Invoke(string.Empty, _isKakao);
	}

	private void OnDestroy()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.RemoveAllListeners();
		if (_closeButton)
			_closeButton.onClick.RemoveAllListeners();
		if (_okButton)
			_okButton.onClick.RemoveAllListeners();
	}
}
