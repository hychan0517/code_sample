using UnityEngine;
using SendBird;
using System.Collections.Generic;
using System;

public class SendBirdManager : MonoBehaviour
{
	public enum eSendBirdErrorCode
	{
		NOT_FOUND_USER = 400201,
		MUTE_STATE = 900041,                //비속어 사용으로 차단됨
	}

	public struct sSendBirdMessageData
	{
		//TODO : 시스템용으로 변수가하나 더 생기고 구조체를 만드는것보단 현재 사용하는게 user_id, message정도이니 그두개 데이터만 뽑아서 가져가는게 낫지않을까..
		public BaseMessage BaseMessage;
		public string SystemMessage;
	}

	public static readonly string DEV_APP_ID = "58865B54-DCAB-43FE-AE75-5BF4500729C2";

	/// <summary> 채널생성에 사용하는 Custom Type 현재는 General만사용 </summary>
	public static readonly string GENERAL_CHANNEL_CUSTOM_TYPE = "General";

	private Coroutine _sendBirdDispatcherCor;
	/// <summary> 탐색된 오픈채널 리스트 </summary>
	private List<OpenChannel> _findedOpenChannelList = new List<OpenChannel>();
	/// <summary> 연결된 오픈채널 </summary>
	private OpenChannel _enteredOpenChannel;

	//테스트용. 개발서버만 오픈,클럽채널에서 이전메세지 불러옴
	private const int PREVIOUS_MESSAGE_LIMIT = 50;

	/// <summary> 센드버드 초기화 확인용 플래그(초기화는 한번만 호출해야함) </summary>
	private bool _sendBirdClienInitFlag;

	/// <summary> 채팅에서 보여지는 유저정보는 센드버드의 userID를 이용하여 GetUserProfile로 받은 데이터를 캐싱함. 패킷 요청에 사용하는 Queue </summary>
	private Queue<long> _requestUserProfileQueue = new Queue<long>();
	public bool isConnect;

	#region Events
	private Action<sSendBirdMessageData> _onReceiveOpenChannelMessage;
	/// <summary> 모든채널 새로운 메세지 알림 </summary>
	private Action _onReceiveNewMessage;
	/// <summary> 센드버드 연결 상태에따른 UI, SendBirdManager 초기화 </summary>
	private Action<bool> _onUpdateConnectState;
	#endregion

	#region Singleton
	// instance
	private static bool _destroyFlag;
	private static SendBirdManager _instance = null;
	//===============================================================================================
	public static SendBirdManager GetInstance()
	{
		if (_destroyFlag)
		{
			return null;
		}
		else if (_instance == null)
		{
			_instance = (SendBirdManager)FindObjectOfType(typeof(SendBirdManager));
			if (!_instance)
			{
				GameObject container = new GameObject();
				container.name = "SendBirdManager";
				_instance = container.AddComponent(typeof(SendBirdManager)) as SendBirdManager;
			}
			DontDestroyOnLoad(_instance.gameObject);
		}

		return _instance;
	}
	#endregion

	private void Awake()
	{
		GetInstance();
	}

