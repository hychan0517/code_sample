using GlobalDefine;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHPTextManager : MonoBehaviour
{
	private List<MonsterHPText> _monsterHPTextList = new List<MonsterHPText>();
	private GameObject _textSample;

	public void InitObject()
	{
		_textSample = Resources.Load<GameObject>(ResourcesPath.MONSTER_HP_TEXT);
		if (_textSample == null)
			LogManager.LogError("Cannot Find MonsterHPText");
	}

	public void SettingMonsterHPText(Monster monster, int damage)
	{
		MonsterHPText monsterHPText = GetInActiveUI();
		if (monsterHPText)
		{
			monsterHPText.SettingText(monster.transform.position, damage);
		}
	}

	private MonsterHPText GetInActiveUI()
	{
		foreach (MonsterHPText i in _monsterHPTextList)
		{
			if (i && i.gameObject.activeSelf == false)
				return i;
		}

		return GetActiveUIOnCreate();
	}


	private MonsterHPText GetActiveUIOnCreate()
	{
		if (_textSample)
		{
			GameObject initText = Instantiate(_textSample, transform);
			MonsterHPText monsterHPText = initText.GetComponent<MonsterHPText>();
			monsterHPText.InitUIObject();
			_monsterHPTextList.Add(monsterHPText);
			return monsterHPText;
		}
		else
			return null;
	}
}
