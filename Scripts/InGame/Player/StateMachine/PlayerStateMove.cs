using UnityEngine;

public class PlayerStateMove : PlayerState
{
	private PlayerControllerUI _playerController;
	private float deltaTime;
	public PlayerStateMove(Player o) : base(o)
	{
		_playerController = App_InGame.Ins.GetInGameUI().GetPlayerControllerUI();
	}
	public override void OnStart()
	{
		deltaTime = Time.deltaTime;
	}

	public override bool OnTransition()
	{
		if (_playerController.IsPlayerMove() == false)
		{
			_player.ChangeStateIdle();
			return true;
		}
		else
			return false;
	}

	public override void Tick()
	{
		if(OnTransition() == false)
		{
			deltaTime = Time.deltaTime;
			Vector3 dir = _playerController.GetPlayerMoveDir();
			Vector3 dirTo3D = new Vector3(dir.x, 0, dir.y);
			float degree = Mathf.Atan2(dirTo3D.x, dirTo3D.z) * Mathf.Rad2Deg;
			_player.transform.localEulerAngles = new Vector3(0, degree, 0);
			_player.transform.position += dirTo3D * deltaTime * _player.GetPlayerDataTable().speed;
		}
	}

	public override void OnEnd()
	{
	}
}
