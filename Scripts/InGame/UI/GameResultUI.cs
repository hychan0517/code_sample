using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
	[SerializeField]
	private Button _bgCloseButton;
	[SerializeField]
	private Button _closeButton;
	[SerializeField]
	private LocalizedText _titleText;
	[SerializeField]
	private GameObject[] _starObjectArr = new GameObject[3];
	[SerializeField]
	private Slider _rateSlider;
	[SerializeField]
	private TextMeshProUGUI _rateText;
	[SerializeField]
	private TextMeshProUGUI _nowTimeText;
	[SerializeField]
	private TextMeshProUGUI _bestTimeText;
	[SerializeField]
	private Button _lobbyButton;
	
	public void InitUIObject()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannot find _bgCloseButton");

		if (_closeButton)
			_closeButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannot find _closeButton");

		if(_titleText == null)
			LogManager.LogError("Cannot find _titleText");

		if(_rateSlider == null)
			LogManager.LogError("Cannot find _rateSlider");

		if(_rateText == null)
			LogManager.LogError("Cannot find _rateText");

		if (_nowTimeText == null)
			LogManager.LogError("Cannot find _nowTimeText");

		if (_bestTimeText == null)
			LogManager.LogError("Cannot find _bestTimeText");

		if (_lobbyButton)
			_lobbyButton.onClick.AddListener(OnClickCloseButton);
		else
			LogManager.LogError("Cannot find _lobbyButton");

		gameObject.SetActive(false);
	}

	public void SettingResultUIFail()
	{
		gameObject.SetActive(true);

		if (_titleText)
			_titleText.text = "game_result_title_fail";

		SettingPlayTimeText();
		SettingBestTimeText();

		StartCoroutine(Co_RateAnimation(App_InGame.Ins.GetMonsterManager().GetClearRate()));
	}

	public void SettingResultUIClear()
	{
		gameObject.SetActive(true);

		if (_titleText)
			_titleText.text = "game_result_title_clear";

		SettingStar();
		SettingPlayTimeText();
		SettingBestTimeText();

		StartCoroutine(Co_RateAnimation(App_InGame.Ins.GetMonsterManager().GetClearRate()));
	}

	private void SettingStar()
	{
		TimeSpan timeSpan = App_InGame.Ins.GetMonsterManager().GetPlayTime();
		if(timeSpan.TotalSeconds < 10)
		{
			SetActiveOnStar(3);
		}
		else if(timeSpan.TotalSeconds < 20)
		{
			SetActiveOnStar(2);
		}
		else
		{
			SetActiveOnStar(1);
		}
	}

	private void SetActiveOnStar(int count)
	{
		for(int i = 0; i < _starObjectArr.Length; ++i)
		{
			if (i < count && _starObjectArr.Length > i && _starObjectArr[i])
				_starObjectArr[i].SetActive(true);
		}
	}

	private void SettingPlayTimeText()
	{
		TimeSpan playTime = App_InGame.Ins.GetMonsterManager().GetPlayTime();
		if (_nowTimeText)
			_nowTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)playTime.TotalHours, (int)playTime.TotalMinutes, (int)playTime.TotalSeconds);
	}

	private void SettingBestTimeText()
	{
		TimeSpan nowTime = App_InGame.Ins.GetMonsterManager().GetPlayTime();
		long bestTimeTicks = App.Ins.userDataTable._bestTime_1;
		if(bestTimeTicks > 0)
		{
			//기록 있음
			if (nowTime.Ticks < bestTimeTicks)
			{
				bestTimeTicks = nowTime.Ticks;
				App.Ins.userDataTable._bestTime_1 = bestTimeTicks;
				App.Ins.playerPrefsManager.SetNowUserData();
			}
		}
		else
		{
			//기록 없음
			bestTimeTicks = nowTime.Ticks;
			App.Ins.userDataTable._bestTime_1 = bestTimeTicks;
			App.Ins.playerPrefsManager.SetNowUserData();
		}

		TimeSpan bestTime = new TimeSpan(bestTimeTicks);
		if (_bestTimeText)
			_bestTimeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)bestTime.TotalHours, (int)bestTime.TotalMinutes, (int)bestTime.TotalSeconds);
	}

	private IEnumerator Co_RateAnimation(float targetRate)
	{
		if(_rateSlider && _rateText)
		{
			_rateSlider.value = 0;
			_rateText.text = "0%";
			float time = 0;

			if (App.Ins.optionDataTable.isTweenAnimation)
			{
				while (_rateSlider.value <= targetRate)
				{
					time += Time.deltaTime;
					_rateSlider.value = targetRate * (time * (0.5f / 1));
					_rateText.text = string.Format("{0}%", (int)(Math.Truncate(_rateSlider.value * 100)));
					yield return null;
				}
			}

			_rateSlider.value = targetRate;
			_rateText.text = string.Format("{0}%", (int)(Math.Truncate(targetRate * 100)));
		}
	}

	private void OnClickCloseButton()
	{
		AudioManager.Ins.PlayClick();
		NetworkManager_InGame.Ins.LeaveRoom();
	}

	private void OnDestroy()
	{
		if (_bgCloseButton)
			_bgCloseButton.onClick.RemoveAllListeners();
		if (_closeButton)
			_closeButton.onClick.RemoveAllListeners();
		if (_lobbyButton)
			_lobbyButton.onClick.RemoveAllListeners();
	}
}
