using UnityEngine;

/// <summary> 로비 -> 인게임시 활용할 데이터 모음 </summary>
public class DataBridgeManager : MonoBehaviour
{
	#region SINGLETON
	static DataBridgeManager _instance = null;

	public static DataBridgeManager Ins
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(DataBridgeManager)) as DataBridgeManager;
				if (_instance == null)
				{
					_instance = new GameObject("DataBridgeManager", typeof(DataBridgeManager)).GetComponent<DataBridgeManager>();
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		DontDestroyOnLoad(this);
	}
	#endregion

	private int _stageID;

	/// <summary> 인게임 -> 로비 이동할 때 호출하여 기존 데이터 삭제 </summary>
	public void InitData()
	{
	}

	public void SetStageID(int stageID)
	{
		_stageID = stageID;
	}

	public int GetNowStageID()
	{
		return _stageID;
	}
}
