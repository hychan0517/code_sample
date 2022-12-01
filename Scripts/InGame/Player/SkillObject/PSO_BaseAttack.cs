using UnityEngine;
using GlobalDefine;
using System;
using Photon.Pun;

public class PSO_BaseAttack : PlayerSkillObject, IEnableTimer, IActiveTimer
{
	//0 : 화살속도
	//1 : 데미지

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

	[PunRPC]
	private void InitDataTable(int skillID)
	{
		_skillDataTable = App.Ins.playerSkillDataManager.GetPlayerSkillDataTableByID(skillID);
	}

	public override void OnDisable()
	{
		base.OnDisable();
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
			if (photonView.IsMine)
			{
				photonView.RPC("InitDataTable", RpcTarget.Others, _skillDataTable.skillID);
			}
		}
		else
		{
			LogManager.LogError("InitPlayerSKillObject Err");
			Destroy(gameObject);
		}
	}


	//0 : 투사체 속도
	//1 : 데미지
	public override void ActivePlayerSkillObject()
	{
		if (photonView.IsMine)
		{
			if (_skillDataTable.optionDataArr == null || _skillDataTable.optionDataArr.Length != 2)
			{
				LogManager.LogError("PSO_BaseAttack skillOption Err");
				Destroy(gameObject);
			}
			else
			{
				PlaySound();

				_targetMonster = App_InGame.Ins.GetMonsterManager().GetTargetMonster();

				if (_player && _targetMonster)
				{
					gameObject.SetActive(true);
					transform.position = _player.transform.position;
					transform.position += Vector3.up;
					SettingTargetAlgles();
					photonView.RPC("ReActive", RpcTarget.Others);
				}
				else
				{
					LogManager.DebugLog("PSO_BaseAttack Active Err");
					gameObject.SetActive(false);
				}
			}
		}
	}

	[PunRPC]
	public void ReActive()
	{
		gameObject.SetActive(true);
		PlaySound();
	}

	private void Update()
	{
		if (photonView.IsMine)
		{
			if (_targetMonster && _targetMonster.IsLive())
			{
				//몬스터 살아있으면 몬스터방향으로, 죽었다면 진행하던 방향으로 나아감
				SettingTargetAlgles();
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
			//TODO : 정규화필요한지 확인
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
					//TODO : 마스터가 생성하지않은 오브젝트에대한 옵션셋팅
					if (_skillDataTable != null)
						monster.Damage(_skillDataTable.optionDataArr[1]);
					else
						Debug.LogError("Null");
					monster.KnockBack(transform.forward.normalized, 300);
				}

				InActivePlayerSkill();
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		
		//photonView.RPC("InActivePlayerSkill", RpcTarget.Others);
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
