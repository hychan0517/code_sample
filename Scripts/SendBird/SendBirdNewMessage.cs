using TMPro;
using UnityEngine;

public class SendBirdNewMessage : MonoBehaviour
{
	//[SerializeField]
	//private TextMeshProUGUI _newMessageText;
	//private SendBirdUI _sendBirdUI;

	//private int _newMessageCount;

	//private void Awake()
	//{
	//	InitUIObject();
	//}

	//private void InitUIObject()
	//{
	//	if (_newMessageText == null)
	//		LogManager.LogError("Cannot Find _newMessageText");

	//	if (SendBirdManager.GetInstance())
	//	{
	//		SendBirdManager.GetInstance().AddReceiveNewMessage(ReceiveNewMessage);
	//		_sendBirdUI = SendBirdManager.GetInstance().GetSendBirdUI();

	//		if (_sendBirdUI)
	//			_newMessageCount = _sendBirdUI.GetNewMessageCount();

	//		SettingNewMessageUIText();
	//	}
	//}


	////UI Func====================================================================================
	//#region UI Fun
	//public void ReceiveNewMessage(SendBirdUI.eSendBirdChannelType type, string userID)
	//{
	//	if (_sendBirdUI != null && _sendBirdUI.gameObject.activeSelf == false)
	//	{
	//		++_newMessageCount;
	//	}
	//	else
	//	{
	//		_newMessageCount = 0;
	//	}

	//	SettingNewMessageUIText();
	//}

	//private void SettingNewMessageUIText()
	//{
	//	if (_newMessageCount > 999)
	//		_newMessageCount = 999;

	//	if (_newMessageText)
	//	{
	//		_newMessageText.text = _newMessageCount.ToString();
	//	}

	//	if (_newMessageCount > 0)
	//	{
	//		gameObject.SetActive(true);
	//	}
	//	else
	//	{
	//		gameObject.SetActive(false);
	//	}
	//}
	//#endregion
	////====================================================================================UI Func


	//private void OnDestroy()
	//{
	//	if (SendBirdManager.GetInstance() != null)
	//		SendBirdManager.GetInstance().RemoveReceiveNewMessage(ReceiveNewMessage);
	//}

}
