using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
	//PlayerSkillManager는 플레이어 스킬을 가지고 호출하여 스킬을 사용한다
	//PlayerSkill은 해당 스킬 사용에 필요한 오브젝트들에게 필요한 옵션값을 할당하고 오브젝트를 풀링한다
	private Dictionary<int, PlayerSkill> _playerSkillDict = new Dictionary<int, PlayerSkill>();

	/// <summary> 자식으로 보유한 모든 스킬을 초기화함 </summary>
	public void InitGameObject()
	{
		_playerSkillDict = new Dictionary<int, PlayerSkill>();

		//TODO : TESTCODE
		PlayerSkill[] skillArr = GetComponentsInChildren<PlayerSkill>(true);
		foreach(var skill in skillArr)
		{
			if (skill.GetSkillID() > 0)
			{
				skill.InitData();
				_playerSkillDict.Add(skill.GetSkillID(), skill);
			}
		}
		//
	}
	
	public void ActivePlayerSkill(int skillID)
	{
		if(_playerSkillDict.ContainsKey(skillID))
		{
			_playerSkillDict[skillID].ActivePlayerSkill();
		}
		else
		{
			LogManager.LogError(string.Format("Cannot Find Skill. ID : {0}", skillID));
		}
	}

	public void AddPlayerSkill(int skillID)
	{

	}
}
