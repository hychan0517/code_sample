using GlobalDefine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviourPunCallbacks
{
	[SerializeField]
	protected int _skillID;

	protected PlayerSkillDataTable _skillDataTable;
	protected GameObject _skillSample;
	protected List<PlayerSkillObject> _skillList = new List<PlayerSkillObject>();
	public void InitData()
	{
		if (_skillID > 0)
		{
			_skillDataTable = App.Ins.playerSkillDataManager.GetPlayerSkillDataTableByID(_skillID);

			if (_skillDataTable == null)
				LogManager.LogError(string.Format("Cannot Find Skill. ID : {0}", _skillID));
			else
			{
				_skillSample = Resources.Load<GameObject>(string.Format(Define.SKILL_PREFAB_PATH, _skillID));
				if (_skillSample == null)
					LogManager.LogError(string.Format("Cannot Find Skill Prefab. ID : {0}", _skillID));
			}
		}
		else
		{
			LogManager.LogError("InitData Skill ID 0 Err");
		}
	}
	public void ActivePlayerSkill()
	{
		//비활성화 오브젝트 탐색
		PlayerSkillObject skillObject = GetInActiveSkill();
		if (skillObject)
		{
			skillObject.ActivePlayerSkillObject();
		}
	}
	/// <summary> 스킬사용이 끝나서 비활성화된 오브젝트 가져옴 </summary>
	protected PlayerSkillObject GetInActiveSkill()
	{
		for(int i = 0; i < _skillList.Count; ++i)
		{
			//Destroy확인
			if (_skillList[i])
			{
				if(_skillList[i].gameObject.activeSelf == false)
					return _skillList[i];
			}
			else
			{
				_skillList.RemoveAt(i);
				--i;
			}
		}

		return GetActiveSkillOnCreate();
	}

	/// <summary> 재사용할 오브젝트가 없으면 생성하고 초기화 </summary>
	protected PlayerSkillObject GetActiveSkillOnCreate()
	{
		GameObject initSkill = PhotonNetwork.Instantiate(string.Format(Define.SKILL_PREFAB_PATH, _skillID), Vector3.zero, Quaternion.identity);
		if (initSkill)
		{
			initSkill.transform.parent = transform;
			PlayerSkillObject skill = initSkill.GetComponent<PlayerSkillObject>();
			skill.InitPlayerSkillObject(_skillDataTable);
			_skillList.Add(skill);
			return skill;
		}
		else
		{
			//비활성화 오브젝트 없으며 생성에도 실패
			LogManager.LogError("GetActiveSkillOnCreate Err");
			return null;
		}
	}

	public int GetSkillID()
	{
		return _skillID;
	}
}
