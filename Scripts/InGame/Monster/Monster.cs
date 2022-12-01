using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;
using GlobalDefine;
using Photon.Pun;
using System;

public class Monster : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private int _monsterID = 0;
	[SerializeField]
	private GameObject _attackTransform;
	[SerializeField]
	private Animator _animator;

	//Renderer
	private List<Material> _materialList = new List<Material>();
	private Coroutine _dissolveCor;


	//Move
	[SerializeField]
	private MonsterStateMachine _monsterStateMachine;
	[SerializeField]
	private AIPath _aiPath;
	[SerializeField]
	private AIDestinationSetter _aiDestinationSetter;
	private Coroutine _knockBackCor;

	[SerializeField]
	private Rigidbody _rig;
	[SerializeField]
	private GameObject _targetSprite;
	private MonsterDataTable _baseDataTable;
	private MonsterDataTable _calculatedDataTable;
	private bool _isLive;
	private bool _isInit;

	private const string BASE_ANIMATION = "Action";
	private long _monsterIndex;

	public void InitMonster()
	{
		if (_attackTransform == null)
			LogManager.LogError("Cannot find _attackTransform");
		if (_animator == null)
			LogManager.LogError("Cannot find _animator");
		if (_aiPath == null)
			LogManager.LogError("Cannot find _aiPath");
		if (_rig == null)
			LogManager.LogError("Cannot find _rig");
		if (_targetSprite)
			_targetSprite.SetActive(false);
		else
			LogManager.LogError("Cannot find _targetSprite");
		if (_monsterStateMachine)
			_monsterStateMachine.SettingStateMachine();
		else
			LogManager.LogError("Cannot find _monsterStateMachine");

		_aiDestinationSetter = GetComponent<AIDestinationSetter>();

		Renderer[] renders = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renders.Length; i++)
		{
			_materialList.AddRange(renders[i].materials);
		}

		if (_monsterID > 0)
		{
			_baseDataTable = App.Ins.monsterDataManager.GetMonsterDataByID(_monsterID);
			if (_baseDataTable == null)
			{
				LogManager.LogError(string.Format("Load Monster Data Err. ID : {0}", _monsterID));
				Destroy(gameObject);
			}
		}
		else
		{
			LogManager.LogError(string.Format("Load Monster Data Err. ID : {0}", _monsterID));
			Destroy(gameObject);
		}

		KillDissolveCor();
		gameObject.SetActive(false);
		photonView.RPC("SpawnMonsterRPC", RpcTarget.Others);
		InitEvent();
		_isInit = true;
	}

	public override void OnEnable()
	{
		if (PhotonNetwork.IsMasterClient == false && _isInit == false)
		{
			//마스터 클라이언트로에서 활성화시킨 몬스터
			Destroy(gameObject.GetComponent<MeleeMonsterStateMachine>());
			Destroy(gameObject.GetComponent<AIPath>());
			Destroy(gameObject.GetComponent<Seeker>());
			_baseDataTable = App.Ins.monsterDataManager.GetMonsterDataByID(_monsterID);
			_calculatedDataTable = _baseDataTable.Copy();
			Renderer[] renders = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renders.Length; i++)
			{
				_materialList.AddRange(renders[i].materials);
			}

			if(_targetSprite)
				_targetSprite.SetActive(false);
			else
				LogManager.LogError("Cannot find _targetSprite");

			InitEvent();
			_isInit = true;
		}

		if(_isInit)
		{
			_isLive = true;
			KillDissolveCor();
			SetDissolveValue(0);
			gameObject.layer = LayerMask.NameToLayer(Define.LIVE_MONSTER_LAYER);
		}
	}

	public void SettingMonster(long monsterIndex)
	{
		if(PhotonNetwork.IsMasterClient)
		{
			if (_monsterID > 0 && _baseDataTable != null)
			{
				gameObject.SetActive(true);
				_monsterIndex = monsterIndex;
				_calculatedDataTable = _baseDataTable.Copy();
				KillKnockBackCor();
				_isLive = true;
				SetCanMove(true);
				SetDissolveValue(0);
				ChangeStateIdle();
				AddMoveSpeed(0);
				gameObject.layer = LayerMask.NameToLayer(Define.LIVE_MONSTER_LAYER);
				photonView.RPC("ActiveMonsterRPC", RpcTarget.Others);
				App_InGame.Ins.GetMonsterHPManager().SettingMonsterHPGauge(this);
			}
			else
			{
				LogManager.LogError(string.Format("SettingMonster Data Error Index : {0}", monsterIndex));
			}
		}
	}

	private void InitEvent()
	{
		App_InGame.Ins.GetEventManager().AddGameOverEvent(GameOver);
	}

	private void FixedUpdate()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (_monsterStateMachine)
				_monsterStateMachine.FixedUpdateEvent();

			SetNeerPlayer();
		}
	}

	private void SetNeerPlayer()
	{
		if(_aiDestinationSetter)
			_aiDestinationSetter.target = App_InGame.Ins.GetNeerPlayer(transform.position).gameObject.transform;
	}

	public void SettingTarget(bool isActive)
	{
		if(_targetSprite)
			_targetSprite.gameObject.SetActive(isActive);
	}

	/// <summary> 버프를 통한 스피드 배율 증감 </summary>
	/// <param name="value"></param>
	public void AddMoveSpeed(float value)
	{
		_calculatedDataTable.speed += _baseDataTable.speed * value;
		_calculatedDataTable.speed = Mathf.Clamp(_calculatedDataTable.speed, 0, _baseDataTable.speed * 2);
		_animator.SetFloat("MoveSpeed", _calculatedDataTable.speed / _baseDataTable.speed);
		_aiPath.maxSpeed = _calculatedDataTable.speed;
	}

	public void ChangeMonsterState(eMonsterStateType monsterStateType)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (_monsterStateMachine)
				_monsterStateMachine.ChangeStateMachine(monsterStateType);
			ChangeAnimation(monsterStateType);
		}
	}

	/// <summary> 다른 애니메이션 후 초기화 이벤트 </summary>
	public void ChangeStateIdle()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			SetCanMove(true);
			if (_monsterStateMachine)
				_monsterStateMachine.ChangeStateIdle();
			ChangeAnimation(eMonsterStateType.Idle);
		}
	}

	public void ChangeAnimation(eMonsterStateType monsterStateType)
	{
		if (PhotonNetwork.IsMasterClient && _animator)
		{
			_animator.SetInteger(BASE_ANIMATION, (int)monsterStateType);
		}
	}

	public void SetCanMove(bool isAble)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (IsLive() == false)
				isAble = false;
			_aiPath.canMove = isAble;
		}
	}


	//Getter==================================================================================================
	#region Getter
	public bool IsLive()
	{
		return _isLive;
	}

	public float GetNowHP()
	{
		return _calculatedDataTable.hp;
	}

	public float GetNowHPRate()
	{
		return Mathf.Clamp(_calculatedDataTable.hp / _baseDataTable.hp, 0, 1);
	}

	public MonsterDataTable GetCalculatedDateTable()
	{
		return _calculatedDataTable;
	}
	#endregion
	//==================================================================================================Getter


	//Attack Process==========================================================================================
	#region Attack Process
	public void AttackProcess()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (_attackTransform)
			{
				Player player = App_InGame.Ins.GetNeerPlayer(transform.position);
				if (player && IsAttakRange())
				{
					player.Damage(GetCalculatedDateTable().attackDamage);
				}
			}
			else
			{
				LogManager.LogError(string.Format("AttackProcess Error Cannt Find AttackTranform Monster : {0}", gameObject.name));
			}
		}
	}

	public bool IsAttakRange()
	{
		Player player = App_InGame.Ins.GetNeerPlayer(transform.position);
		float range = GetCalculatedDateTable().attackRange;
		if (player)
		{
			if ((player.transform.position - transform.position).sqrMagnitude <= range * range)
				return true;
			else
				return false;
		}
		else
		{
			return false;
		}
	}

	public bool IsAttackDegree()
	{
		//TODO : 공격모션 이후 혹은 공격판정에 각도도 확인할건지..
		if (App_InGame.Ins.GetNeerPlayer(transform.position))
		{
			transform.LookAt(App_InGame.Ins.GetNeerPlayer(transform.position).gameObject.transform);
			return true;
		}
		else
		{
			return false;
		}
	}
	#endregion
	//==========================================================================================Attack Process


	//Damage Process==========================================================================================
	#region Damage Process
	public void Damage(float damage)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			//소수점 데미지는 버림
			_calculatedDataTable.hp -= (int)damage;
			if (_calculatedDataTable.hp <= 0)
				Die();

			//TODO : Hp Gauge까지 포함하여 이벤트로 만들지 생각해보기
			photonView.RPC("DamageRPC", RpcTarget.Others, damage);
			photonView.RPC("SetHP", RpcTarget.Others, _calculatedDataTable.hp);
			App_InGame.Ins.GetMonsterHPTextManager().SettingMonsterHPText(this, (int)damage);
		}
	}

	public void KnockBack(Vector3 dir, float power)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (_rig && IsLive())
			{
				_rig.velocity = Vector3.zero;
				_rig.AddForce(dir * power);
				KillKnockBackCor();
				_knockBackCor = StartCoroutine(Co_KnockBackTimer());
			}
		}
	}

	private IEnumerator Co_KnockBackTimer()
	{
		SetCanMove(false);
		yield return new WaitForSeconds(0.5f);
		SetCanMove(true);
		_knockBackCor = null;
	}

	private void KillKnockBackCor()
	{
		if (_knockBackCor != null)
			StopCoroutine(_knockBackCor);
		_knockBackCor = null;
	}

	private void Die()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			_animator.SetInteger("Action", 2);
			KillKnockBackCor();
			SetCanMove(false);
			App_InGame.Ins.SetSkillAttack();
			photonView.RPC("DieMonsterRPC", RpcTarget.Others);
		}

		_isLive = false;
		_dissolveCor = StartCoroutine(Co_Dissolve());
		gameObject.layer = LayerMask.NameToLayer(Define.DEAD_MONSTER_LAYER);
		App_InGame.Ins.GetMonsterManager().AddHuntCount();
	}

	private void KillDissolveCor()
	{
		if (_dissolveCor != null)
		{
			StopCoroutine(_dissolveCor);
			_dissolveCor = null;
		}
	}

	private IEnumerator Co_Dissolve()
	{
		float deltaTime = 0;
		while(deltaTime <= 2)
		{
			deltaTime += Time.deltaTime;
			SetDissolveValue(deltaTime / 2);
			yield return null;
		}

		KillDissolveCor();
		gameObject.SetActive(false);
	}

	private void SetDissolveValue(float value)
	{
		for (int i = 0; i < _materialList.Count; i++)
		{
			_materialList[i].SetFloat("_Dissolve", value);
		}
	}
	#endregion
	//==========================================================================================Damage Process


	//Pun Events==============================================================================================
	#region Pun Events
	[PunRPC]
	private void SpawnMonsterRPC()
	{
		App_InGame.Ins.GetMonsterManager().SpawnMonsterPRC(this);
	}

	[PunRPC]
	private void ActiveMonsterRPC()
	{
		gameObject.SetActive(true);
		App_InGame.Ins.GetMonsterHPManager().SettingMonsterHPGauge(this);
	}

	[PunRPC]
	private void DieMonsterRPC()
	{
		LogManager.NetworkLog(string.Format("Die Monster RPC. Index : {0}", _monsterIndex));
		_isLive = false;
		_dissolveCor = StartCoroutine(Co_Dissolve());
		gameObject.layer = LayerMask.NameToLayer(Define.DEAD_MONSTER_LAYER);
	}

	[PunRPC]
	public void DamageRPC(float damage)
	{
		if (_isInit)
		{
			App_InGame.Ins.GetMonsterHPTextManager().SettingMonsterHPText(this, (int)damage);
		}
	}

	[PunRPC]
	public void SetHP(float hp)
	{
		if (_calculatedDataTable != null)
			_calculatedDataTable.hp = hp;
	}
	#endregion
	//==============================================================================================Pun Events


	private void GameOver()
	{
		SetCanMove(false);
		ChangeStateIdle();
		_monsterStateMachine = null;
	}
}
