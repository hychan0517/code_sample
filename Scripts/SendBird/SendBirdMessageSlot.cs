using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SendBird;

public class SendBirdMessageSlot : MonoBehaviour
{
	public struct sSendBirdMessageSlotData
	{
		public int SlotIndex;
		public int DataIndex;
		public SendBirdManager.sSendBirdMessageData SendBirdMessageData;
		public bool IsMove;
		public bool IsProcess;
	}

	[SerializeField]
	private RectTransform _rect;
	[Header("UserMessage")]
	[SerializeField]
	private GameObject _userMessage;
	[SerializeField]
	private TextMeshProUGUI _sendBirdUserMessageNameText;
	[SerializeField]
	private Image _sendBirdUserMessageBG;
	[SerializeField]
	private TextMeshProUGUI _sendBirdUserMessageText;
	[Header("SystemMessage")]
	[SerializeField]
	private GameObject _systemMessage;
	[SerializeField]
	private TextMeshProUGUI _sendBirdSystemMessageNameText;
	[SerializeField]
	private Image _sendBirdSystemMessageBG;
	[SerializeField]
	private TextMeshProUGUI _sendBirdSystemMessageText;

	private Vector2 _originSize;
	private Action<int, int, float, bool> _onCompleteSlotScaling;
	private Coroutine _scalingCor;
	private System.Collections.Generic.Queue<sSendBirdMessageSlotData> _slotProcessQueue = new System.Collections.Generic.Queue<sSendBirdMessageSlotData>();

	private SendBirdUI _sendBirdUI;
	private SendBirdManager.sSendBirdMessageData _lastProcessData;

	public void InitUIObject(Action<int, int, float, bool> onCompleteSlotScaling)
	{
		if (_rect == null)
			LogManager.LogError("Cannot Find _rect");

		//User
		if (_userMessage == null)
			LogManager.LogError("Cannot Find _userMessage");

		if (_sendBirdUserMessageNameText == null)
			LogManager.LogError("Cannot Find _sendBirdUserMessageNameText");

		if (_sendBirdUserMessageBG == null)
			LogManager.LogError("Cannot Find _sendBirdUserMessageBG");

		if (_sendBirdUserMessageText == null)
			LogManager.LogError("Cannot Find _sendBirdUserMessageText");

		//System
		if (_systemMessage == null)
			LogManager.LogError("Cannot Find _systemMessage");

		if (_sendBirdSystemMessageNameText == null)
			LogManager.LogError("Cannot Find _sendBirdSystemMessageNameText");

		if (_sendBirdSystemMessageBG == null)
			LogManager.LogError("Cannot Find _sendBirdSystemMessageBG");

		if (_sendBirdSystemMessageText == null)
			LogManager.LogError("Cannot Find _sendBirdSystemMessageText");

		_onCompleteSlotScaling = onCompleteSlotScaling;
		_sendBirdUI = App.Ins.globalUI.sendBirdUI;

		gameObject.AddComponent<UIScreenScaler>().InitScaler(UIScreenScaler.eUIType.HorOnly);
		gameObject.SetActive(false);
	}

	private void Start()
	{
		_originSize = _rect.rect.size;
	}


	//UI Func====================================================================================
	#region UI Fun
	public void ResetSlot()
	{
		gameObject.SetActive(false);
		if (_scalingCor != null)
			StopCoroutine(_scalingCor);
		_scalingCor = null;
		_slotProcessQueue.Clear();
	}

	public void SettingSendBirdMessageSlot(int slotIndex, int dataIndex, SendBirdManager.sSendBirdMessageData sendBirdMessageData, bool isMove, bool isProcess)
	{
		gameObject.SetActive(true);
		sSendBirdMessageSlotData slotData = new sSendBirdMessageSlotData();
		slotData.SlotIndex = slotIndex;
		slotData.DataIndex = dataIndex;
		slotData.SendBirdMessageData = sendBirdMessageData;
		slotData.IsMove = isMove;
		slotData.IsProcess = isProcess;
		_lastProcessData = sendBirdMessageData;
		_slotProcessQueue.Enqueue(slotData);
		ProcessSlot();
	}

	private void SettingUserMessage(BaseMessage baseMessage)
	{
		_userMessage.SetActive(true);
		_systemMessage.SetActive(false);
		if (_sendBirdUserMessageNameText)
			_sendBirdUserMessageNameText.text = baseMessage.GetSender().UserId;
		if (_sendBirdUserMessageText)
			_sendBirdUserMessageText.text = baseMessage.Message;
	}

	private void SettingSystemMessage(string message)
	{
		_userMessage.SetActive(false);
		_systemMessage.SetActive(true);
		if (_sendBirdSystemMessageText)
			_sendBirdSystemMessageText.text = message;
	}

	private void ProcessSlot()
	{
		if (gameObject.activeSelf && _sendBirdUI && _sendBirdUI.gameObject.activeSelf && _slotProcessQueue.Count > 0 && _scalingCor == null)
		{
			sSendBirdMessageSlotData slotData = _slotProcessQueue.Dequeue();
			if(gameObject.activeSelf)
				_scalingCor = StartCoroutine(Co_DelayForLayout(slotData));
		}
	}
	#endregion
	//====================================================================================UI Func


	private IEnumerator Co_DelayForLayout(sSendBirdMessageSlotData slotData)
	{
		if (string.IsNullOrWhiteSpace(slotData.SendBirdMessageData.SystemMessage) == false)
		{
			//시스템 메세지
			SettingSystemMessage(slotData.SendBirdMessageData.SystemMessage);
		}
		else if (slotData.SendBirdMessageData.BaseMessage != null)
		{
			//채팅 메세지
			SettingUserMessage(slotData.SendBirdMessageData.BaseMessage);
		}
		else
		{
			LogManager.LogError("SendBirdMessageSlot Type Error");
		}

		yield return null;

		if (_userMessage.activeSelf)
		{
			_rect.sizeDelta = new Vector2(_originSize.x, _originSize.y + _sendBirdUserMessageBG.rectTransform.rect.height);
		}
		else if (_systemMessage.activeSelf)
		{
			_rect.sizeDelta = new Vector2(_originSize.x, _originSize.y + _sendBirdSystemMessageBG.rectTransform.rect.height);
		}
		else
		{
			LogManager.LogError("Co_DelayForLayout Type Error");
		}

		if (slotData.IsProcess)
			_onCompleteSlotScaling?.Invoke(slotData.SlotIndex, slotData.DataIndex, _rect.rect.height, slotData.IsMove);
		_scalingCor = null;
		ProcessSlot();
	}
}
