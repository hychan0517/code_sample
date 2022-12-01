using GlobalDefine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour, IInstantiatable
{
	[SerializeField]
	private Button _bgCloseButton;
	[SerializeField]
	private Button _closeButton;

	[Header("Langauge")]
	//TODO : 리스트가 많아지면 배열로 변경
	[SerializeField]
	private Toggle _koreanToggle;
	[SerializeField]
	private Toggle _englishToggle;

	[Header("Tween")]
	[SerializeField]
	private Toggle _tweenToggle;

	[Header("Audio")]
	[SerializeField]
	private Slider _musicSlider;
	[SerializeField]
	private TextMeshProUGUI _musicSliderText;
	[SerializeField]
	private Slider _soundSlider;
	[SerializeField]
	private TextMeshProUGUI _soundSliderText;

	public void Init()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannt Find _bgCloseButton");

		if (_closeButton)
			_closeButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannt Find _closeButton");

		if (_koreanToggle)
			_koreanToggle.onValueChanged.AddListener((_x) => { OnValueChangedLangaugeToggle(_x, eLangaugeType.Korean); });
		else
			LogManager.LogError("Cannt Find _koreanToggle");

		if (_englishToggle)
			_englishToggle.onValueChanged.AddListener((_x) => { OnValueChangedLangaugeToggle(_x, eLangaugeType.English); });
		else
			LogManager.LogError("Cannt Find _englishToggle");

		if (_tweenToggle)
			_tweenToggle.onValueChanged.AddListener(OnValueChangedTweenToggle);
		else
			LogManager.LogError("Cannt Find _tweenToggle");

		if (_musicSlider)
			_musicSlider.onValueChanged.AddListener(OnValueChangedMusicSlider);
		else
			LogManager.LogError("Cannt Find _musicSlider");

		if (_musicSliderText == null)
			LogManager.LogError("Cannt Find _musicSliderText");

		if (_soundSlider)
			_soundSlider.onValueChanged.AddListener(OnValueChangedSoundSlider);
		else
			LogManager.LogError("Cannt Find _soundSlider");

		if (_soundSliderText == null)
			LogManager.LogError("Cannt Find _soundSliderText");

		gameObject.SetActive(false);
	}

	public void SettingOptionUI()
	{
		gameObject.SetActive(true);
		SettingLangaugeToggle();
		SettingTweenToggle();
		SettingAudio();
		App.Ins.PushBackButtonEvent(DisableUI);
	}

	private void SettingLangaugeToggle()
	{
		eLangaugeType nowType = (eLangaugeType)App.Ins.optionDataTable.langaugeIndex;

		if (_koreanToggle && _englishToggle)
		{
			switch (nowType)
			{
				case eLangaugeType.Korean:
					_koreanToggle.isOn = true;
					break;
				case eLangaugeType.English:
					_englishToggle.isOn = true;
					break;
				default:
					LogManager.LogError("SettingLangaugeToggle Type Error");
					break;
			}
		}
	}

	private void SettingTweenToggle()
	{
		if (_tweenToggle)
			_tweenToggle.isOn = App.Ins.optionDataTable.isTweenAnimation;
	}

	private void SettingAudio()
	{
		if (_musicSlider)
			_musicSlider.value = App.Ins.optionDataTable.musicVolume;
		if (_soundSlider)
			_soundSlider.value = App.Ins.optionDataTable.soundVolume;
	}


	//UI Events==================================================================================
	#region UI Events
	private void OnClickCloseButton()
	{
		App.Ins.ActiveBackButtonEvent();
	}

	private void DisableUI()
	{
		gameObject.SetActive(false);
	}

	private void OnValueChangedLangaugeToggle(bool isOn, eLangaugeType type)
	{
		if (isOn)
		{
			int index = (int)type;
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
			App.Ins.optionDataTable.langaugeIndex = index;
			App.Ins.playerPrefsManager.SetOptionData();
		}
	}

	private void OnValueChangedTweenToggle(bool isOn)
	{
		App.Ins.optionDataTable.isTweenAnimation = isOn;
		App.Ins.playerPrefsManager.SetOptionData();
	}

	private void OnValueChangedMusicSlider(float value)
	{
		if (_musicSliderText)
			_musicSliderText.text = Math.Truncate(value * 100).ToString();

		App.Ins.optionDataTable.musicVolume = value;
		App.Ins.playerPrefsManager.SetOptionData();
		AudioManager.Ins.SetMusicVolume(value);
	}

	private void OnValueChangedSoundSlider(float value)
	{
		if (_soundSliderText)
			_soundSliderText.text = Math.Truncate(value * 100).ToString();
		
		App.Ins.optionDataTable.soundVolume = value;
		App.Ins.playerPrefsManager.SetOptionData();
		AudioManager.Ins.SetSoundVolume(value);
	}
	#endregion
	//==================================================================================UI Events


	private void OnDestroy()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.RemoveAllListeners();
		if (_closeButton)
			_closeButton.onClick.RemoveAllListeners();
		if (_koreanToggle)
			_koreanToggle.onValueChanged.RemoveAllListeners();
		if (_englishToggle)
			_englishToggle.onValueChanged.RemoveAllListeners();
		if (_tweenToggle)
			_tweenToggle.onValueChanged.RemoveAllListeners();
		if (_musicSlider)
			_musicSlider.onValueChanged.RemoveAllListeners();
		if (_soundSlider)
			_soundSlider.onValueChanged.RemoveAllListeners();
	}
}
