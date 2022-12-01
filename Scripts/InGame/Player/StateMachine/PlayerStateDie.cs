public class PlayerStateDie : PlayerState
{
	public PlayerStateDie(Player o) : base(o)
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
	public override void OnEnd()
	{
	}
}