	//Init===============================================================================================
	#region Init
	/// <summary> 센드버드 연결. AccountIdx와 Nickname사용하므로 Lobby첫 진입에 실행 </summary>
	public void ConnectSendBird()
	{
		//if (SendBirdClient.GetConnectionState() == SendBirdClient.ConnectionState.CLOSED && BicycleUtil.GetMemberShipType() != MemberShipType.Basic)
		LogManager.Log(string.Format("SendBird State : {0}", SendBirdClient.GetConnectionState()));
		if (SendBirdClient.GetConnectionState() == SendBirdClient.ConnectionState.CLOSED)
		{
			ResetData();
			isConnect = true;

			if (_sendBirdClienInitFlag == false)
			{
				_sendBirdClienInitFlag = true;

				//1. DontDestroyObject설정
				SendBirdClient.SetupUnityDispatcher(gameObject);

				//2. 시작
				_sendBirdDispatcherCor = StartCoroutine(SendBirdClient.StartUnityDispatcher);

				//3. AppID 설정 (한번만 호출되어야함)
				SendBirdClient.Init(GetAPPID());

#if BICYCLE_LOGGING
				//4. 로그이벤트 연결
				SendBirdClient.Log += (message) =>
				{
					LogManager.Instance.PrintLog(LogManager.eLogType.ReceiveNetworkWebSocket, message);
				};
#endif
				//5. Receive 이벤트 연결
				SendBirdClient.ChannelHandler channelHandler = new SendBirdClient.ChannelHandler();
				channelHandler.OnMessageReceived = (BaseChannel baseChannel, BaseMessage baseMessage) =>
				{
					sSendBirdMessageData sendBirdMessageData = new sSendBirdMessageData();
					sendBirdMessageData.BaseMessage = baseMessage;
					sendBirdMessageData.SystemMessage = string.Empty;

					if (baseChannel == _enteredOpenChannel)
					{
						ActiveReceiveNewMessage();
						ActiveReceiveOpenChannelMessageEvent(sendBirdMessageData);
					}
				};
				//무슨의미..?
				SendBirdClient.AddChannelHandler("default", channelHandler);
			}
			else
			{
				_sendBirdDispatcherCor = StartCoroutine(SendBirdClient.StartUnityDispatcher);
			}

			//5. 연결, 토큰설정
			// 5-1. public static void Connect(string userId, ConnectHandler handler);
			// 5-2. public static void Connect(string userId, string accessToken, ConnectHandler handler);
			//5-1 함수 사용시 토큰 유니크값으로 자동생성(디버깅용으로 활용)
			LogManager.StartLog("ConnectSendBird");
			//잘못된 Access Token입력해도 접속 잘됨
			//클라에서 만들어서 야핏서버에 전달할수도 있고 서버에서 미리 만들어서 할당하는것도 가능
			//AccessToken이 만들어진 유저에 한해서 잘못된 AccessToken입력하면 에러
			SendBirdClient.Connect(App.Ins.userDataTable.nickName, (User user, SendBirdException e) =>
			{
				if (e != null)
				{
					LogManager.LogError("ConnectSendBird Failed");
					LogManager.LogError(e.ToString());
					LogManager.EndLog("ConnectSendBird");
					ResetData();
				}
				else
				{
					LogManager.EndLog("ConnectSendBird");

					ActiveUpdateConnectState(true);

					//6. 오픈채널 탐색
					EnterOpenChannel();
				}
			});
		}
	}

	private void KillDispatcherCoroutine()
	{
		if (_sendBirdDispatcherCor != null)
			StopCoroutine(_sendBirdDispatcherCor);
		_sendBirdDispatcherCor = null;
	}

	private string GetAPPID()
	{
		switch (App.Ins.GetConnectServerType())
		{
			case GlobalDefine.eConnectServerType.Dev:
				return DEV_APP_ID;
			default:
				LogManager.LogError("GetAPPID eConnectServerType Error");
				return string.Empty;
		}
	}
	#endregion
	//===============================================================================================Init

	//Open===============================================================================================
	#region Open
	/// <summary> 오픈채널 리스트 불러오기 -> UI표기 </summary>
	private void EnterOpenChannel()
	{
		if (IsConnectSendbird())
		{
			LogManager.StartLog("GetOpenChannelList");
			_findedOpenChannelList.Clear();
			OpenChannelListQuery openChannelListQuery = OpenChannel.CreateOpenChannelListQuery();
			openChannelListQuery.NameKeyword = "OpenChannel_1";
			openChannelListQuery.Next((List<OpenChannel> channels, SendBirdException e) =>
			{
				if (e == null)
				{
					//오픈채널 불러오기 성공
					_findedOpenChannelList = channels;
#if BICYCLE_LOGGING
					foreach (OpenChannel i in _openChannelList)
					{
						LogManager.NetworkLog(string.Format("Find Channel : {0}", i.Name));
					}
#endif
					LogManager.EndLog("GetOpenChannelList");
					EnterOpenChannelByOpenChannelIndex(0);
				}
				else
				{
					//오픈채널 불러오기 실패
					LogManager.LogError("GetOpenChannelList Failed");
					LogManager.LogError(e.ToString());
					LogManager.EndLog("GetOpenChannelList");
				}
			});
		}
	}

