using UnityEngine;

public class PlayerStateSkillAttack : PlayerState
{
	public PlayerStateSkillAttack(Player o) : base(o)
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
		Monster targetMonster = App_InGame.Ins.GetMonsterManager().GetTargetMonster();
		if (targetMonster != null)
		{
			Vector3 targetDir = targetMonster.transform.position - _player.transform.position;
			float degree = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
			_player.transform.eulerAngles = new Vector3(0, degree, 0);
		}
	}
	public override void OnEnd()
	{
	}
}
