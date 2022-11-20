using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	[Header("Object")]
	[SerializeField]
	private GameObject _playerObject;

	//Vibrate
	private bool _vibrateReturnFlag;
	private int _currVibrateCount;
	private Vector3 _vibrateDir;
	private Coroutine _vibrateCor;

	//Zoom
	/// <summary> 줌 인으로 변하는 카메라 Vector값 </summary>
	private Vector3 _zoomDir;
	private Coroutine _zoomInOutCor;
	private float _zoomInOutElapesTime;

	[Header("Option")]
	[SerializeField]
	private Vector3 _positionOffset = new Vector3(0, 17f, -8.1f);
	[SerializeField]
	private Vector3 _forwardOffset = new Vector3(0, -0.866f, 0.5f);
	[SerializeField]
	private float _zoomInDuration = 1f;
	[SerializeField]
	private float _zoomOutDuration = 0.3f;
	[SerializeField]
	private float _zoomDistance = 3f;
	/// <summary> 진동 수 </summary>
	[SerializeField]
	private float _vibrateCount = 4f;
	/// <summary> 진동 세기 </summary>
	[SerializeField]
	private float _vibratePower = 3f;
	/// <summary> 1회 진동당 시간 </summary>
	[SerializeField]
	private float _vibrateTime = 0.2f;


	//Public=============================================================================================================
	#region Public
	public void InitPlayer(GameObject player)
	{
		_playerObject = player;
	}

	/// <summary> 데이터 초기화, 진동 시작 </summary>
	public void OnStartVibrate()
	{
		KillVibrateCor();
		_vibrateCor = StartCoroutine(Co_Vibrate());
	}

	/// <summary> 데이터 초기화, 줌인 시작 </summary>
	public void OnStartZoomInOut()
	{
		KillZoomInOutCor();
		_zoomInOutCor = StartCoroutine(Co_ZoomIn());
	}

	private void LateUpdate()
	{
		if (_playerObject)
		{
			//줌, 진동, 플레이어 움직임 전체 반영된 최종위치
			Vector3 vDesiredPosition = _playerObject.transform.position + _positionOffset + _zoomDir + _vibrateDir;

			//카메라 속도에따른 보간
			transform.position = Vector3.Lerp(transform.position, vDesiredPosition, Time.deltaTime * GetCameraSpeed((vDesiredPosition - transform.position).sqrMagnitude));
			transform.forward = _forwardOffset;
		}
	}
	#endregion
	//=============================================================================================================Public

	//Vibrate============================================================================================================
	#region Vibrate
	/// <summary> 진동관련 코루틴, 값 초기화 </summary>
	private void KillVibrateCor()
	{
		if (_vibrateCor != null)
			StopCoroutine(_vibrateCor);

		_currVibrateCount = 0;
		_vibrateDir = Vector3.zero;
	}

	IEnumerator Co_Vibrate()
	{
		//_vibrateCount만큼 진동
		while (_currVibrateCount <= _vibrateCount)
		{
			//_vibrateTime시간마다 위치 재설정, 랜덤위치 -> 시작지점 -> 랜덤위치 반복
			if (_vibrateReturnFlag)
				SetPosition();
			else
				SetStartPosition();

			yield return new WaitForSeconds(_vibrateTime);
		}

		KillVibrateCor();
	}

	/// <summary> 진동 목표 랜덤 위치 설정 </summary>
	private void SetPosition()
	{
		_vibrateReturnFlag = false;
		float fX = Random.Range(-_vibratePower, _vibratePower);
		float fY = Random.Range(-_vibratePower, _vibratePower);
		float fZ = Random.Range(-_vibratePower, _vibratePower);
		_vibrateDir = new Vector3(fX, fY, fZ);
		++_currVibrateCount;
	}

	/// <summary> 진동 목표 최초 위치로 초기화 </summary>
	private void SetStartPosition()
	{
		_vibrateReturnFlag = true;
		_vibrateDir = Vector3.zero;
		++_currVibrateCount;
	}
	#endregion
	//============================================================================================================Vibrate


	//Zoom===============================================================================================================
	#region Zoom
	/// <summary> 줌인관련 코루틴, 값 초기화 </summary>
	private void KillZoomInOutCor()
	{
		if (_zoomInOutCor != null)
			StopCoroutine(_zoomInOutCor);

		_zoomDir = Vector3.zero;
		_zoomInOutElapesTime = 0;
		_zoomInOutCor = null;
	}

	private IEnumerator Co_ZoomIn()
	{
		while (_zoomInOutElapesTime <= _zoomInDuration)
		{
			//줌인
			_zoomInOutElapesTime += Time.deltaTime;
			_zoomDir = _forwardOffset * (_zoomDistance * (_zoomInOutElapesTime * (1 / _zoomInDuration)));
			yield return null;
		}

		KillZoomInOutCor();
		_zoomInOutCor = StartCoroutine(Co_ZoomOut());
	}

	/// <summary> 줌인관련 코루틴, 값 초기화 </summary>
	private IEnumerator Co_ZoomOut()
	{
		while (_zoomInOutElapesTime <= _zoomOutDuration)
		{
			//줌아웃
			_zoomInOutElapesTime += Time.deltaTime;
			_zoomDir = _forwardOffset * (_zoomDistance * (1 - (_zoomInOutElapesTime * (1 / _zoomOutDuration))));
			yield return null;
		}

		KillZoomInOutCor();
	}
	//===============================================================================================================Zoom
	#endregion


	/// <summary> 카메라와 플레이어 거리에따른 속도 </summary>
	private float GetCameraSpeed(float distance)
	{
		return Mathf.Clamp(distance / 10, 1, 10);
	}


	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
