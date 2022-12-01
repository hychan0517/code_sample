using UnityEngine;
using GlobalDefine;
using System;

public class LifeTimeObject : MonoBehaviour, IEnableTimer, IActiveTimer
{
	private long _enagleTimeTicks;
	private long _disableTimeTicks;

	private void OnEnable()
	{
		SetEnableTicks();
		App_InGame appIngame = App_InGame.Ins;
		if(appIngame)
			App_InGame.Ins.GetEventManager().AddDisableTimer(this);
	}

	private void OnDisable()
	{
		SetDisableTicks();
		App_InGame appIngame = App_InGame.Ins;
		if (appIngame)
			App_InGame.Ins.GetEventManager().AddDestroyTimer(this);
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}

	public void SetEnableTicks()
	{
		_enagleTimeTicks = DateTime.Now.Ticks;
	}

	public void SetDisableTicks()
	{
		_disableTimeTicks = DateTime.Now.Ticks;
	}

	public long GetEnabledTicks()
	{
		return _enagleTimeTicks;
	}

	public long GetDisabledTicks()
	{
		return _disableTimeTicks;
	}
}
