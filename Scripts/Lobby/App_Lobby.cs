using DG.Tweening;
using GlobalDefine;
using UnityEngine;

public class App_Lobby : MonoBehaviour
{
	#region SINGLETON
	static App_Lobby _instance = null;

	public static App_Lobby Ins
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(App_Lobby)) as App_Lobby;
				if (_instance == null)
				{
					_instance = new GameObject("App_Lobby", typeof(App_Lobby)).GetComponent<App_Lobby>();
				}
			}
			return _instance;
		}
	}
	#endregion

	[SerializeField]
	public Canvas mainCanvas;
	[SerializeField]
	public LobbyUI lobbyUI;


	private void Awake()
	{
		App_Lobby i = Ins;
		App.Ins.SetSceneState(eSceneType.Lobby);
	}

	private void Start()
	{
		lobbyUI = FindObjectOfType<LobbyUI>(true);
		mainCanvas = lobbyUI.GetComponent<Canvas>();

		if (mainCanvas == null)
			LogManager.LogError("Cannot Find lobbyUI");

		if (lobbyUI)
		{
			lobbyUI.InitUIObject();
			lobbyUI.SettingLobbyUI();
		}
		else
		{
			LogManager.LogError("Cannot Find lobbyUI");
		}
	}


	private void OnDestroy()
	{
		DOTween.KillAll();
		if (App.Ins)
			App.Ins.ClearBackButtonEvent();
	}
}
