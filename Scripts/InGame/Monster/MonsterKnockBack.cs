using System.Collections;
using UnityEngine;

public class MonsterKnockBack : MonoBehaviour
{
	private Rigidbody _rig;
	private void Awake()
	{
		_rig = GetComponent<Rigidbody>();
	}
	//private Vector3 _originPosition;
	//private Vector3 _targetDir;
	//private Vector3 _knockBackedDir;
	//private float _drag;
	//private float _knockBackTime;
	public void OnStartKnockBack(Vector3 dir, float power)
	{
		//_originPosition = transform.position;
		//_targetDir = dir * power;
		//_knockBackTime = 0.3f;
		if(_rig)
		{
			_rig.AddForce(dir * power);
		}
	}

	//private void FixedUpdate()
	//{
	//	if(_knockBackTime >= 0)
	//	{
	//		//TODO : 맵 충돌체크
	//		_knockBackTime -= Time.deltaTime;
	//	}
	//}
}
