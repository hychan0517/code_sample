using UnityEngine;

public class UserDataTable
{
	public string nickName;
	public string accessToken;
	public string userID;
	public bool _isLoginComplete;
	//TODO : º¯°æ
	public long _bestTime_1;
	public long _bestTime_2;

	public void LoadUserData()
	{
		ResetData();

		string jsonStr = App.Ins.playerPrefsManager.GetLoginedUserData();
		if (string.IsNullOrWhiteSpace(jsonStr) == false)
		{
			LogManager.Log("Get Logined UserData Success");
			UserDataTable temp = JsonUtility.FromJson<UserDataTable>(jsonStr);

			if (temp != null)
				SetLoadedUserData(temp);
			else
				LogManager.LogError("Get Logined UserData Parsing Failed");
		}
	}

	public void SetLoadedUserData(UserDataTable loadedData)
	{
		nickName = loadedData.nickName;
		accessToken = loadedData.accessToken;
		userID = loadedData.userID;
		_isLoginComplete = false;
		_bestTime_1 = loadedData._bestTime_1;
		_bestTime_2 = loadedData._bestTime_2;
	}

	public void ResetData()
	{
		nickName = string.Empty;
		accessToken = string.Empty;
		userID = string.Empty;
		_isLoginComplete = false;
		_bestTime_1 = 0;
		_bestTime_2 = 0;
	}
}
