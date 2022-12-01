using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _playerNameText;
	[SerializeField]
	private Image _hpFillImage;
	[SerializeField]
	private TextMeshProUGUI _playerHPRateText;
	[SerializeField]
	private GameObject _dieState;
	private Player _player;

	public void InitUIObject()
	{
		if (_playerNameText == null)
			LogManager.LogError("Cannof find _playerNameText");

		if (_hpFillImage == null)
			LogManager.LogError("Cannof find _hpFillImage");

		if (_playerHPRateText == null)
			LogManager.LogError("Cannof find _playerHPRateText");

		if (_dieState)
			_dieState.SetActive(false);
		else
			LogManager.LogError("Cannot find _dieState");

		gameObject.SetActive(false);
	}

	public void SetPlayer(Player player)
	{
		gameObject.SetActive(true);
		_player = player;
		if (_playerNameText)
			_playerNameText.text = player.name;
		StartCoroutine(Co_UpdatePlayerHP());
	}

	private IEnumerator Co_UpdatePlayerHP()
	{
		while (_player)
		{
			if (_player.GetPlayerHPRate() > 0)
			{
				_hpFillImage.fillAmount = App_InGame.Ins.GetPlayer().GetPlayerHPRate();
				_playerHPRateText.text = string.Format("{0}%", Math.Truncate(_hpFillImage.fillAmount * 100));

				yield return null;
			}
			else
			{
				_hpFillImage.fillAmount = 0;
				_playerHPRateText.text = "0%";
				break;
			}
		}

		if (_dieState)
			_dieState.SetActive(true);

		if(_player == null)
			Destroy(gameObject);
	}
}
