using GlobalDefine;
using Photon.Pun;
using System;
using UnityEngine;

public class PSO_BaseAttack_F : PlayerSkillObject, IEnableTimer, IActiveTimer
{
	//0 : 화살속도
	//1 : 데미지
	//2 : 넉백파워

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
			LogManager.LogError("PSO_BaseAttack_F skillOption Err");
			Destroy(gameObject);
		}
		else
		{
			PlaySound();

			_targetMonster = App_InGame.Ins.GetMonsterManager().GetTargetMonster();
			if (_player)
			{
				transform.position = _player.transform.position;
				transform.position += Vector3.up;
				if(_targetMonster == null)
				{
					transform.rotation = _player.transform.rotation;
					_forward = transform.forward.normalized;
				}
				else
				{
					SettingTargetAlgles();
				}
				gameObject.SetActive(true);
			}
			else
			{
				LogManager.Log("PSO_BaseAttack_F Active Err");
				gameObject.SetActive(false);
			}
		}
	}

	private void Update()
	{
		if (photonView.IsMine)
		{
			if (_targetMonster)
			{
				//몬스터 살아있으면 몬스터방향으로, 죽었다면 진행하단 방향으로 나아감
				if (_targetMonster.IsLive())
				{
					SettingTargetAlgles();
				}
			}
			transform.position += _forward * Time.deltaTime * _skillDataTable.optionDataArr[0];
		}
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
			if (PhotonNetwork.IsMasterClient)
			{
				Monster monster = other.GetComponent<Monster>();
				if (monster)
				{
					monster.Damage(_skillDataTable.optionDataArr[1]);
					monster.KnockBack(_forward, _skillDataTable.optionDataArr[2]);
				}
				App_InGame.Ins.GetHitObjectManager().ActivePlayerHitObject(_skillDataTable, other.transform.position);
				InActivePlayerSkill();
				LogManager.Log("Trigger Base Attack");
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		//else if(other.gameObject.layer == LayerMask.NameToLayer("Wall"))
		//{
		//	App_InGame.Ins.GetHitObjectManager().ActivePlayerHitObject(_skillDataTable, new Vector3(other.transform.position.x, _player.transform.position.y, other.transform.position.z));
		//}

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
