using GlobalDefine;
using System.Collections.Generic;
using UnityEngine;

public class HitObjectManager : MonoBehaviour
{
	private Dictionary<int, List<PlayerHitObject>> _hitObjectListDict = new Dictionary<int, List<PlayerHitObject>>();

	public void ActivePlayerHitObject(PlayerSkillDataTable skillDataTable, Vector3 pos)
	{
		PlayerHitObject hitObject = GetInActiveSkill(skillDataTable);
		if (hitObject)
		{
			hitObject.ActiveHitObject(pos);
		}
	}

	/// <summary> 비활성화된 오브젝트 가져옴 </summary>
	protected PlayerHitObject GetInActiveSkill(PlayerSkillDataTable skillDataTable)
	{
		if (_hitObjectListDict.ContainsKey(skillDataTable.skillID))
		{
			for (int i = 0; i < _hitObjectListDict[skillDataTable.skillID].Count; ++i)
			{
				//Destroy확인
				if (_hitObjectListDict[skillDataTable.skillID][i])
				{
					if (_hitObjectListDict[skillDataTable.skillID][i].gameObject.activeSelf == false)
						return _hitObjectListDict[skillDataTable.skillID][i];
				}
				else
				{
					_hitObjectListDict[skillDataTable.skillID].RemoveAt(i);
					--i;
				}
			}

			return GetActiveHitObjectOnCreate(skillDataTable);
		}
		else
		{
			_hitObjectListDict.Add(skillDataTable.skillID, new List<PlayerHitObject>());
			return GetActiveHitObjectOnCreate(skillDataTable);
		}

	}

	/// <summary> 재사용할 오브젝트가 없으면 생성하고 초기화 </summary>
	protected PlayerHitObject GetActiveHitObjectOnCreate(PlayerSkillDataTable skillDataTable)
	{
		GameObject load = Resources.Load<GameObject>(string.Format(Define.HIT_PREFAB_PATH, skillDataTable.skillID));
		if (load)
		{
			GameObject initHit = Instantiate(load, transform);
			PlayerHitObject skill = initHit.GetComponent<PlayerHitObject>();
			skill.InitObject(skillDataTable);
			_hitObjectListDict[skillDataTable.skillID].Add(skill);
			return skill;
		}
		else
		{
			//비활성화 오브젝트 없으며 생성에도 실패
			LogManager.LogError("GetActiveHitObjectOnCreate Err");
			return null;
		}
	}
}
