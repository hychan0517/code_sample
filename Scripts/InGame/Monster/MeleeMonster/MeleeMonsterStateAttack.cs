using UnityEngine;

public class MeleeMonsterStateAttack : MonsterState
{
	private Vector3 _attackPos;
	public MeleeMonsterStateAttack(Monster o) : base(o)
	{
	}

	public override void OnStart()
	{
		_monster.SetCanMove(false);
		_attackPos = _monster.transform.position;
	}

	public override bool OnTransition()
	{
		return true;
	}

	public override void Tick()
	{
		//_monster.transform.position = _attackPos;
	}

	public override void OnEnd()
	{

	}
}
