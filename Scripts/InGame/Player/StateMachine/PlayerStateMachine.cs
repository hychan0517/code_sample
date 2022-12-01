using System.Collections.Generic;
using UnityEngine;
using GlobalDefine;

public class PlayerStateMachine : MonoBehaviour
{
	private Dictionary<ePlayerStateType, PlayerState> _stateDict = new Dictionary<ePlayerStateType, PlayerState>();
	private PlayerState _nowState;
	private ePlayerStateType _nowStateType;

	[SerializeField]
	private bool _isAttack = false;
	[SerializeField]
	private float _elapsedTimeOfAttack = 0.0f;

	public void SettingStateMachine()
	{
		Player o = gameObject.GetComponent<Player>();
		_stateDict.Add(ePlayerStateType.Idle, new PlayerStateIdle(o));
		_stateDict.Add(ePlayerStateType.Move, new PlayerStateMove(o));
		_stateDict.Add(ePlayerStateType.BaseAttack, new PlayerStateBaseAttack(o));
		_stateDict.Add(ePlayerStateType.SkillAttack, new PlayerStateSkillAttack(o));
		_stateDict.Add(ePlayerStateType.UltimateAttack, new PlayerStateUltimateAttack(o));
		_stateDict.Add(ePlayerStateType.Die, new PlayerStateDie(o));
		_nowState = _stateDict[ePlayerStateType.Idle];
		_nowState.OnStart();
	}

	public void ChangeStateMachine(ePlayerStateType stateType)
	{
		_nowState.OnEnd();
		_nowStateType = stateType;
		_nowState = _stateDict[stateType];
		_nowState.OnStart();
	}

	public void ChangeStateMachineIdle()
	{
		ChangeStateMachine(ePlayerStateType.Idle);
		_elapsedTimeOfAttack = 0.0f;
		_isAttack = false;
	}

	/// <summary> 애니메이션상태, 공격 딜레이 체크 </summary>
	public bool IsPossibleAttack()
	{
		if (_nowStateType == ePlayerStateType.Idle && IsAttackDelay())
			return true;
		else
			return false;
	}

	/// <summary> 애니메이션상태, 공격 딜레이 체크 </summary>
	public bool IsPossibleSkillAttack()
	{
		if ((_nowStateType == ePlayerStateType.Idle || _nowStateType == ePlayerStateType.Move) && _isAttack == false)
			return true;
		else
			return false;
	}

	private bool IsAttackDelay()
	{
		if (_isAttack == true) 
			return false;
		else if (_elapsedTimeOfAttack >= Define.DEFAULT_ATTACK_SPEED / App_InGame.Ins.GetPlayer().GetPlayerDataTable().attackSpeed)
			return true;
		else
			return false;
	}

	public void FixedUpdateEvent()
	{
		_elapsedTimeOfAttack += Time.deltaTime;
		_nowState.Tick();
	}
}