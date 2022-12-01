using UnityEngine;

//라이브용 로그를 쉽게 분리하고 필요에따라 실행했던 로그들을 수집하는 기능을 하는 클래스
public static class LogManager
{
	/// <summary> 디버그용 로드 Symbol </summary>
	private const string LOG_TYPE_DEV = "DEBUG_LOG";
	private const string DEBUG_LOG_BASE_STR = "<color=#FFFF00>{0}</color>";
	private const string START_LOG_BASE_STR = "<color=#00FF00>▼▼▼▼{0}▼▼▼▼</color>";
	private const string END_LOG_BASE_STR = "<color=#00FF00>▲▲▲▲{0}▲▲▲▲</color>";
	private const string NOTICE_LOG_BASE_STR = "<color=#00FFFF>===={0}====</color>";
	private const string NETWORK_LOG_BASE_STR = "<color=#000FF>{0}</color>";

	public static void Log(string message)
	{
		Debug.Log(message);
	}

	public static void StartLog(string message)
	{
		Debug.LogFormat(START_LOG_BASE_STR, message);
	}

	public static void EndLog(string message)
	{
		Debug.LogFormat(END_LOG_BASE_STR, message);
	}

	public static void NoticeLog(string message)
	{
		Debug.LogFormat(NOTICE_LOG_BASE_STR, message);
	}

	public static void NetworkLog(string message)
	{
		Debug.LogFormat(NETWORK_LOG_BASE_STR, message);
	}

	public static void LogError(string message)
	{
		Debug.LogError(message);
	}

	[System.Diagnostics.Conditional(LOG_TYPE_DEV)]
	public static void DebugLog(string message)
	{
		Debug.LogFormat(DEBUG_LOG_BASE_STR, message);
	}

	[System.Diagnostics.Conditional(LOG_TYPE_DEV)]
	public static void DebugErrorLog(string message)
	{
		Debug.LogError(message);
	}
}
