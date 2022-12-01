using UnityEngine;
using System.Collections.Generic;
using GlobalDefine;

public class MonsterHPManager : MonoBehaviour
{
		private List<MonsterHPGauge> _monsterHPGaugeList = new List<MonsterHPGauge>();
		private GameObject _gaugeSample;

		public void InitObject()
		{
			_gaugeSample = Resources.Load<GameObject>(ResourcesPath.MONSTER_HP_GAUGE);
			if (_gaugeSample == null)
				LogManager.LogError("Cannot Find MonsterHPGauge");
		}

		public void SettingMonsterHPGauge(Monster monster)
		{
			MonsterHPGauge gauge = GetInActiveUI();
			if (gauge)
			{
				gauge.SettingMonster(monster);
			}
		}

		private MonsterHPGauge GetInActiveUI()
		{
			foreach (MonsterHPGauge i in _monsterHPGaugeList)
			{
				if (i && i.gameObject.activeSelf == false)
					return i;
			}

			return GetActiveUIOnCreate();
		}


		private MonsterHPGauge GetActiveUIOnCreate()
		{
			if (_gaugeSample)
			{
				GameObject initGauge = Instantiate(_gaugeSample, transform);
				MonsterHPGauge gauge = initGauge.GetComponent<MonsterHPGauge>();
				gauge.InitUIObject();
				_monsterHPGaugeList.Add(gauge);
				return gauge;
			}
			else
				return null;
		}
}
