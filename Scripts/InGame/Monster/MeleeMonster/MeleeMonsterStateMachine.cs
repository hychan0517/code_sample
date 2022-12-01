using GlobalDefine;
using UnityEngine;

public class MeleeMonsterStateMachine : MonsterStateMachine
{
	protected Monster _monster;
	public override void SettingStateMachine()
	{
		Monster monster = GetComponent<Monster>();
		_monster = monster;
		if (_monster)
		{
			_stateDict.Add(eMonsterStateType.Idle, new MonsterStateIdle(monster));
			_stateDict.Add(eMonsterStateType.Move, new MeleeMonsterStateMove(monster));
			_stateDict.Add(eMonsterStateType.Attack, new MeleeMonsterStateAttack(monster));
			_stateDict.Add(eMonsterStateType.Die, new MonsterStateDie(monster));
			_nowState = _stateDict[eMonsterStateType.Idle];
			_nowState.OnStart();
		}
		else
		{
			LogManager.LogError(string.Format("SettingStateMachine Err : {0}", gameObject.name));
			Destroy(gameObject);
		}
	}

	public override void AttackProcess()
	{
		
	}

	protected override void UpdateState()
	{
		
	}
}