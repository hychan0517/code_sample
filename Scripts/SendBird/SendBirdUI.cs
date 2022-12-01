using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SendBirdManager;

public class SendBirdUI : MonoBehaviour
{
	[SerializeField]
	private Canvas _sendBirdCanvas;
	[SerializeField]
	private RectTransform _rect;
	[SerializeField]
	private CanvasGroup _canvasGroup;
	[SerializeField]
	private GameObject _mainUI;


	[Header("Preview")]
	[SerializeField]
	private GameObject _previewPanel;
	[SerializeField]
	private TextMeshProUGUI _previewMessageText;
	[SerializeField]
	private Button _sendbirdButton;

	[Header("Top")]
	[SerializeField]
	private Button _closeButton;

	[Header("Chatting")]
	[SerializeField]
	private RectTransform _chattingContent;
	[SerializeField]
	private TMP_InputField _messageInputField;
	[SerializeField]
	private Button _sendButton;

	[Header("Scroll")]
	[SerializeField]
	private ScrollRect _chattingScrollView;
	[SerializeField]
	private RectTransform _chattingScrollViewRect;
	[SerializeField]
	private GameObject _sendBirdMessageSlotSample;

	[Header("NewMessage")]
	[SerializeField]
	private GameObject _newMessagePanel;
	[SerializeField]
	private Button _newMessageButton;

	[Header("Disconnect")]
	[SerializeField]
	private GameObject _disconnectPanel;

	//도배
	/// <summary> 유저 메세지 전송 히스토리 </summary>
	private DateTime[] _checkForSpam = new DateTime[SPAM_CHECK_COUNT];
	private int _checkForSpamIndex;
	private int _sendCount;
	/// <summary> n초 동안 </summary>
	private const float SPAM_CHECK_SECONDS = 10f;
	/// <summary> n회 입력시 </summary>
	private const int SPAM_CHECK_COUNT = 5;
	/// <summary> n초 동안 채팅금지 </summary>
	private const float SPAM_BLOCK_SECONDS = 30f;

	//비속어 채팅금지
	public class BanChattingPacket
	{
		public string Message { get; set; }
		public long SendTime { get; set; }
	}
	/// <summary> 유저가 입력한 비속어 정보 </summary>
	private Dictionary<int, BanChattingPacket> _badChatDataDict = new Dictionary<int, BanChattingPacket>();
	private int _badChatIndex;
	private int BadChatIndex { get { return _badChatIndex++; } set { _badChatIndex = value; } }
	//비속어 사용, 도배에의한 차단상태
	private bool _isBlock;
	private Coroutine _blockCor;
	//TODO : 번들. 또는 테이블로라도..
	/// <summary> BAD_WORD_BLOCK_CHECK_SECONDS안에 BAD_WORD_BLOCK_COUNT번 욕설하면 채팅제한 </summary>
	private const float BAD_WORD_BLOCK_CHECK_SECONDS = 10;
	/// <summary> BAD_WORD_BLOCK_CHECK_SECONDS안에 BAD_WORD_BLOCK_COUNT번 욕설하면 채팅제한 </summary>
	private const int BAD_WORD_BLOCK_COUNT = 3;
	/// <summary> 비속어 사용으로인한 채팅제한 초 </summary>
	private const float BAD_WORD_BLOCK_SECONDS = 300;

	//Scroll
	private Dictionary<int, float> _slotHeightDataDict = new Dictionary<int, float>();
	private List<sSendBirdMessageData> _openChannelMessageList = new List<sSendBirdMessageData>();
	private List<SendBirdMessageSlot> _slotList = new List<SendBirdMessageSlot>();
	private float _totalHeight;
	private Vector2 _slotSize;
	private float _spacing = 0;
	private int _showCount;  // 화면에 보여질 슬롯
	private int _slotIndex = 0;
	private int _dataIndex = 0;

	/// <summary> 새로운메세지 수신시 스크롤 최하단으로 이동시킬것인지 </summary>
	private bool _isAutoScroll;

