using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class PlayerSkillDataManager
{
    private Dictionary<int, PlayerSkillDataTable> _playerSkillDict = new Dictionary<int, PlayerSkillDataTable>();
    public void LoadPlayerSkillData()
	{
		if (_playerSkillDict.Count > 0)
		{
			LogManager.LogError("Already Loaded Skill Data Table");
			return;
		}

		TextAsset loadText = Resources.Load<TextAsset>("Data/player_skill_data_table");
		if (string.IsNullOrEmpty(loadText.text))
			LogManager.LogError("Cannot Find Skill Data Table");
		else
		{
			try
			{
				List<PlayerSkillDataTable> jsonData = JsonConvert.DeserializeObject<List<PlayerSkillDataTable>>(loadText.text);
				foreach(var i in jsonData)
				{
					_playerSkillDict.Add(i.skillID, i);
				}
			}
			catch(Exception e)
			{
				LogManager.LogError(e.ToString());
			}
		}
	}

	public PlayerSkillDataTable GetPlayerSkillDataTableByID(int id)
	{
		if (_playerSkillDict.ContainsKey(id))
			return _playerSkillDict[id];
		else
			return null;
	}
}
