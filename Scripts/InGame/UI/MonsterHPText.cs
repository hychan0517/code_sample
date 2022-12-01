using DG.Tweening;
using TMPro;
using UnityEngine;

public class MonsterHPText : MonoBehaviour
{
	private TextMeshProUGUI _text;
	private Vector3 _originScale;

	private Tween _scaleTween;
	private Tween _colorTween;

	private const float DURATION_TIME = 0.5f;

	public void InitUIObject()
	{
		_text = GetComponent<TextMeshProUGUI>();
		if (_text == null)
		{
			LogManager.LogError("CannotFind MonsterHPText Component");
			Destroy(gameObject);
		}
		else
		{
			_originScale = transform.localScale;
			gameObject.SetActive(false);
		}
	}

	public void SettingText(Vector3 position, int damage)
	{
		ResetUI();
		transform.position = position + (Vector3.up * 3);
		_text.text = damage.ToString();
		_colorTween = _text.DOColor(new Color(1, 1, 1, 0.5f), DURATION_TIME);
		_scaleTween = _text.transform.DOScale(_originScale, DURATION_TIME).OnComplete(SetActiveOff);
	}

	private void ResetUI()
	{
		gameObject.SetActive(true);
		_text.transform.localScale = _originScale * 2;
		_text.color = Color.white;

		if (_scaleTween != null && _scaleTween.active)
			_scaleTween.Kill();
		if (_colorTween != null && _colorTween.active)
			_colorTween.Kill();
	}

	private void SetActiveOff()
	{
		gameObject.SetActive(false);
	}

	//private void Update()
	//{
	//	//TODO : 카메라와 직교하는 방향으로 올리기? 카메라 바라보기?
	//}
}
