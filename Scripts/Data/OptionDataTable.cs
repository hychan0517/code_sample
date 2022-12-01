using UnityEngine;

public class OptionDataTable
{
	public int langaugeIndex;
	public bool isTweenAnimation;
	public float musicVolume;
	public float soundVolume;

	public void LoadOptionData()
	{
		ResetData();

		string jsonStr = App.Ins.playerPrefsManager.GetOptionData();
		if (string.IsNullOrWhiteSpace(jsonStr) == false)
		{
			LogManager.Log("Get OptionDataTable Success");
			OptionDataTable temp = JsonUtility.FromJson<OptionDataTable>(jsonStr);

			if (temp != null)
				SetLoadedOptionData(temp);
			else
				LogManager.LogError("Get OptionDataTable Parsing Failed");
		}
	}

	private void SetLoadedOptionData(OptionDataTable loadedData)
	{
		langaugeIndex = loadedData.langaugeIndex;
		isTweenAnimation = loadedData.isTweenAnimation;
		musicVolume = loadedData.musicVolume;
		soundVolume = loadedData.soundVolume;
	}

	public void ResetData()
	{
		langaugeIndex = 0;
		isTweenAnimation = true;
		musicVolume = 1;
		soundVolume = 1;
	}
}
