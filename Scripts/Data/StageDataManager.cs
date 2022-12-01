using System;
using System.Collections.Generic;

public class StageDataManager
{
	public Dictionary<int, List<StageDataTable>> _stageDataListDict = new Dictionary<int, List<StageDataTable>>();

	public void LoadStageData()
	{
		List<Dictionary<string, string>> loadText = UnityUtility.CSVRead("Data/StageData");
		try
		{
			if (loadText.Count > 0)
			{
				foreach (Dictionary<string, string> i in loadText)
				{
					int stageID = int.Parse(i["stageID"]);

					if (_stageDataListDict.ContainsKey(stageID) == false)
						_stageDataListDict.Add(stageID, new List<StageDataTable>());

					StageDataTable temp = new StageDataTable();
					temp.monsterID = int.Parse(i["monsterID"]);
					temp.spawnSeconds = float.Parse(i["spawnSeconds"]);
					temp.xPosition = float.Parse(i["xPosition"]);
					temp.yPosition = float.Parse(i["yPosition"]);
					temp.zPosition = float.Parse(i["zPosition"]);
					temp.xRotation = float.Parse(i["xRotation"]);
					temp.yRotation = float.Parse(i["yRotation"]);
					temp.zRotation = float.Parse(i["zRotation"]);

					_stageDataListDict[stageID].Add(temp);
				}
			}
		}
		catch(Exception e)
		{
			LogManager.LogError("LoadStageData Error");
			LogManager.LogError(e.ToString());
		}
	}

	public List<StageDataTable> GetStageDataListByStageID(int stageID)
	{
		if (_stageDataListDict.ContainsKey(stageID))
		{
			return _stageDataListDict[stageID];
		}
		else
		{
			LogManager.LogError(string.Format("GetStageDataListByStageID Not Found Key : {0}", stageID));
			return new List<StageDataTable>();
		}
	}
}
