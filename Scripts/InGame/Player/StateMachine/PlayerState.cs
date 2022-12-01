public abstract class PlayerState
{
	protected Player _player;
	public PlayerState(Player o)
	{
		_player = o;
	}
	public abstract void OnStart();
	public abstract void Tick();
	public abstract void OnEnd();
	public abstract bool OnTransition();
}