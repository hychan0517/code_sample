using UnityEngine;

public class MeleeMonsterStateMove : MonsterState
{
	public MeleeMonsterStateMove(Monster o) : base(o)
	{
	}

	public override void OnStart()
	{

	}

	public override bool OnTransition()
	{
		Player player = App_InGame.Ins.GetNeerPlayer(_monster.gameObject.transform.position);
		if (player)
		{
			if (_monster.IsAttakRange() && _monster.IsAttackDegree())
			{
				_monster.ChangeMonsterState(GlobalDefine.eMonsterStateType.Attack);
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return true;
		}
	}

	public override void Tick()
	{
		OnTransition();
	}
	public override void OnEnd()
	{

	}
}
