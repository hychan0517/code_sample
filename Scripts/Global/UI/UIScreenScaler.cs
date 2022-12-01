using UnityEngine;
using UnityEngine.UI;

public class UIScreenScaler : MonoBehaviour
{
	public enum eUIType
	{
		None,
		Base,				//가로세로 비율을 스크린과 맞춤
		GridLayout,         //그리드레이아웃그룹 자식 오브젝트 가로세로 크기(cellSize,spacing)를 스크린과 맞춤
		Layout,				//레이아웃그룹 자식 오브젝트 가로세로 비율을 스크린과 맞춤
		AutoRatio,			//가로세로비율 비교해서 더 작은 비율을 정비율로 맞춤
		AutoScale,          //정비율 스케일 변경
		HorOnly,
	}
	[SerializeField] private eUIType _uiType = eUIType.None;

	private bool _scaleFlag = false;

	private void Start()
	{
		if(_scaleFlag == false)
			DoScale();
	}

	public void InitScaler(eUIType type)
	{
		_uiType = type;
		if(_scaleFlag == false)
			DoScale();
	}

	private void DoScale()
	{
		if (_uiType == eUIType.None)
		{
			LogManager.LogError("UIType is None");
			return;
		}

		Vector2 ratio = UnityUtility.GetRatioOfScreenResolution();
		RectTransform rect = gameObject.GetComponent<RectTransform>();
		_scaleFlag = true;
		if (rect)
		{
			switch (_uiType)
			{
				case eUIType.Base:
					rect.sizeDelta = new Vector2(rect.rect.width * ratio.x, rect.rect.height * ratio.y);
					break;
				case eUIType.GridLayout:
					var grid = GetComponent<GridLayoutGroup>();
					if (grid == null)
						return;
					grid.cellSize *= ratio;
					grid.spacing *= ratio;
					break;
				case eUIType.Layout:
					GameObject[] childs = gameObject.GetChildsObject();
					foreach (GameObject child in childs)
					{
						RectTransform childRect = child.GetComponent<RectTransform>();
						childRect.sizeDelta = new Vector2(childRect.rect.width * ratio.x, childRect.rect.height * ratio.y);
					}
					break;
				case eUIType.AutoRatio:
					float minRatio = Mathf.Min(ratio.x, ratio.y);
					rect.sizeDelta = new Vector2(rect.rect.width * minRatio, rect.rect.height * minRatio);
					break;
				case eUIType.AutoScale:
					LogManager.LogError("TODO");
					break;
				case eUIType.HorOnly:
					rect.sizeDelta = new Vector2(rect.rect.width * ratio.x, rect.rect.height);
					break;
			}
		}
	}
}
