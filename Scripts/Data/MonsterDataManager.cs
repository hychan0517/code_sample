using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDataManager
{
	private Dictionary<int, MonsterDataTable> _monsterDataDict = new Dictionary<int, MonsterDataTable>();
	public void LoadMonsterData()
	{
		if (_monsterDataDict.Count > 0)
		{
			LogManager.LogError("Already Loaded Monster Data Table");
			return;
		}

		TextAsset loadText = Resources.Load<TextAsset>("Data/monster_data_table");
		if (string.IsNullOrEmpty(loadText.text))
			LogManager.LogError("Cannot Find Monster Data Table");
		else
		{
			try
			{
				List<MonsterDataTable> jsonData = JsonConvert.DeserializeObject<List<MonsterDataTable>>(loadText.text);
				foreach (var i in jsonData)
				{
					_monsterDataDict.Add(i.monsterID, i);
				}
			}
			catch (Exception e)
			{
				LogManager.LogError(e.ToString());
			}
		}
	}

	public MonsterDataTable GetMonsterDataByID(int id)
	{
		if (_monsterDataDict.ContainsKey(id))
			return _monsterDataDict[id];
		else
			return null;
	}
}
