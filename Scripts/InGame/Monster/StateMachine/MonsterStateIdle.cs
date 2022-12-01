using UnityEngine;

public class MonsterStateIdle : MonsterState
{
	public MonsterStateIdle(Monster o) : base(o)
	{
	}
	public override void OnEnd()
	{
	}

	public override void OnStart()
	{
	}

	public override bool OnTransition()
	{
		_monster.ChangeMonsterState(GlobalDefine.eMonsterStateType.Move);
		return true;
	}

	public override void Tick()
	{
		OnTransition();
	}
}
