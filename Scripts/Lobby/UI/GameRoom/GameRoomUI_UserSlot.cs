using System;
using TMPro;
using UnityEngine;

public class GameRoomUI_UserSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _nameText;
	[SerializeField]
	private TextMeshProUGUI _stateText;
	[SerializeField]
	private GameObject _masterIcon;


	//Init=====================================================================================================================
	#region Init
	public void InitUIObject()
	{
		if (_nameText == null)
			LogManager.LogError("Cannot Find _nameText");

		if (_stateText == null)
			LogManager.LogError("Cannot Find _stateText");

		if(_masterIcon == null)
			LogManager.LogError("Cannot Find _masterIcon");

		gameObject.SetActive(false);
	}
	#endregion
	//=====================================================================================================================Init


	public void SettingSlot(string name, bool isMaster)
	{
		gameObject.SetActive(true);

		if(_nameText)
			_nameText.text = name;

		if (_stateText)
		{
			if (isMaster)
				_stateText.text = "Master";
			else
				_stateText.text = string.Empty;
		}

		if (_masterIcon)
			_masterIcon.SetActive(isMaster);
	}

	public string GetUsetName()
	{
		if (_nameText)
			return _nameText.text;
		else
			return string.Empty;
	}
}
