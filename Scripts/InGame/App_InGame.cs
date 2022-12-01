using DG.Tweening;
using GlobalDefine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class App_InGame : MonoBehaviour
{
	#region SINGLETON
	static App_InGame _instance = null;
	private static bool _destroyFlag = false;
	public static App_InGame Ins
	{
		get
		{
			if (_destroyFlag)
				return null;

			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(App_InGame)) as App_InGame;
				if (_instance == null)
				{
					_instance = new GameObject("App_InGame", typeof(App_InGame)).GetComponent<App_InGame>();
				}
			}
			return _instance;
		}
	}
	#endregion

	//Camera
	private Camera _uiCamera;
	private PlayerCamera _playerCamera;

	//UI
	private Canvas _mainCanvas;
	private InGameUI _inGameUI;

	//Player
	private Player _player;
	private List<Player> _playerList = new List<Player>();
	private PlayerSkillManager _playerSkillManager;
	//HitManager
	private HitObjectManager _hitObjectManager;

	//Monster
	private MonsterManager _monsterManager;
	private MonsterHPManager _monsterHPManager;
	private MonsterHPTextManager _monsterHPTextManager;

	//EventManager
	private EventManager _eventManager;



	public bool _isStart;

	private void Awake()
	{
		_destroyFlag = false;
		App_InGame i = Ins;
		App.Ins.SetSceneState(eSceneType.InGame);
	}

	private void Start()
	{
		_playerCamera = Camera.main.GetComponent<PlayerCamera>();
		_uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
		_mainCanvas = FindObjectOfType<Canvas>(true);

		_eventManager = FindObjectOfType<EventManager>(true);
		_hitObjectManager = FindObjectOfType<HitObjectManager>(true);

		_inGameUI = FindObjectOfType<InGameUI>(true);
		_inGameUI.InitUIObject();

		_monsterHPManager = FindObjectOfType<MonsterHPManager>(true);
		_monsterHPManager.InitObject();

		_monsterHPTextManager = FindObjectOfType<MonsterHPTextManager>(true);
		_monsterHPTextManager.InitObject();

		_playerSkillManager = FindObjectOfType<PlayerSkillManager>(true);
		_playerSkillManager.InitGameObject();

		_monsterManager = FindObjectOfType<MonsterManager>(true);
		_monsterManager.InitMonsterManager();

		_eventManager.AddGameStartEvent(OnSpawnPlayerCharacter);

		App.Ins.PushBackButtonEvent(App.Ins.globalUI.OnClickGameExitButtonToBack);

		InstantiateMap();
	}

	private void InstantiateMap()
	{
		// track 생성
		var task = Addressables.InstantiateAsync(string.Format("{0}{1}", Define.MAP_RESOURCE_BASE, DataBridgeManager.Ins.GetNowStageID()));

		task.Completed += (handle) =>
		{
			StartCoroutine(OnStatic(handle.Result));
		};
	}

	private IEnumerator OnStatic(GameObject go)
	{
		foreach (var child in go.GetComponentsInChildren<Transform>())
			child.gameObject.isStatic = true;
		yield return new WaitForEndOfFrame();
		StaticBatchingUtility.Combine(go);

		GameStart();
	}

	public void GameStart()
	{
		LogManager.Log("Game Start");
		_isStart = true;
		_eventManager.ActiveGameStartEvent();
	}

	private void OnSpawnPlayerCharacter()
	{
		GameObject init = PhotonNetwork.Instantiate(ResourcesPath.PLAYER, Vector3.zero, Quaternion.identity);
		Player player = init.GetComponent<Player>();
		_player = player;
		_player.InitGameObject(App.Ins.userDataTable.nickName);
		GetInGameUI().GetMyInfoSlot().SetPlayer(_player);
		_playerCamera.InitPlayer(_player);
		_playerList.Add(_player);
	}

	public void OnJoinPlayer(Player player)
	{
		_playerList.Add(player);
	}

	//UI
	public Camera GetUICamera()
	{
		return _uiCamera;
	}
	public InGameUI GetInGameUI()
	{
		return _inGameUI;
	}

	//Player
	public Player GetPlayer()
	{
		return _player;
	}

	public Player GetNeerPlayer(Vector3 position)
	{
		float minDistance = float.MaxValue;
		float distance = 0;
		int index = 0;
		for (int i = 0; i < _playerList.Count; ++i)
		{
			distance = (_playerList[i].gameObject.transform.position - position).sqrMagnitude;
			if(distance < minDistance)
			{
				index = i;
				minDistance = distance;
			}
		}

		return _playerList[index];
	}

	/// <summary> 몬스터 처치시 랜덤확률로 활 공격 스킬 생성 </summary>
	public void SetSkillAttack()
	{
		//TODO : 랜덤추가, 하나로 데이터관리하고 양쪽에서 확인하는게 더 좋을듯
		eBaseAttackSkillType type = (eBaseAttackSkillType)Random.Range(2, (int)eBaseAttackSkillType.Max);
		type = (eBaseAttackSkillType)Random.Range(2, (int)eBaseAttackSkillType.Light);
		type = eBaseAttackSkillType.Fire;
		LogManager.Log(string.Format("Set Player Skill Attack : {0}", type));
		_player.SetBaseSkillAttack(type);
		_inGameUI.GetPlayerControllerUI().SetSkillAttack(type);
	}

	/// <summary> 플레이어 활 공격 스킬 사용시 연출 </summary>
	public void ActivePlayerSkillAttackAnimation()
	{
		//줌인아웃
		_playerCamera.OnStartZoomInOut();
		//BG 연출
		//_inGameUI.OnStartBaseSkillAnimation();
	}

	public void ActivePlayerSkill(int skillID)
	{
		_playerSkillManager.ActivePlayerSkill(skillID);
	}


	//Monster
	public MonsterManager GetMonsterManager()
	{
		return _monsterManager;
	}

	public MonsterHPManager GetMonsterHPManager()
	{
		return _monsterHPManager;
	}

	public MonsterHPTextManager GetMonsterHPTextManager()
	{
		return _monsterHPTextManager;
	}


	//EventManager
	public EventManager GetEventManager()
	{
		return _eventManager;
	}


	//HitObjectManager
	public HitObjectManager GetHitObjectManager()
	{
		return _hitObjectManager;
	}

	public void GameOver()
	{
		LogManager.Log("Game Over");
		_isStart = false;
		GetEventManager().ActiveGameOverEvent();
		GetInGameUI()._gameResultUI.SettingResultUIFail();
		GetPlayer().GameOver();
	}

	public void GameClear()
	{
		LogManager.Log("Game Clear");
		_isStart = false;
		GetEventManager().ActiveGameClearEvent();
		GetInGameUI()._gameResultUI.SettingResultUIClear();
		GetPlayer().GameClear();
	}

	private void OnDestroy()
	{
		_destroyFlag = true;
		DOTween.KillAll();
		if (App.Ins)
			App.Ins.ClearBackButtonEvent();
	}
}
