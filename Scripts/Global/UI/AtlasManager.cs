using UnityEngine;
using UnityEngine.U2D;
public class AtlasManager : MonoBehaviour
{
	#region SINGLETON
	static AtlasManager _instance = null;

	public static AtlasManager Ins
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(AtlasManager)) as AtlasManager;
				if (_instance == null)
				{
					_instance = new GameObject("AtlasManager", typeof(AtlasManager)).GetComponent<AtlasManager>();
				}
			}

			return _instance;
		}
	}
	#endregion
	private void Awake()
	{
		DontDestroyOnLoad(this);
		_atlas = Resources.Load("Atlas/", typeof(SpriteAtlas)) as SpriteAtlas;
	}
	public SpriteAtlas _atlas { get; private set; }
}