	/// <summary> 수신된 Open타입 채널 리스트 채널인덱스로 오픈채널 입장 </summary>
	private void EnterOpenChannelByOpenChannelIndex(int channelIndex)
	{
		if (IsConnectSendbird())
		{
			if (_findedOpenChannelList.Count <= channelIndex)
			{
				//채널 인덱스 에러
				LogManager.LogError(string.Format("EnterOpenChannel Index Error. ChannelCount : {0}, Index : {1}", _findedOpenChannelList.Count, channelIndex));
			}
			else
			{
				LogManager.StartLog("EnterOpenChannel");
#if BICYCLE_LOGGING
				LogManager.Instance.PrintLog(LogManager.eLogType.SendNetworkHTTP, string.Format("EnterOpenChannel URL : {0}", _openChannelList[channelIndex].Url));
#endif
				OpenChannel.GetChannel(_findedOpenChannelList[channelIndex].Url, (OpenChannel openChannel, SendBirdException e) =>
				{
					if (e != null)
					{
						//채널 불러오기 실패
						LogManager.LogError("EnterOpenChannel GetChannel Error");
						LogManager.LogError(e.ToString());
						LogManager.EndLog("EnterOpenChannel");
					}
					else
					{
						openChannel.Enter((SendBirdException e) =>
						{
							if (e != null)
							{
								//채널입장 실패
								LogManager.LogError("EnterOpenChannel Enter Error");
								LogManager.LogError(e.ToString());
								LogManager.EndLog("EnterOpenChannel");
							}
							else
							{
								//채널입장 성공
								LogManager.NetworkLog(string.Format("EnterChannel Success Name : {0}", openChannel.Name));
								LogManager.EndLog("EnterOpenChannel");
								_enteredOpenChannel = openChannel;
								LoadEnteredOpenChannelPreviousMessages();
							}
						});
					}
				});
			}
		}
	}

	/// <summary> 입장한 오픈채널의 이전 메세지 불러오기 </summary>
	private void LoadEnteredOpenChannelPreviousMessages()
	{
		if (IsConnectSendbird())
		{
			if (_enteredOpenChannel != null)
			{
				if (PREVIOUS_MESSAGE_LIMIT > 0)
				{
					LogManager.StartLog("LoadEnteredOpenChannelPreviousMessages");
					// 채널 보기당 하나의 인스턴스만 있어야 합니다. 
					PreviousMessageListQuery mListQuery = _enteredOpenChannel.CreatePreviousMessageListQuery();
					mListQuery.Load(PREVIOUS_MESSAGE_LIMIT, true, (List<BaseMessage> message, SendBirdException e) =>
					{
						if (e != null)
						{
							LogManager.LogError("LoadEnteredOpenChannelPreviousMessages Error");
							LogManager.LogError(e.ToString());
							LogManager.EndLog("LoadEnteredOpenChannelPreviousMessages");
						}
						else
						{
							LogManager.EndLog("LoadEnteredOpenChannelPreviousMessages");
							if (message != null)
							{
								for (int i = message.Count - 1; i >= 0; --i)
								{
									sSendBirdMessageData sendBirdMessageData = new sSendBirdMessageData();
									sendBirdMessageData.BaseMessage = message[i];
									sendBirdMessageData.SystemMessage = string.Empty;
									ActiveReceiveOpenChannelMessageEvent(sendBirdMessageData);
								}
							}
						}
					});
				}
			}
			else
			{
				//참여한 오픈채널 없음
				LogManager.LogError("LoadEnteredOpenChannelPreviousMessages Error EnteredOpenChannel is null");
				LogManager.EndLog("LoadEnteredOpenChannelPreviousMessages");
			}
		}
	}

	/// <summary> 입장한 오픈채널에 메세지 전송 </summary>
	public void SendUserMessageToEnteredOpenChannel(string message)
	{
		if (IsConnectSendbird())
		{
			if (_enteredOpenChannel != null)
			{
				_enteredOpenChannel.SendUserMessage(message, (UserMessage userMessage, SendBirdException e) =>
				{
					if (e == null)
					{
#if BICYCLE_LOGGING
						LogManager.NetworkLog("SendUserMessageToEnteredOpenChannel Success");
#endif
						sSendBirdMessageData sendBirdMessageData = new sSendBirdMessageData();
						sendBirdMessageData.BaseMessage = userMessage;
						sendBirdMessageData.SystemMessage = string.Empty;
						ActiveReceiveOpenChannelMessageEvent(sendBirdMessageData);

						//센드버드의 비속어필터링. 제한이 클라기능으로 변경되어 사용안하지만 코드 남김
						//비속어 -> "**"대체옵션사용할 경우 여기서 카운팅하고
						//비속어사용했을 때 전송을 막도록 옵션이되었을경우 아래 ErrorCode로 확인하여야함
						//if (string.Equals(message, userMessage.Message) == false)
						//{
						//	//비속어 사용
						//	GetSendBirdUI().AddBadWordInfo(message);
						//}
					}
					else
					{
						LogManager.LogError("SendUserMessageToEnteredOpenChannel Error");
						LogManager.LogError(e.ToString());
					}
				});
			}
			else
			{
				//참여한 오픈채널 없음
				LogManager.LogError("SendUserMessageToEnteredOpenChannel Error Not Entered OpenChannel");
			}
		}
	}

