using System.Linq;
using UnityEngine;

public class PHO_BaseAttack_F : PlayerHitObject
{
	private GameObject[] _targetMonster = new GameObject[10];

	public override void ActiveHitObject(Vector3 position)
	{
		base.ActiveHitObject(position);
		_targetMonster = new GameObject[10];
	}
	private void OnTriggerStay(Collider other)
	{
		if (_targetMonster.Contains(other.gameObject) == false)
		{
			for (int i = 0; i < _targetMonster.Length; ++i)
			{
				if (_targetMonster[i] == null)
				{
					Monster component = other.GetComponent<Monster>();
					if (component.IsLive())
					{
						_targetMonster[i] = other.gameObject;
						//넉백방향
						Vector3 dir = other.transform.position - transform.position;
						//사이즈비율로 멀어질수록 데미지감소
						float maxDistance = transform.localScale.x * _collider.radius;
						float damagerRate = 1 - Mathf.Clamp(dir.magnitude / maxDistance, 0.1f, 0.9f);
						component.Damage(_skillDataTable.optionDataArr[1] * damagerRate);
						component.KnockBack(dir.normalized, _skillDataTable.optionDataArr[2]);
						break;
					}
				}
			}
		}
	}
}
