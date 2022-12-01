using UnityEngine;
using GlobalDefine;

public class UINotchController : MonoBehaviour
{
	public enum eChangeTypeOfNotch
	{
		None,
		LeftPosition,
		RightPosition,
		BottomPosition,
		SideScale,
	}

	public eChangeTypeOfNotch _changeTypeOfNotch;
	public float _ratio = 1;
	private bool _scaleFlag = false;

	private void Start()
	{
		if(_scaleFlag == false)
			Scaling();
	}

	public void InitController(eChangeTypeOfNotch type, float ratio)
	{
		_changeTypeOfNotch = type;
		_ratio = ratio;
		if (_scaleFlag == false)
			Scaling();
	}

	private void Scaling()
	{
		_scaleFlag = true;
		RectTransform rect = GetComponent<RectTransform>();
		RectTransform canvasRect = App.Ins.globalUI.canvasRectTransform;

		if (rect && canvasRect)
		{
			float safeWidthRatio = 1 - (Screen.safeArea.width / Screen.width);
			float safeHeightRatio = 1 - (Screen.safeArea.height / Screen.height);
			float safeWidth = canvasRect.rect.width * safeWidthRatio;
			float safeHeight = canvasRect.rect.height * safeHeightRatio;
			switch (_changeTypeOfNotch)
			{
				case eChangeTypeOfNotch.BottomPosition:
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + safeHeight);
					break;
				case eChangeTypeOfNotch.LeftPosition:
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + (safeWidth / 2 * _ratio), rect.anchoredPosition.y);
					break;
				case eChangeTypeOfNotch.RightPosition:
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - (safeWidth / 2 * _ratio), rect.anchoredPosition.y);
					break;
				case eChangeTypeOfNotch.SideScale:
					rect.sizeDelta = new Vector3(rect.rect.width * (safeWidthRatio * _ratio), rect.rect.height);
					break;
			}
		}
	}
}
