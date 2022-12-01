using GlobalDefine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
	private List<StageDataTable> _stageDataList = new List<StageDataTable>();
	private int _lastSpawnMonsterIndex = 0;
	
	private List<Monster> _monsterList = new List<Monster>();
	/// <summary> 유저와 가장 가까운 몬스터의 _stageDataList 인덱스 </summary>
	private int _targetMonsterIndex;

	private float _startTime;
	private float _endTime;
	private float _huntCount;

	public void InitMonsterManager()
	{
		_stageDataList = App.Ins.stageDataManager.GetStageDataListByStageID(DataBridgeManager.Ins.GetNowStageID());
		_targetMonsterIndex = -1;

		App_InGame.Ins.GetEventManager().AddGameStartEvent(GameStart);
		App_InGame.Ins.GetEventManager().AddGameOverEvent(GameOver);
		App_InGame.Ins.GetEventManager().AddGameClearEvent(GameClear);
	}

	public void GameStart()
	{
		_startTime = Time.realtimeSinceStartup;
	}

	public void GameOver()
	{
		_targetMonsterIndex = -1;
		_endTime = Time.realtimeSinceStartup;
		Debug.LogError(_endTime);
	}

	public void GameClear()
	{
		_targetMonsterIndex = -1;
		_endTime = Time.realtimeSinceStartup;
	}

	public void FindTargetMonster()
	{
		Player player = App_InGame.Ins.GetPlayer();
		if (player)
		{
			int index = -1;
			float minDistance = float.MaxValue;
			for (int i = 0; i < _monsterList.Count; i++)
			{
				if (_monsterList[i] && _monsterList[i].IsLive())
				{
					float distance = (player.transform.position - _monsterList[i].transform.position).sqrMagnitude;
					float attackRange = player.GetPlayerDataTable().attackRange;
					if (distance < attackRange * attackRange && distance < minDistance)
					{
						index = i;
						minDistance = distance;	
					}
				}
			}
			SetTargetMonster(index);
		}
	}
	private void SetTargetMonster(int index)
	{
		if(index != _targetMonsterIndex)
		{
			if(_targetMonsterIndex != -1)
				_monsterList[_targetMonsterIndex].SettingTarget(false);
			if(index != -1)
				_monsterList[index].SettingTarget(true);
		}
		_targetMonsterIndex = index;
	}
	public Monster GetTargetMonster()
	{
		if (_targetMonsterIndex == -1)
			return null;
		else
			return _monsterList[_targetMonsterIndex];
	}

	private void FixedUpdate()
	{
		if (App_InGame.Ins._isStart)
		{
			FindTargetMonster();
			if (PhotonNetwork.IsMasterClient)
				CheckNextMonsterSpawn();
		}
	}

	private void CheckNextMonsterSpawn()
	{
		if (_stageDataList.Count > _lastSpawnMonsterIndex)
		{
			if (_stageDataList[_lastSpawnMonsterIndex].spawnSeconds <= Time.realtimeSinceStartup - _startTime)
			{
				SpawnNextMonster();
			}
		}
	}

	private void SpawnNextMonster()
	{
		StageDataTable nowStageData = _stageDataList[_lastSpawnMonsterIndex];
		Monster monster = GetDisabledMonster(nowStageData.monsterID);
		if(monster == null)
		{
			MonsterDataTable monsterData = App.Ins.monsterDataManager.GetMonsterDataByID(nowStageData.monsterID);
			if (monsterData != null)
			{
				LogManager.Log(string.Format("Instantiate New Monster Object Name : {0}", monsterData.name));
				GameObject instantiate = PhotonNetwork.Instantiate(string.Format(Define.MONSTER_PREFAB_PATH, monsterData.name), new Vector3(nowStageData.xPosition, nowStageData.yPosition, nowStageData.zPosition), Quaternion.Euler(nowStageData.xRotation, nowStageData.yRotation, nowStageData.zRotation));
				if (instantiate)
				{
					instantiate.transform.parent = transform;
					monster = instantiate.GetComponent<Monster>();
					monster.InitMonster();
				}
				else
				{
					LogManager.LogError(string.Format("SpawnNextMonster Instantiate Error Monster : {0}", monsterData.name));
				}
			}
			else
			{
				LogManager.LogError(string.Format("SpawnNextMonster MonsterData Error Monster ID : {0}", nowStageData.monsterID));
			}
		}

		if(monster != null)
		{
			monster.SettingMonster(_lastSpawnMonsterIndex + 1);
			_monsterList.Add(monster);
		}
		else
		{
			LogManager.LogError(string.Format("SpawnNextMonster Error StageID : {0}, Monster Index : {1}", DataBridgeManager.Ins.GetNowStageID(), nowStageData.monsterID));
		}
		++_lastSpawnMonsterIndex;

		CheckNextMonsterSpawn();
	}

	private Monster GetDisabledMonster(int monsterID)
	{
		for(int i = 0; i < _monsterList.Count; ++i)
		{
			if (_monsterList[i] && _monsterList[i].gameObject.activeSelf == false && _monsterList[i].GetCalculatedDateTable().monsterID == monsterID)
				return _monsterList[i];
		}
		return null;
	}

	public void SpawnMonsterPRC(Monster monster)
	{
		if (_monsterList.Contains(monster) == false)
			_monsterList.Add(monster);
		else
			LogManager.LogError(string.Format("SpawnedMonsterPRC Error. Monster ID : {0}", monster.GetCalculatedDateTable().monsterID));
	}

	public void AddHuntCount()
	{
		++_huntCount;
		if (_huntCount >= _stageDataList.Count)
			App_InGame.Ins.GameClear();
	}

	public float GetClearRate()
	{
		int totalCount = _stageDataList.Count;
		if (totalCount > 0)
		{
			return _huntCount / totalCount;
		}
		else
		{
			LogManager.LogError("GetClearRate Total Count Error");
			return 0;
		}	
	}

	public TimeSpan GetPlayTime()
	{
		float span = _endTime - _startTime;
		return new TimeSpan((long)span * 10000000);
	}
}
