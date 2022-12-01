using DG.Tweening;
using UnityEngine;

public class UITweener : MonoBehaviour
{
	[SerializeField]
	private GameObject _tweenObject;
	private Vector3 _originScale;

	private void Awake()
	{
		_originScale = transform.localScale;
	}

	private void OnEnable()
	{
		if (_tweenObject)
		{
			if (App.Ins.optionDataTable.isTweenAnimation)
			{
				_tweenObject.transform.localScale = Vector3.zero;
				_tweenObject.transform.DOScale(Vector3.one, App.Ins.tweenDurationTime);
			}
			else
			{
				_tweenObject.transform.localScale = _originScale;
			}
		}

	}
}
