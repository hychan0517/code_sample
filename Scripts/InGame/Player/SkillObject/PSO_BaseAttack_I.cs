using GlobalDefine;
using System;
using UnityEngine;

public class PSO_BaseAttack_I : PlayerSkillObject, IEnableTimer, IActiveTimer
{
	//0 : 화살속도
	//1 : 데미지
	//2 : 디버프 지속시간


	private GameObject _player;
	private Monster _targetMonster;

	private Vector3 _forward;

	private long _enableTimeTicks;
	private long _disableTimeTicks;

	public override void OnEnable()
	{
		base.OnEnable();
		SetEnableTicks();
		App_InGame appIngame = App_InGame.Ins;
		if (appIngame)
			App_InGame.Ins.GetEventManager().AddDisableTimer(this);
	}
	public override void OnDisable()
	{
		base.OnEnable();
		SetDisableTicks();
		App_InGame appIngame = App_InGame.Ins;
		if (appIngame)
			App_InGame.Ins.GetEventManager().AddDestroyTimer(this);
	}


	public override void InitPlayerSkillObject(PlayerSkillDataTable skillDataTable)
	{
		if (skillDataTable != null)
		{
			base.InitPlayerSkillObject(skillDataTable);
			_player = App_InGame.Ins.GetPlayer().gameObject;
		}
		else
		{
			LogManager.LogError("InitPlayerSKillObject Err");
			Destroy(gameObject);
		}
	}

	public override void ActivePlayerSkillObject()
	{
		if (_skillDataTable.optionDataArr == null || _skillDataTable.optionDataArr.Length != 3)
		{
			LogManager.LogError("PSO_BaseAttack_I skillOption Err");
			Destroy(gameObject);
		}
		else
		{
			_targetMonster = App_InGame.Ins.GetMonsterManager().GetTargetMonster();
			if (_player)
			{
				gameObject.SetActive(true);
				transform.position = _player.transform.position;
				transform.position += Vector3.up;
				SettingTargetAlgles();
				if (_targetMonster == null)
				{
					_forward = _player.transform.forward.normalized;
					transform.LookAt(_player.transform.position + Vector3.up + _forward);
				}
			}
			else
			{
				LogManager.Log("PSO_BaseAttack_I Active Err");
				gameObject.SetActive(false);
			}
		}
	}

	private void Update()
	{
		transform.position += _forward * Time.deltaTime * _skillDataTable.optionDataArr[0];
	}

	/// <summary> 타켓 몬스터 방향 벡터 세팅, 회전 </summary>
	private void SettingTargetAlgles()
	{
		if (_targetMonster)
		{
			transform.LookAt(new Vector3(_targetMonster.transform.position.x, _targetMonster.transform.position.y, _targetMonster.transform.position.z));
			_forward = transform.forward.normalized;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(Define.MONSTER_LAYER))
		{
			Monster monster = other.GetComponent<Monster>();
			if (monster)
			{
				monster.Damage(_skillDataTable.optionDataArr[1]);
				monster.AddMoveSpeed(-10);
			}
			App_InGame.Ins.GetHitObjectManager().ActivePlayerHitObject(_skillDataTable, other.transform.position);
			LogManager.Log("Trigger Base Attack");
		}
		else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			App_InGame.Ins.GetHitObjectManager().ActivePlayerHitObject(_skillDataTable, other.transform.position);
			InActivePlayerSkill();
		}
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}

	public void SetEnableTicks()
	{
		_enableTimeTicks = DateTime.Now.Ticks;
	}

	public long GetEnabledTicks()
	{
		return _enableTimeTicks;
	}

	public void SetDisableTicks()
	{
		_disableTimeTicks = DateTime.Now.Ticks;
	}

	public long GetDisabledTicks()
	{
		return _disableTimeTicks;
	}
}
