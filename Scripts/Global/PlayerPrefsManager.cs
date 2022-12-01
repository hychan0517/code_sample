using UnityEngine;

public class PlayerPrefsManager
{
    //기기에 종속된 정보
    private readonly string PREFS_OPTION = "Language";
    private readonly string PREFS_LOGINED_USER_DATA = "LoginedUserData";
    private readonly string PREFS_LAST_USER_INDEX = "LastUserIndex";

    //계정당 변경되는 정보
    private readonly string PREFS_USER_DATA = "UserData_{0}";

    public string GetOptionData()
	{
        if (PlayerPrefs.HasKey(PREFS_OPTION))
            return PlayerPrefs.GetString(PREFS_OPTION);
        else
            return string.Empty;
    }

    public void SetOptionData()
	{
        OptionDataTable optionDataTable = App.Ins.optionDataTable;
        string jsonStr = JsonUtility.ToJson(optionDataTable);
        PlayerPrefs.SetString(PREFS_OPTION, jsonStr);
        PlayerPrefs.Save();
    }

    public string GetLoginedUserData()
	{
        if (PlayerPrefs.HasKey(PREFS_LOGINED_USER_DATA))
            return PlayerPrefs.GetString(PREFS_LOGINED_USER_DATA);
        else
            return string.Empty;
    }

    public void SetLoginedUserData()
    {
        UserDataTable loginedUserData = App.Ins.userDataTable;
        string jsonStr = JsonUtility.ToJson(loginedUserData);
        PlayerPrefs.SetString(PREFS_LOGINED_USER_DATA, jsonStr);
        PlayerPrefs.Save();
    }

    public void DeleteLoginedUserData()
	{
        PlayerPrefs.DeleteKey(PREFS_LOGINED_USER_DATA);
        PlayerPrefs.Save();
    }

    public string GetUserData(string userID)
	{
        string key = string.Format(PREFS_USER_DATA, userID);
        LogManager.Log(string.Format("Get User Data ID : {0}", key));
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetString(key);
        else
            return string.Empty;
    }

    public void SetNowUserData()
	{
        if (string.IsNullOrWhiteSpace(App.Ins.userDataTable.accessToken) == false)
        {
            UserDataTable loginedUserData = App.Ins.userDataTable;
            string key = string.Format(PREFS_USER_DATA, loginedUserData.userID);
            string jsonStr = JsonUtility.ToJson(loginedUserData);
            LogManager.Log(string.Format("Set User Data ID : {0}, jsonData : {1}", key, jsonStr));
            PlayerPrefs.SetString(key, jsonStr);
            PlayerPrefs.Save();
        }
        else
		{
            LogManager.Log("Passed SetNowUserData Guest User");

        }
    }

    public int GetUserIndex()
    {
        int index = 0;
        if (PlayerPrefs.HasKey(PREFS_LAST_USER_INDEX))
            index = PlayerPrefs.GetInt(PREFS_LAST_USER_INDEX);

        SetLastUserIndex(++index);
        return index;
    }

    private void SetLastUserIndex(int index)
	{
        PlayerPrefs.SetInt(PREFS_LAST_USER_INDEX, index);
        PlayerPrefs.Save();
    }


}
