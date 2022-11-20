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
	/// <summary> �� ������ ���ϴ� ī�޶� Vector�� </summary>
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
	/// <summary> ���� �� </summary>
	[SerializeField]
	private float _vibrateCount = 4f;
	/// <summary> ���� ���� </summary>
	[SerializeField]
	private float _vibratePower = 3f;
	/// <summary> 1ȸ ������ �ð� </summary>
	[SerializeField]
	private float _vibrateTime = 0.2f;


	//Public=============================================================================================================
	#region Public
	public void InitPlayer(GameObject player)
	{
		_playerObject = player;
	}

	/// <summary> ������ �ʱ�ȭ, ���� ���� </summary>
	public void OnStartVibrate()
	{
		KillVibrateCor();
		_vibrateCor = StartCoroutine(Co_Vibrate());
	}

	/// <summary> ������ �ʱ�ȭ, ���� ���� </summary>
	public void OnStartZoomInOut()
	{
		KillZoomInOutCor();
		_zoomInOutCor = StartCoroutine(Co_ZoomIn());
	}

	private void LateUpdate()
	{
		if (_playerObject)
		{
			//��, ����, �÷��̾� ������ ��ü �ݿ��� ������ġ
			Vector3 vDesiredPosition = _playerObject.transform.position + _positionOffset + _zoomDir + _vibrateDir;

			//ī�޶� �ӵ������� ����
			transform.position = Vector3.Lerp(transform.position, vDesiredPosition, Time.deltaTime * GetCameraSpeed((vDesiredPosition - transform.position).sqrMagnitude));
			transform.forward = _forwardOffset;
		}
	}
	#endregion
	//=============================================================================================================Public

	//Vibrate============================================================================================================
	#region Vibrate
	/// <summary> �������� �ڷ�ƾ, �� �ʱ�ȭ </summary>
	private void KillVibrateCor()
	{
		if (_vibrateCor != null)
			StopCoroutine(_vibrateCor);

		_currVibrateCount = 0;
		_vibrateDir = Vector3.zero;
	}

	IEnumerator Co_Vibrate()
	{
		//_vibrateCount��ŭ ����
		while (_currVibrateCount <= _vibrateCount)
		{
			//_vibrateTime�ð����� ��ġ �缳��, ������ġ -> �������� -> ������ġ �ݺ�
			if (_vibrateReturnFlag)
				SetPosition();
			else
				SetStartPosition();

			yield return new WaitForSeconds(_vibrateTime);
		}

		KillVibrateCor();
	}

	/// <summary> ���� ��ǥ ���� ��ġ ���� </summary>
	private void SetPosition()
	{
		_vibrateReturnFlag = false;
		float fX = Random.Range(-_vibratePower, _vibratePower);
		float fY = Random.Range(-_vibratePower, _vibratePower);
		float fZ = Random.Range(-_vibratePower, _vibratePower);
		_vibrateDir = new Vector3(fX, fY, fZ);
		++_currVibrateCount;
	}

	/// <summary> ���� ��ǥ ���� ��ġ�� �ʱ�ȭ </summary>
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
	/// <summary> ���ΰ��� �ڷ�ƾ, �� �ʱ�ȭ </summary>
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
			//����
			_zoomInOutElapesTime += Time.deltaTime;
			_zoomDir = _forwardOffset * (_zoomDistance * (_zoomInOutElapesTime * (1 / _zoomInDuration)));
			yield return null;
		}

		KillZoomInOutCor();
		_zoomInOutCor = StartCoroutine(Co_ZoomOut());
	}

	/// <summary> ���ΰ��� �ڷ�ƾ, �� �ʱ�ȭ </summary>
	private IEnumerator Co_ZoomOut()
	{
		while (_zoomInOutElapesTime <= _zoomOutDuration)
		{
			//�ܾƿ�
			_zoomInOutElapesTime += Time.deltaTime;
			_zoomDir = _forwardOffset * (_zoomDistance * (1 - (_zoomInOutElapesTime * (1 / _zoomOutDuration))));
			yield return null;
		}

		KillZoomInOutCor();
	}
	//===============================================================================================================Zoom
	#endregion


	/// <summary> ī�޶�� �÷��̾� �Ÿ������� �ӵ� </summary>
	private float GetCameraSpeed(float distance)
	{
		return Mathf.Clamp(distance / 10, 1, 10);
	}


	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
