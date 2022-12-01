using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillUI : MonoBehaviour
{
	private PlayerSkillUI_Slot[] _skillUISlotArr = new PlayerSkillUI_Slot[3];

	public void InitUIObject()
	{
		_skillUISlotArr = GetComponentsInChildren<PlayerSkillUI_Slot>(true);
		foreach(var i in _skillUISlotArr)
		{
			i.InitUIObject();
		}

		//TODO : TESTCODE
		_skillUISlotArr[0].SettingSkill(4);
	}

	public void AddSkill(int skillID)
	{

	}
}
