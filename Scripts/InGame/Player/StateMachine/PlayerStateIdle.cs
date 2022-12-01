public class PlayerStateIdle : PlayerState
{
	public PlayerStateIdle(Player o) : base(o)
	{

	}
	public override void OnStart()
	{

	}

	public override bool OnTransition()
	{
		return false;
	}

	public override void Tick()
	{
		if (App_InGame.Ins.GetInGameUI().GetPlayerControllerUI().IsPlayerMove())
			_player.ChangePlayerState(GlobalDefine.ePlayerStateType.Move);
	}
	public override void OnEnd()
	{
	}
}
