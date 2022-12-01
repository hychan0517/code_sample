using UltimateClean;
using UnityEngine;
using System.Collections;
using GlobalDefine;

public class MonsterHPGauge : MonoBehaviour
{
	private Monster _monster;
	private SlicedFilledImage _slider;
	private float _lastProcessHP;
	private Coroutine _hpCoroutine;

	public void InitUIObject()
	{
		_slider = gameObject.GetChildObjectOptimize("Loading Bar").GetComponent<SlicedFilledImage>();
	}

	public void SettingMonster(Monster monster)
	{
		gameObject.SetActive(true);
		_monster = monster;
		_slider.fillAmount = 1;
		if (_monster)
			_lastProcessHP = _monster.GetNowHP();
	}

	private void Update()
	{
		if(_monster != null && _monster.gameObject.activeSelf && _slider.fillAmount > 0)
		{
			transform.position = _monster.transform.position + Vector3.up * 2f;
			if(_monster.GetNowHP() != _lastProcessHP)
			{
				if (_hpCoroutine != null)
					StopCoroutine(_hpCoroutine);

				_hpCoroutine = StartCoroutine(Co_DoHPGauge(_monster.GetNowHPRate()));
				_lastProcessHP = _monster.GetNowHP();
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private IEnumerator Co_DoHPGauge(float targetFill)
	{
		float nowFill = _slider.fillAmount;
		float gapGille = targetFill - nowFill;
		float deltaTime = 0;
		while(deltaTime <= Define.MONSTER_HP_DURATION)
		{
			yield return null;
			deltaTime += Time.deltaTime;
			_slider.fillAmount = Mathf.Clamp(nowFill + gapGille * (deltaTime * (1 / Define.MONSTER_HP_DURATION)), 0, 1);
		}
		_hpCoroutine = null;
	}
}
