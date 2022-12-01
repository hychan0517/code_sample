using UnityEngine;
using GlobalDefine;
using Photon.Pun;
using UnityEngine.Localization.Settings;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class App : MonoBehaviour
{
	#region SINGLETON
	static App _instance = null;
	private static bool _destroyFlag = false;

	public static App Ins
	{
		get
		{
			if (_destroyFlag)
				return null;

			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(App)) as App;
				if (_instance == null)
				{
					_instance = new GameObject("App", typeof(App)).GetComponent<App>();
				}
			}

			return _instance;
		}
	}
	#endregion

	//Server
	private eConnectServerType _connectServerType = eConnectServerType.Dev;

	//Scene
	private eSceneType _nowSceneState;

	//Manager
	public PlayerSkillDataManager playerSkillDataManager { private set; get; } = new PlayerSkillDataManager();
	public MonsterDataManager monsterDataManager { private set; get; } = new MonsterDataManager();
	public StageDataManager stageDataManager { private set; get; } = new StageDataManager();
	public BadWordDataManager badWordDataManager { private set; get; } = new BadWordDataManager();
	public PlayerPrefsManager playerPrefsManager { private set; get; } = new PlayerPrefsManager();
	public AddressableManager addressableManager { private set; get; } = new AddressableManager();

	//Table
	public UserDataTable userDataTable { private set; get; } = new UserDataTable();
	public OptionDataTable optionDataTable { private set; get; } = new OptionDataTable();

	public GlobalUI globalUI;

	private Stack<Action> _backButtonEventStack = new Stack<Action>();
	public float tweenDurationTime;

	//Init=====================================================================================================================
	#region Init
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		LoadDataManager();
		SettingLanguage();
	}

	private void Start()
	{
		globalUI = FindObjectOfType<GlobalUI>(true);
		if (globalUI)
			globalUI.InitUIObject();
		else
			LogManager.LogError("Cannot Find globalUI");
		//TODO : TESTCODE
		GoToLobby();
	}

	private void LoadDataManager()
	{
		userDataTable.LoadUserData();
		optionDataTable.LoadOptionData();
		playerSkillDataManager.LoadPlayerSkillData();
		monsterDataManager.LoadMonsterData();
		stageDataManager.LoadStageData();
		badWordDataManager.ReadTextCSV();
		addressableManager.Init();
	}

	private void SettingLanguage()
	{
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[optionDataTable.langaugeIndex];
	}
	#endregion
	//=====================================================================================================================Init


	//Scene====================================================================================================================
	#region Scene
	public void GoToLobby()
	{
		SetSceneState(eSceneType.LobbyLoading);
		UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
	}
	public void SetSceneState(eSceneType loadScene)
	{
		_nowSceneState = loadScene;
	}
	public eSceneType GetNowSceneState()
	{
		return _nowSceneState;
	}
	#endregion
	//====================================================================================================================Scene


	//Getter===================================================================================================================
	#region Getter
	public eConnectServerType GetConnectServerType()
	{
		return _connectServerType;
	}
	#endregion
	//===================================================================================================================Getter


	//Event====================================================================================================================
	#region Event
	public void PushBackButtonEvent(Action backButtonEvent)
	{
		if(_backButtonEventStack.Contains(backButtonEvent) == false)
			_backButtonEventStack.Push(backButtonEvent);
	}

	public void ClearBackButtonEvent()
	{
		_backButtonEventStack.Clear();
	}

	public void ActiveBackButtonEvent()
	{
		//TODO : 백버튼 로직 고민해보기
		//1. Stack : 한번의 버튼으로 2개이상 이벤트 발생시키기 힘듬, 중간에 다른 이벤트를 넣기 힘듬(현재방식)
		//2. UI상태에 따라 매번 초기화 후 체인으로 필요한 이벤트 할당 : 번거로움 코드 지저분함 하지만 굉장히 유연함(야핏방식)
		//모든 닫기버튼에서 ActiveBackButtonEvent호출하도록 작업하면 버그로 UI로직까지 망가질 확률이 높지 않을까
		//1. ActiveBackButtonEvent호출 + 기본적으로 수행해야하는 기능은 버튼에 넣어 2번 실행한다..?
		//2. 실제 백버튼을 눌렀을때만 ActiveBackButtonEvent호출하고 이외에는 DeleteLastBackButtonEvent 호출한다..? 흠..
		if (_backButtonEventStack.Count > 0)
		{
			try
			{
				Action backButtonEvent = _backButtonEventStack.Pop();
				if (backButtonEvent != null)
				{
					AudioManager.Ins.PlayClick();
					backButtonEvent?.Invoke();
				}
			}
			catch(Exception e)
			{
				LogManager.LogError("BackButton Error");
				LogManager.LogError(e.ToString());
			}
		}
	}

	public void DeleteLastBackButtonEvent(Action backButtonEvent)
	{
		if (_backButtonEventStack.Count > 0 && _backButtonEventStack.Contains(backButtonEvent))
			_backButtonEventStack.Pop();
	}
	#endregion
	//====================================================================================================================Event
	public void Logout()
	{
		userDataTable.ResetData();
		playerPrefsManager.DeleteLoginedUserData();
		SendBirdManager.GetInstance().DisconnectSendBird();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			DOTween.CompleteAll();
		}
		else if (Input.GetKeyUp(KeyCode.Escape))
		{
			DOTween.CompleteAll();
			ActiveBackButtonEvent();
		}
	}

	private void OnDestroy()
	{
		_destroyFlag = true;
		if (PhotonNetwork.IsConnected)
		{
			LogManager.Log("OnDisconnect Network On App Destroy");
			PhotonNetwork.Disconnect();
		}
	}
}
