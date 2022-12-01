using UnityEngine;

/// <summary> �κ� -> �ΰ��ӽ� Ȱ���� ������ ���� </summary>
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

	/// <summary> �ΰ��� -> �κ� �̵��� �� ȣ���Ͽ� ���� ������ ���� </summary>
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
