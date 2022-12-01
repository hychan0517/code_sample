public class MonsterStateDie : MonsterState
{
	public MonsterStateDie(Monster o) : base(o)
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
		return true;
	}

	public override void Tick()
	{
	}
}
