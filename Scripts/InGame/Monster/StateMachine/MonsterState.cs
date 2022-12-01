public abstract class MonsterState
{
	public Monster _monster;
	public MonsterState(Monster o)
	{
		_monster = o;
	}
	public abstract void OnStart();
	public abstract void Tick();
	public abstract void OnEnd();
	public abstract bool OnTransition();
}