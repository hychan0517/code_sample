using GlobalDefine;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterStateMachine : MonoBehaviour
{
	protected MonsterState _nowState;
	protected Dictionary<eMonsterStateType, MonsterState> _stateDict = new Dictionary<eMonsterStateType, MonsterState>();
	protected eMonsterStateType _nowStateType;

	protected bool _isAttack = false;
	protected float _elapsedTimeOfAttack = 0.0f;

	public abstract void SettingStateMachine();
	public abstract void AttackProcess();
	protected abstract void UpdateState();

	public void ChangeStateMachine(eMonsterStateType stateType)
	{
		if (_nowState._monster.gameObject.activeSelf == false || _stateDict.ContainsKey(stateType) == false)
		{
			LogManager.LogError("Monster ChangeStateMachine Err");
			return;
		}
		_nowState.OnEnd();
		_nowState = _stateDict[stateType];
		_nowState.OnStart();
		_nowStateType = stateType;
	}

	public void ChangeStateIdle()
	{
		ChangeStateMachine(eMonsterStateType.Idle);
		_elapsedTimeOfAttack = 0.0f;
		_isAttack = false;
	}

	public void FixedUpdateEvent()
	{
		_nowState.Tick();
		if (_nowState.GetType() == typeof(MonsterStateIdle))
		{
			UpdateState();
		}
	}
}
