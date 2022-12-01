using UnityEngine;

public class PSO_Dash : PlayerSkillObject
{
	//0 : 지속시간
	//1 : 추가 이동속도


	private GameObject _player;
	private float _deltaTime;
	private float _durationSeconds;

	public override void ActivePlayerSkillObject()
	{
		if (_skillDataTable.optionDataArr == null || _skillDataTable.optionDataArr.Length != 2)
		{
			LogManager.LogError("Dash skillOption Err");
			Destroy(gameObject);
		}
		else
		{
			//TODO : 버프 계산
			gameObject.SetActive(true);
			_deltaTime = 0;
			_player = App_InGame.Ins.GetPlayer().gameObject;
			_durationSeconds = _skillDataTable.optionDataArr[0];
			App_InGame.Ins.GetPlayer().AddMoveSpeed(_skillDataTable.optionDataArr[1]);
		}
	}

	private void Update()
	{
		_deltaTime += Time.deltaTime;
		if (_player && _deltaTime <= _durationSeconds)
		{
			transform.position = _player.transform.position;
		}
		else
		{
			InActivePlayerSkill();
			App_InGame.Ins.GetPlayer().AddMoveSpeed(-_skillDataTable.optionDataArr[1]);
		}
	}
}