	public void InitUIObject()
	{
		if (_sendBirdCanvas == null)
			LogManager.LogError("Cannot Find _sendBirdCanvas");

		if (_rect == null)
			LogManager.LogError("Cannot Find _rect");

		if (_canvasGroup == null)
			LogManager.LogError("Cannot Find _canvasGroup");

		if (_sendbirdButton)
			_sendbirdButton.onClick.AddListener(SettingSendBirdUI);

		//Top
		if (_closeButton == null)
			LogManager.LogError("Cannot Find _closeButton");
		else
			_closeButton.onClick.AddListener(OnClickCloseButton);

		//Chatting
		if (_chattingContent)
			_chattingContent.gameObject.AddComponent<UINotchController>().InitController(UINotchController.eChangeTypeOfNotch.SideScale, 0.75f);
		else
			LogManager.LogError("Cannot Find _chattingContent");

		if (_messageInputField == null)
			LogManager.LogError("Cannot Find _messageInputField");

		if (_sendButton == null)
			LogManager.LogError("Cannot Find _sendButton");
		else
			_sendButton.onClick.AddListener(OnClickSendButton);

		//Scroll
		if (_chattingScrollView == null)
			LogManager.LogError("Cannot Find _scrollView");
		else
			_chattingScrollView.onValueChanged.AddListener(OnValueChangedScroll);

		if (_chattingScrollViewRect == null)
			LogManager.LogError("Cannot Find _scrollViewRect");

		if (_sendBirdMessageSlotSample == null)
			LogManager.LogError("Cannot Find _sendBirdMessageSlotSample");
		else
			_sendBirdMessageSlotSample.SetActive(false);

		//Disconnect
		if (_disconnectPanel == null)
			LogManager.LogError("Cannot Find _disconnectPanel");
		else
			_disconnectPanel.SetActive(false);

		//New Message
		if (_newMessagePanel == null)
			LogManager.LogError("Cannot Find _newMessagePanel");
		else
			_newMessagePanel.SetActive(false);

		if (_newMessageButton == null)
			LogManager.LogError("Cannot Find _newMessageButton");
		else
			_newMessageButton.onClick.AddListener(OnClickNewMessageButton);

		SendBirdManager.GetInstance().AddReceiveOpenChannelMessageEvent(OnReceiveOpenChannelMessage);
		SendBirdManager.GetInstance().AddReceiveNewMessage(ReceiveNewMessage);
		SendBirdManager.GetInstance().AddUpdateConnectState(UpdateConnectState);

		gameObject.SetActive(true);

		if(_previewPanel)
			_previewPanel.SetActive(false);

		SetActiveOffMainUI();
	}


	//UI Func====================================================================================
	#region UI Fun
	/// <summary> UI 활성화 </summary>
	public void SettingSendBirdUI()
	{
		AudioManager.Ins.PlayClick();
		App.Ins.PushBackButtonEvent(SetActiveOffMainUI);
		SetActiveMainUI(true);
		_isAutoScroll = true;

		//새로운 메세지 초기화
		SendBirdManager.GetInstance().ActiveReceiveNewMessage();

		//슬롯 및 스크롤 초기화는 토글 이벤트에서
		SetActiveNewMessageButton(false);
		SettingConnectState(SendBirdManager.GetInstance().isConnect);

		if (SendBirdManager.GetInstance().isConnect == false)
		{
			SendBirdManager.GetInstance().ConnectSendBird();
			LogManager.Log("SendBird Reconnect");
		}
		else
		{
			SettingScroll();
		}
	}


	/// <summary> 슬롯마다 높이를 계산을위해 첫 슬롯부터 SettingSlot하게되는데 눈에보이지않도록 Alpha값 제어 </summary>
	private void SetAlpha(bool isAlpha)
	{
		if (_canvasGroup && _chattingScrollView)
		{
			if (isAlpha)
			{
				_chattingScrollView.vertical = false;
				_canvasGroup.alpha = 0;
			}
			else
			{
				_chattingScrollView.vertical = true;
				_canvasGroup.alpha = 1;
			}
		}
	}

	private void SettingConnectState(bool isConnect)
	{
		if (_disconnectPanel)
			_disconnectPanel.SetActive(!isConnect);
		if (_previewPanel)
			_previewPanel.SetActive(isConnect);
	}

