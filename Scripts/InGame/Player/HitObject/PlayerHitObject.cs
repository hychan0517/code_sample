using GlobalDefine;
using System.Collections;
using UnityEngine;

public abstract class PlayerHitObject : LifeTimeObject
{
	protected PlayerSkillDataTable _skillDataTable;
	protected SphereCollider _collider;
	protected AudioSound _audioSound;

	public void InitObject(PlayerSkillDataTable skillDataTable)
	{
		_skillDataTable = skillDataTable;
		_collider = GetComponent<SphereCollider>();
		_audioSound = GetComponentInChildren<AudioSound>();
		if (_audioSound)
			_audioSound.Init();
	}
	public virtual void ActiveHitObject(Vector3 position)
	{
		gameObject.SetActive(true);
		transform.position = position;
		StartCoroutine(Co_DisableCollider());
		SetActiveCollider(true);
		if (_audioSound)
			_audioSound.PlaySound();
	}
	private IEnumerator Co_DisableCollider()
	{
		yield return Define.WAIT_FOR_SECONS_POINT_ONE;
		SetActiveCollider(false);
	}
	private void SetActiveCollider(bool isActive)
	{
		if (_collider)
			_collider.enabled = isActive;
	}
}
