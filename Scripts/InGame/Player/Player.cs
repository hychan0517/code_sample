using GlobalDefine;
using Photon.Pun;
using System;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
	#region Photon Animator View Note
	//Photon Animator View 설명
	//각 레이어 또는 파마미터별로 옵션설정 가능함
	//Discrete synchronization는 초당10번 값이 전송된다는 의미
	//Continuous는 매 프레임 전송
	#endregion
	private Animator _animator;
	/// <summary> 플레이어 기본 데이터 </summary>
	private PlayerDataTable _playerDataTable;
	/// <summary> 플레이어 계산된 데이터 </summary>
	private PlayerDataTable _calculatedPlayerData;
	private PlayerStateMachine _playerStateMachine;

	//Skill
	private ParticleSystem _baseAttackSkillFireEffect;
	private eBaseAttackSkillType _eBaseAttackSkillType;
	/// <summary> 기본공격도중 기본스킬사용하여 기본공격 이후 기본스킬사용해야하는지 확인용 <summary> 
	private bool _skillFlag;

	public Action _damageEvent;
	private const string BASE_ANIMATION = "Action";

	public void InitGameObject(string name)
	{
		gameObject.name = name;
		InitPlayerData();
		_animator = GetComponent<Animator>();
		_playerStateMachine = GetComponent<PlayerStateMachine>();
		_playerStateMachine.SettingStateMachine();
		_baseAttackSkillFireEffect = gameObject.GetChildObjectOptimize("SlashRed").GetComponent<ParticleSystem>();
		AddMoveSpeed(0);
	}

	private void Start()
	{
		if (photonView.IsMine == false)
		{
			App_InGame.Ins.OnJoinPlayer(this);
			InitPlayerData();
			//App_InGame.Ins.GetInGameUI().GetPlayerInfoUI().SetPlayer(this);
		}
	}

	private void InitPlayerData()
	{
		_playerDataTable = new PlayerDataTable();
		_playerDataTable.maxHP = 100;
		_playerDataTable.nowHP = 100;
		_playerDataTable.speed = 7.5f;
		_playerDataTable.attackSpeed = 10f;
		_playerDataTable.attackRange = 10;
		_calculatedPlayerData = _playerDataTable.Copy();
	}

	private void Update()
	{
		if (photonView.IsMine)
		{
			if (_playerStateMachine.IsPossibleAttack())
			{
				if (_skillFlag)
				{
					BaseAttackSkillToTarget();
					_skillFlag = false;
				}
				else
				{
					AttackToTarget();
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (photonView.IsMine)
		{
			_playerStateMachine.FixedUpdateEvent();
		}
	}

	public PlayerDataTable GetPlayerDataTable()
	{
		return _calculatedPlayerData;
	}

	/// <summary> 버프를 통한 스피드 배율 증감 </summary>
	public void AddMoveSpeed(float value)
	{
		if (photonView.IsMine)
		{
			_calculatedPlayerData.speed += _playerDataTable.speed * value;
			_calculatedPlayerData.speed = Mathf.Clamp(_calculatedPlayerData.speed, 0, _playerDataTable.speed * 2);
			_animator.SetFloat("MoveSpeed", _calculatedPlayerData.speed / _playerDataTable.speed);
		}
	}

	public void ChangePlayerState(ePlayerStateType playerStateType)
	{
		if (photonView.IsMine)
		{
			_playerStateMachine.ChangeStateMachine(playerStateType);
			ChangeAnimation(playerStateType);
		}
	}

	/// <summary> Animation콜백함수, Move입력 없거나 다른 애니메이션 후 초기화 이벤트 </summary>
	public void ChangeStateIdle()
	{
		if (photonView.IsMine)
		{
			_playerStateMachine.ChangeStateMachineIdle();
			ChangeAnimation(ePlayerStateType.Idle);
		}
	}

	public void ChangeAnimation(ePlayerStateType playerStateType)
	{
		if (photonView.IsMine)
			_animator.SetInteger(BASE_ANIMATION, (int)playerStateType);
	}


	//BaseAttack=========================================================================
	#region BaseAttack
	public bool IsPossibleAttack()
	{
		return _playerStateMachine.IsPossibleAttack();
	}
	
	public void SetBaseSkillAttack(eBaseAttackSkillType type)
	{
		_eBaseAttackSkillType = type;
	}

	private void AttackToTarget()
	{
		//기본공격은 매번 타겟몬스터 있을때만 공격
		if (App_InGame.Ins.GetMonsterManager().GetTargetMonster() != null)
		{
			ChangePlayerState(ePlayerStateType.BaseAttack);
		}
	}
	public void BaseAttackSkillToTarget()
	{
		if (_playerStateMachine.IsPossibleSkillAttack())
		{
			App_InGame.Ins.ActivePlayerSkillAttackAnimation();

			//활 스킬 공격도 스킬과 로직은 같음
			switch (_eBaseAttackSkillType)
			{
				case eBaseAttackSkillType.Fire:
					_baseAttackSkillFireEffect.Play();
					break;
				case eBaseAttackSkillType.Ice:
					break;
				case eBaseAttackSkillType.Light:
					break;
			}

			ChangePlayerState(ePlayerStateType.SkillAttack);
			_skillFlag = false;
		}
		else
		{
			_skillFlag = true;
		}
	}

	/// <summary> Animation콜백함수, 한번 사용한 기본공격 스킬은 초기화 </summary>
	public void ActiveBaseAttackSkill()
	{
		if (photonView.IsMine)
		{
			//TODO : 버튼 다운부터 이 함수 호출전에 다른타입의 기본공격스킬 습득시 다른 스킬이나가게됨
			if (_eBaseAttackSkillType > 0)
			{
				ActiveSkill(((int)_eBaseAttackSkillType));
				_eBaseAttackSkillType = eBaseAttackSkillType.None;
				App_InGame.Ins.SetSkillAttack();
			}
			else
			{
				LogManager.LogError("ActiveBaseAttakSkill Type None Err");
			}
		}
	}
	#endregion
	//=========================================================================BaseAttack


	//Damage Process=====================================================================
	#region Damage Process
	public void Damage(float damage)
	{
		//소수점 데미지는 버림
		_calculatedPlayerData.nowHP -= (int)damage;
		if (_calculatedPlayerData.nowHP <= 0)
			Die();
		else
			_damageEvent?.Invoke();

		photonView.RPC("DamagerPRC", RpcTarget.Others, damage);
	}

	[PunRPC]
	private void DamagerPRC(float damage)
	{
		//소수점 데미지는 버림
		_calculatedPlayerData.nowHP -= (int)damage;
		if(photonView.IsMine)
		{
			if (_calculatedPlayerData.nowHP <= 0)
				Die();
			else
				_damageEvent?.Invoke();
		}
	}

	private void Die()
	{
	}

	public float GetPlayerHPRate()
	{
		return Mathf.Clamp(_calculatedPlayerData.nowHP / _calculatedPlayerData.maxHP, 0, 1);
	}
	#endregion
	//=====================================================================Damage Process

	private void ActiveSkill(int skillID)
	{
		if (photonView.IsMine)
		{
			if (skillID > 1)
				LogManager.Log(string.Format("Active Skill : {0}", skillID));
			App_InGame.Ins.ActivePlayerSkill(skillID);
		}
	}

	//Network=====================================================================
	#region Network
	public void GameOver()
	{
		photonView.RPC("GameOverRPC", RpcTarget.Others);
	}

	[PunRPC]
	public void GameOverRPC()
	{
		App_InGame.Ins.GameOver();
	}

	public void GameClear()
	{
		photonView.RPC("GameClearRPC", RpcTarget.Others);
	}

	[PunRPC]
	public void GameClearRPC()
	{
		App_InGame.Ins.GameClear();
	}
	#endregion
	//=====================================================================Network
}