	private void SetActiveMainUI(bool isActive)
	{
		if (_mainUI)
			_mainUI.SetActive(isActive);
	}

	private void SetActiveOffMainUI()
	{
		if (_mainUI)
			_mainUI.SetActive(false);
	}

	#endregion
	//====================================================================================UI Func


	//Spam=======================================================================================
	#region Spam
	private void NextCheckForSpamIndex()
	{
		++_checkForSpamIndex;
		if (_checkForSpamIndex >= SPAM_CHECK_COUNT)
			_checkForSpamIndex = 0;
	}

	private void CheckForSpam()
	{
		++_sendCount;
		if (_sendCount >= SPAM_CHECK_COUNT)
		{
			int checkIndex = _sendCount % SPAM_CHECK_COUNT;
			if ((DateTime.Now - _checkForSpam[checkIndex]).TotalSeconds <= SPAM_CHECK_SECONDS)
			{
				ChattingBlock(SPAM_BLOCK_SECONDS);
			}
		}
	}
	#endregion
	//=======================================================================================Spam


	//Bad Word====================================================================================
	#region Bad Word
	/// <summary> 최근30초동안 욕설 n번이상 했는지 확인 </summary>
	public void AddBadWordInfo(string originMessage)
	{
		_badChatDataDict.Add(BadChatIndex, new BanChattingPacket() { Message = originMessage, SendTime = DateTime.Now.Ticks });
		if (_badChatDataDict.Count >= BAD_WORD_BLOCK_COUNT)
		{
			//_chatBlcokDefaultValue.Count 이전 메세지부터 이번 메세지까지 경과시간이 _chatBlcokDefaultValue.Timesec 이내라면 채팅금지
			int index = _badChatDataDict.Count - BAD_WORD_BLOCK_COUNT;
			if (_badChatDataDict.ContainsKey(index))
			{
				TimeSpan timeSpan = DateTime.Now - new DateTime(_badChatDataDict[index].SendTime);
				if (timeSpan.TotalSeconds <= BAD_WORD_BLOCK_CHECK_SECONDS)
				{
					//설정 시간동안 설정 횟수만큼 욕설반복사용. 채팅금지
					List<BanChattingPacket> chattings = new List<BanChattingPacket>();
					for (int i = index; i < _badChatIndex; ++i)
					{
						//사용한 비속어 정보 서버 전달
						if (_badChatDataDict.ContainsKey(i))
							chattings.Add(_badChatDataDict[i]);
					}

					//시스템 메세지
					OnReceiveSystemMessage("chat_system_message_bad_word_block");

					ChattingBlock(BAD_WORD_BLOCK_SECONDS);

					//비속어 사용 정보 초기화
					_badChatDataDict.Clear();
					_badChatIndex = 0;
				}
				else
				{
					OnReceiveSystemMessage("chat_system_message_bad_word");
				}
			}
			else
			{
				LogManager.LogError("CheckChattingBan Err");
			}
		}
		else
		{
			//전체시간 중 비속어 사용 횟수 초과하지 않아서 계산 필요없이 시스템메세지 표기
			OnReceiveSystemMessage("chat_system_message_bad_word");
		}
	}
	#endregion
	//====================================================================================Bad Word


	//Block=======================================================================================
	#region Block
	private void ChattingBlock(float blockSeconds)
	{
		_isBlock = true;
		KillBlockCor();
		StartCoroutine(Co_WaitForBlock(blockSeconds));
	}

	private void KillBlockCor()
	{
		if (_blockCor != null)
			StopCoroutine(_blockCor);
		_blockCor = null;
	}

	private IEnumerator Co_WaitForBlock(float blockSeconds)
	{
		float deltaTime = 0;
		while (deltaTime <= blockSeconds)
		{
			deltaTime += Time.deltaTime;
			yield return null;
		}

		_blockCor = null;
		_isBlock = false;
	}
	#endregion
	//=======================================================================================Block


