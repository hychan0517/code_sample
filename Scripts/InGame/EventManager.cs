using System.Collections.Generic;
using UnityEngine;
using GlobalDefine;
using System;
using System.Collections;

public class EventManager : MonoBehaviour
{
	private Action _gameStartEvent;
	private Action _gameOverEvent;
	private Action _gameClearEvent;
	private List<IActiveTimer> _activeTimerList = new List<IActiveTimer>();
	private List<IEnableTimer> _enableTimerList = new List<IEnableTimer>();
	private const int TEN_SECONDS = 100000000;
	private void Awake()
	{
		StartCoroutine(Co_CheckActiveTimer());
	}

	public void AddGameStartEvent(Action addEvent)
	{
		_gameStartEvent += addEvent;
	}

	public void ActiveGameStartEvent()
	{
		_gameStartEvent?.Invoke();
	}

	public void AddGameOverEvent(Action addEvent)
	{
		_gameOverEvent += addEvent;
	}

	public void ActiveGameOverEvent()
	{
		_gameOverEvent?.Invoke();
	}

	public void AddGameClearEvent(Action addEvent)
	{
		_gameClearEvent += addEvent;
	}

	public void ActiveGameClearEvent()
	{
		_gameClearEvent?.Invoke();
	}

	public void AddDestroyTimer(IActiveTimer invoker)
	{
		if(_activeTimerList.Contains(invoker) == false)
			_activeTimerList.Add(invoker);
	}

	public void AddDisableTimer(IEnableTimer invoker)
	{
		if(_enableTimerList.Contains(invoker) == false)
			_enableTimerList.Add(invoker);
	}
	private void RemoveActiveTimer(int index)
	{
		if (_activeTimerList.Count > index)
			_activeTimerList.RemoveAt(index);
		else
			LogManager.Log("Cannot Find RemoveActiveTimer");
	}

	private void RemoveEnableTimer(int index)
	{
		if (_enableTimerList.Count > index)
			_enableTimerList.RemoveAt(index);
		else
			LogManager.Log("Cannot Find RemoveEnableTimer");
	}

	private IEnumerator Co_CheckActiveTimer()
	{
		WaitForSeconds delay = new WaitForSeconds(10);
		while(true)
		{
			yield return delay;
			long nowTicks = DateTime.Now.Ticks;

			for (int i = 0; i < _enableTimerList.Count; ++i)
			{
				if ((Component)_enableTimerList[i] != null)
				{
					GameObject targetObject = _enableTimerList[i].GetGameObject();
					if (targetObject == null || targetObject.activeSelf == false || nowTicks - _enableTimerList[i].GetEnabledTicks() > TEN_SECONDS)
					{
						//설정한 시간 이상으로 오브젝트가 활성화되어 있으면 비활성화
						if(targetObject)
							targetObject.gameObject.SetActive(false);
						RemoveEnableTimer(i);
						--i;
					}
				}
				else
				{
					//다른 요인으로 오브젝트가 Destroy됐을 경우
					RemoveEnableTimer(i);
					--i;
				}
			}


			for (int i = 0; i < _activeTimerList.Count; ++i)
			{
				if ((Component)_activeTimerList[i] != null)
				{
					GameObject targetObject = _activeTimerList[i].GetGameObject();
					if (targetObject != null)
					{
						if(targetObject.activeSelf)
						{
							RemoveActiveTimer(i);
							--i;
						}
						else if (nowTicks - _activeTimerList[i].GetDisabledTicks() > TEN_SECONDS)
						{
							//설정한 시간 이상으로 오브젝트가 사용되지 않을 경우 Destroy
							Destroy(targetObject);
							RemoveActiveTimer(i);
							--i;
						}
					}

				}
				else
				{
					//다른 요인으로 오브젝트가 Destroy됐을 경우
					RemoveActiveTimer(i);
					--i;
				}
			}
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
