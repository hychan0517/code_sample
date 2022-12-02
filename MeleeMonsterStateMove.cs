public class MeleeMonsterStateMove : MonsterState
{
	private Player _targetPlayer;

	public MeleeMonsterStateMove(Monster o) : base(o)
	{
	}

	public override void OnStart()
	{

	}

	public override bool OnTransition()
	{
		_targetPlayer = App_InGame.Ins.GetNeerPlayer(_monster.gameObject.transform.position);
		if (_targetPlayer)
		{
			return false;
		}
		else
		{
			_monster.ChangeStateIdle();
			return true;
		}
	}

	public override void Tick()
	{
		if(OnTransition() == false)
		{
			if (_monster.IsAttakRange() && _monster.IsAttackDegree())
			{
				_monster.ChangeMonsterState(GlobalDefine.eMonsterStateType.Attack);
			}
		}
	}

	public override void OnEnd()
	{

	}
}
