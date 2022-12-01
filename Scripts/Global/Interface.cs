namespace GlobalDefine
{
    using UnityEngine;

    public interface ISortableByDate
    {
        long GetDate();
    }

    /// <summary> 오브젝트 생명주기용 이벤트 </summary>
    public interface IActiveTimer
	{
        GameObject GetGameObject();
        void SetDisableTicks();
        long GetDisabledTicks();
	}

    public interface IEnableTimer
	{
        GameObject GetGameObject();
        void SetEnableTicks();
        long GetEnabledTicks();
	}

    public interface IInstantiatable
	{
        void Init();
    }
}