	//=================================================================================New Message
	#region New Message	
	private void SettingPreviewText(sSendBirdMessageData message)
	{
		if(_previewMessageText && string.IsNullOrWhiteSpace(message.SystemMessage))
		{
			_previewMessageText.text = string.Format("{0}:{1}", message.BaseMessage.GetSender().UserId, message.BaseMessage.Message);
		}
	}

	private void ReceiveNewMessage()
	{
		//채팅 패널의 새로운메세지가 있습니다 버튼활성화
		if (_isAutoScroll == false)
		{
			SetActiveNewMessageButton(true);
		}
	}

	private void SetActiveNewMessageButton(bool isActive)
	{
		if (_newMessagePanel)
			_newMessagePanel.SetActive(isActive);
	}
	#endregion
	//=================================================================================New Message


	//Scroll=====================================================================================
	#region Scroll
	private void SettingScroll()
	{
		CreateSlot();
		ResetSlot();
		ResetScrollView();
		SettingSlotDefault(true);
	}

	private void CreateSlot()
	{
		if (_slotList.Count == 0 && _sendBirdMessageSlotSample)
		{
			// 슬롯 크기
			Rect realSlotSize = _sendBirdMessageSlotSample.GetComponent<RectTransform>().rect;

			// 비율 적용된 슬롯 사이즈
			_slotSize = new Vector2(realSlotSize.width, realSlotSize.height + _spacing);

			// 화면에 보여질 슬롯 수
			var showCount = Convert.ToInt32(Math.Truncate(_chattingScrollViewRect.rect.height / _slotSize.y));
			_showCount = showCount;

			// 기본 슬롯 생성,배치
			for (int i = 0; i < _showCount; i++)
			{
				var slot = Instantiate(_sendBirdMessageSlotSample, _chattingContent.transform);
				slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -_slotSize.y * i);
				SendBirdMessageSlot messageSlot = slot.GetComponent<SendBirdMessageSlot>();
				messageSlot.InitUIObject(OnCompleteMessageSlotScaling);
				_slotList.Add(slot.GetComponent<SendBirdMessageSlot>());
			}
		}
	}

	private void ResetSlot()
	{
		foreach (SendBirdMessageSlot i in _slotList)
		{
			i.ResetSlot();
		}
	}

	private void ResetScrollView()
	{
		_chattingScrollView.velocity = Vector2.zero;
		_chattingContent.anchoredPosition = Vector2.zero;
		_chattingContent.sizeDelta = new Vector2(_chattingContent.sizeDelta.x, 0);
		_slotIndex = 0;
		_dataIndex = 0;
		_totalHeight = 0;
		_slotHeightDataDict.Clear();
	}

	private void SettingSlotDefault(bool isReset)
	{
		if (isReset)
		{
			SetAlpha(true);
		}
		else
		{
			float scrollPosition = _chattingScrollViewRect.rect.height + _chattingContent.anchoredPosition.y;
			if (scrollPosition / _chattingContent.rect.height <= 0.95f)
			{
				SetAlpha(true);
			}
		}

		int count = _openChannelMessageList.Count;

		for (int i = _dataIndex; i < count; i++)
		{
			SettingSlot(i % _showCount, i, true);
		}

		if (count > _showCount)
		{
			_slotIndex = count % _showCount;
			_dataIndex = count - _showCount;
		}
		else
		{
			_slotIndex = 0;
			_dataIndex = 0;
		}
	}

	/// <summary> 스크롤을 최하단으로 이동 </summary>
	private void MoveToScrollEndPosition()
	{
		if (_chattingContent.rect.height > _chattingScrollViewRect.rect.height)
		{
			_chattingContent.anchoredPosition = new Vector2(_chattingContent.anchoredPosition.x, _chattingContent.rect.height - _chattingScrollViewRect.rect.height);
		}
		else
		{
			_chattingContent.anchoredPosition = new Vector2(_chattingContent.anchoredPosition.x, 0);
		}

		_chattingScrollView.velocity = Vector2.zero;
	}

	private void OnValueChangedScroll(Vector2 scrollPosition)
	{
		CheckScroll();
		CheckAutoScroll(scrollPosition.y);
	}

	private void CheckAutoScroll(float scrollPositionY)
	{
		if (scrollPositionY >= 0.05f)
		{
			//스크롤 수동이동하면 자동스크롤 해제
			if (Input.GetMouseButton(0) || Input.touchCount != 0)
			{
				_isAutoScroll = false;
			}
		}
		else
		{
			SetActiveNewMessageButton(false);
			_isAutoScroll = true;
		}
	}

	private void CheckScroll()
	{
		if (_chattingScrollView.vertical)
		{
			if (_chattingContent.anchoredPosition.y > GetSlotHeightByDataIndex(_dataIndex + 1))
			{
				DownScroll();
			}
			else if (_chattingContent.anchoredPosition.y < GetSlotHeightByDataIndex(_dataIndex))
			{
				UpScroll();
			}
		}
	}

	private float GetSlotHeightByDataIndex(int dataIndex)
	{
		if (_slotHeightDataDict.ContainsKey(dataIndex))
			return _slotHeightDataDict[dataIndex];
		else
			return -1;
	}

	private void DownScroll()
	{
		int count = _openChannelMessageList.Count;

		if (_dataIndex + _showCount < count)
		{
			SettingSlot(_slotIndex, _dataIndex + _showCount, false);
			SetNextSlotIndex();
			CheckScroll();
		}
	}

	private void UpScroll()
	{
		if (_dataIndex > 0)
		{
			SetPreviousSlotIndex();
			SettingSlot(_slotIndex, _dataIndex, false);
			CheckScroll();
		}
	}

	private void SetNextSlotIndex()
	{
		//최상단 슬롯의 인덱스 갱신
		_dataIndex++;
		if (_slotIndex == _slotList.Count - 1)
		{
			_slotIndex = 0;
		}
		else
		{
			++_slotIndex;
		}
	}

	private void SetPreviousSlotIndex()
	{
		//최상단 슬롯의 인덱스 갱신
		--_dataIndex;
		if (_slotIndex == 0)
		{
			_slotIndex = _slotList.Count - 1;
		}
		else
		{
			_slotIndex = _slotIndex - 1;
		}
	}

	/// <param name="isMove"> 스크롤 최하단으로 이동시킬것인지 </param>
	private void SettingSlot(int slotIndex, int dataIndex, bool isMove)
	{
		int dataCount = _openChannelMessageList.Count;
		//슬롯 높이 계산을 해야하는지
		bool isProcess = _slotHeightDataDict.ContainsKey(dataIndex) ? false : true;

		if (slotIndex >= 0 && dataIndex >= 0 && slotIndex < _slotList.Count && dataIndex < dataCount)
		{
			_slotList[slotIndex].SettingSendBirdMessageSlot(slotIndex, dataIndex, _openChannelMessageList[dataIndex], isMove, isProcess);
		}

		//계산 끝난 슬롯
		if (isProcess == false)
		{
			if (slotIndex >= 0 && dataIndex >= 0 && slotIndex < _slotList.Count && dataIndex < dataCount)
			{
				_slotList[slotIndex].transform.localPosition = new Vector3(_slotList[slotIndex].transform.localPosition.x,
																		-GetSlotYPositionByDataIndex(dataIndex),
																		_slotList[slotIndex].transform.localPosition.z);
			}
		}
	}

	/// <summary> 슬롯에서 ContentSizeFitter로 계산 끝난 후 콜백 </summary>
	/// <param name="height"> 계산된 슬롯의 높이(스크롤 코드에 사용) </param>
	/// <param name="isMove"> 마지막 슬롯인지 </param>
	private void OnCompleteMessageSlotScaling(int slotIndex, int dataIndex, float height, bool isMove)
	{
		if (_slotHeightDataDict.ContainsKey(dataIndex) == false)
		{
			_slotHeightDataDict.Add(dataIndex, _totalHeight);
			_totalHeight += height;
			_chattingContent.sizeDelta = new Vector2(_chattingContent.sizeDelta.x, _totalHeight);
		}

		int dataCount = _openChannelMessageList.Count;

		if (slotIndex >= 0 && dataIndex >= 0 && slotIndex < _slotList.Count && dataIndex < dataCount)
		{
			_slotList[slotIndex].transform.localPosition = new Vector3(_slotList[slotIndex].transform.localPosition.x,
																	-GetSlotYPositionByDataIndex(dataIndex),
																	_slotList[slotIndex].transform.localPosition.z);
		}

		if (isMove)
		{
			if (dataIndex == _openChannelMessageList.Count - 1)
			{
				//마지막 슬롯이라면 최하단 이동
				MoveToScrollEndPosition();
				//슬롯 활성화
				SetAlpha(false);
			}
		}
	}

	private float GetSlotYPositionByDataIndex(int dataIndex)
	{
		if (_slotHeightDataDict.ContainsKey(dataIndex))
			return _slotHeightDataDict[dataIndex];
		else
			return 0;
	}
	#endregion
	//=====================================================================================Scroll



	//Custom Events==============================================================================
	#region Custom Events
	private void OnReceiveOpenChannelMessage(sSendBirdMessageData message)
	{
		SettingPreviewText(message);
		_openChannelMessageList.Add(message);
		if (gameObject.activeSelf && _isAutoScroll)
			SettingSlotDefault(false);
	}

	public void OnReceiveSystemMessage(string message)
	{
		sSendBirdMessageData sendBirdMessageData = new sSendBirdMessageData();
		sendBirdMessageData.SystemMessage = message;
		_openChannelMessageList.Add(sendBirdMessageData);

		if (_isAutoScroll)
			SettingSlotDefault(false);
	}
	#endregion
	//==============================================================================Custom Events


	//UI Events==================================================================================
	#region UI Events
	private void OnClickCloseButton()
	{
		App.Ins.ActiveBackButtonEvent();
	}

	private void OnClickSendButton()
	{
		AudioManager.Ins.PlayClick();
		if (string.IsNullOrWhiteSpace(_messageInputField.text) == false)
		{
			//메세지 보냈을때는 자동스크롤
			_isAutoScroll = true;
			if (_isBlock == false)
			{
				_checkForSpam[_checkForSpamIndex] = DateTime.Now;
				NextCheckForSpamIndex();
				CheckForSpam();

				if (_isBlock == false)
				{
					string repairStr = App.Ins.badWordDataManager.GetRepairWord(_messageInputField.text);

					SendBirdManager.GetInstance().SendUserMessageToEnteredOpenChannel(repairStr);

					if (repairStr != _messageInputField.text)
						AddBadWordInfo(_messageInputField.text);
				}
				else
				{
					OnReceiveSystemMessage("chat_system_message_blocked");
				}

				_messageInputField.text = string.Empty;
			}
			else
			{
				//제한상태
				_messageInputField.text = string.Empty;
				OnReceiveSystemMessage("chat_system_message_blocked");
			}
		}
	}

	private void OnClickNewMessageButton()
	{
		AudioManager.Ins.PlayClick();
		SetActiveNewMessageButton(false);
		SettingSlotDefault(false);
	}
	#endregion
	//==================================================================================UI Events


	/// <summary> 연결상태에따른 UI초기화 </summary>
	private void UpdateConnectState(bool isConnect)
	{
		if (isConnect)
		{
			if (_mainUI && _mainUI.activeSelf)
				SettingSendBirdUI();
			SettingConnectState(true);
		}
		else
		{
			ResetData();
		}
	}

	/// <summary> 비활성화 상태로 초기화 </summary>
	private void ResetData()
	{
		_openChannelMessageList.Clear();
		_isBlock = false;
		SettingConnectState(false);
		ResetSlot();
		KillBlockCor();
	}

	private void OnDestroy()
	{
		if (_sendbirdButton) _sendbirdButton.onClick.RemoveAllListeners();
		if (_closeButton) _closeButton.onClick.RemoveAllListeners();
		if (_sendButton) _sendButton.onClick.RemoveAllListeners();
		if (_newMessageButton) _newMessageButton.onClick.RemoveAllListeners();
		if (_chattingScrollView) _chattingScrollView.onValueChanged.RemoveAllListeners();
	}
}