	/// <summary> 입장한 오픈채널에서 퇴장 </summary>
	public void ExitOpenChannel()
	{
		if (IsConnectSendbird())
		{
			if (_enteredOpenChannel != null)
			{
				LogManager.StartLog("ExitOpenChannel");
				OpenChannel.GetChannel(_enteredOpenChannel.Url, (OpenChannel openChannel, SendBirdException e) =>
				{
					if (e == null)
					{
						openChannel.Exit((SendBirdException e) =>
						{
							if (e == null)
							{
								_enteredOpenChannel = null;
								LogManager.NetworkLog(string.Format("ExitOpenChannel Success Name : {0}", openChannel.Name));
								LogManager.EndLog("ExitOpenChannel");
							}
							else
							{
								LogManager.LogError("ExitOpenChannel Exit Error");
								LogManager.LogError(e.ToString());
								LogManager.EndLog("ExitOpenChannel");
							}
						});
					}
					else
					{
						//GetChannel 실패
						LogManager.LogError("ExitOpenChannel GetChannel Error");
						LogManager.LogError(e.ToString());
						LogManager.EndLog("ExitOpenChannel");
					}
				});
			}
			else
			{
				//참여한 오픈채널 없음
				LogManager.LogError("ExitOpenChannel Error EnteredOpenChannel is null");
				LogManager.EndLog("ExitOpenChannel");
			}
		}
	}
	#endregion
	//===============================================================================================Open


	//Custom Events======================================================================================
	#region Custom Events
	//Open
	public void AddReceiveOpenChannelMessageEvent(Action<sSendBirdMessageData> message)
	{
		_onReceiveOpenChannelMessage += message;
	}

	public void ActiveReceiveOpenChannelMessageEvent(sSendBirdMessageData message)
	{
		_onReceiveOpenChannelMessage?.Invoke(message);
	}

	//New Message
	public void AddReceiveNewMessage(Action newMessage)
	{
		_onReceiveNewMessage += newMessage;
	}

	public void RemoveReceiveNewMessage(Action newMessage)
	{
		_onReceiveNewMessage -= newMessage;
	}

	public void ActiveReceiveNewMessage()
	{
		_onReceiveNewMessage?.Invoke();
	}

	//Connect
	public void AddUpdateConnectState(Action<bool> updateConnectStateEvent)
	{
		_onUpdateConnectState += updateConnectStateEvent;
	}

	public void ActiveUpdateConnectState(bool isConnect)
	{
		_onUpdateConnectState?.Invoke(isConnect);
	}
	#endregion
	//======================================================================================Custom Events


	private bool IsConnectSendbird()
	{
		if (SendBirdClient.GetConnectionState() != SendBirdClient.ConnectionState.OPEN)
		{
			//서버 연결 끊김
			ResetData();
			LogManager.LogError("SendBird Failed Not Connected Server");
			return false;
		}
		else
		{
			return true;
		}
	}

	public void DisconnectSendBird()
	{
		if (SendBirdClient.GetConnectionState() != SendBirdClient.ConnectionState.CLOSED)
		{
			Debug.Log("DisconnectSendBird");
			SendBirdClient.Disconnect(() =>
			{
				Debug.Log("DisconnectSendBird Complete");
				ResetData();
			});
		}
		else
		{
			ResetData();
			Debug.Log("SendBird Already Disconnected");
		}
	}

	private void ResetData()
	{
		ActiveUpdateConnectState(false);
		isConnect = false;
		_findedOpenChannelList.Clear();
		_enteredOpenChannel = null;
		_requestUserProfileQueue.Clear();
		KillDispatcherCoroutine();
	}

	private void OnDestroy()
	{
		_destroyFlag = true;
		DisconnectSendBird();
		_onReceiveOpenChannelMessage = null;
		_onReceiveNewMessage = null;
		_onUpdateConnectState = null;
	}
}
