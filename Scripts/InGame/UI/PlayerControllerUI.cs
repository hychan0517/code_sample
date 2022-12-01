using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GlobalDefine;

public class PlayerControllerUI : MonoBehaviour
{
	private Image _bgImage;
	private EventTrigger _playerMoveTrigger;

	//Move
	private Image _stickImage;
	/// <summary> 조이스틱 최초 위치. Stick 이동 최대거리 계산용 </summary>
	private Vector2 _stickStartPos;
	/// <summary> BG원의 반지름. Stick 이동 최대거리 계산용 </summary>
	private float _bgHalfOfWidth;
	/// <summary> 조이스틱으로 움직인 이동 방향 </summary>
	private Vector2 _moveDir;
	/// <summary> 플레이어 UI 인풋 여부 </summary>
	private bool _playerMoveFlag;

	//Attack
	//private Button _baseAttakButton;
	private Button _skillAttackButton;
	private Image _skillAttackImage;

	public void InitUIObject()
	{
		GameObject[] childs = gameObject.GetChildsObject();
		foreach(GameObject child in childs)
		{
			if(string.Equals(child.name, "Move"))
			{
				GameObject[] moveArr = child.GetChildsObject();
				foreach(GameObject move in moveArr)
				{
					if (string.Equals(move.name, "BG"))
					{
						_bgImage = move.GetComponent<Image>();
						_playerMoveTrigger = move.GetComponent<EventTrigger>();
						EventTrigger.Entry enterEntry = new EventTrigger.Entry();
						EventTrigger.TriggerEvent enterEvent = new EventTrigger.TriggerEvent();
						enterEvent.AddListener(OnPlayerMoveEnter);
						enterEntry.eventID = EventTriggerType.PointerDown;
						enterEntry.callback = enterEvent;
						_playerMoveTrigger.triggers.Add(enterEntry);

						EventTrigger.Entry exitEntry = new EventTrigger.Entry();
						EventTrigger.TriggerEvent exitEvent = new EventTrigger.TriggerEvent();
						exitEvent.AddListener(OnPlayerMoveExit);
						exitEntry.eventID = EventTriggerType.PointerUp;
						exitEntry.callback = exitEvent;
						_playerMoveTrigger.triggers.Add(exitEntry);

						_bgHalfOfWidth = _bgImage.rectTransform.rect.width * 0.5f;
					}
					else if (string.Equals(move.name, "Stick"))
					{
						_stickImage = move.GetComponent<Image>();
					}
				}
			}
			else if(string.Equals(child.name, "Attack"))
			{
				//2022-06-11 HYC : 기본공격 버튼 삭제, 자동공격으로 변경
				//_baseAttakButton = child.GetComponent<Button>();
				//_baseAttakButton.onClick.AddListener(BaseAttack);
			}
			else if (string.Equals(child.name, "SkillAttack"))
			{
				_skillAttackButton = child.GetComponent<Button>();
				_skillAttackButton.onClick.AddListener(OnClickSkillAttack);

				GameObject[] skillAttackArr = child.GetChildsObject();
				foreach (GameObject skillAttack in skillAttackArr)
				{
					if (string.Equals(skillAttack.name, "SkillAttackIconImage"))
					{
						
						_skillAttackImage = skillAttack.GetComponent<Image>();
						_skillAttackImage.gameObject.SetActive(false);
					}
				}
			}
		}

		_stickStartPos = App_InGame.Ins.GetUICamera().WorldToScreenPoint(_stickImage.transform.position);
	}

	/// <summary> 습득한 활 공격 스킬 아이콘 표기 </summary>
	public void SetSkillAttack(eBaseAttackSkillType type)
	{
		_skillAttackImage.gameObject.SetActive(true);
		switch (type)
		{
			case eBaseAttackSkillType.Fire:
				break;
			case eBaseAttackSkillType.Ice:
				break;
			case eBaseAttackSkillType.Light:
				break;
		}
	}

	//Unity Event=========================================================================================================================
	#region Unity Event
	/// <summary> 조이스틱 BG 범위 내 포인트 다운 </summary>
	private void OnPlayerMoveEnter(BaseEventData arg0)
	{
		_playerMoveFlag = true;
	}

	/// <summary> 조이스틱 포인트 업 </summary>
	private void OnPlayerMoveExit(BaseEventData arg0)
	{
		_playerMoveFlag = false;
		_stickImage.rectTransform.anchoredPosition = Vector2.zero;
	}

	private void OnClickSkillAttack()
	{
		//스킬어택 습득 여부는 이미지 활성/비활성으로 판단
		if (_skillAttackImage.gameObject.activeSelf)
		{
			App_InGame.Ins.GetPlayer().BaseAttackSkillToTarget();
			_skillAttackImage.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
#if UNITY_EDITOR
		MoveStickToDebug();
#else
		if (_playerMoveFlag)
		{
			MoveStickToMousePosition();
		}
#endif
	}
	#endregion
	//=========================================================================================================================Unity Event


	/// <summary> 조이스틱 이미지 위치 수정 </summary>
	private void MoveStickToMousePosition()
	{
		Vector2 mousePos = Input.mousePosition;
		Vector2 dir = mousePos - _stickStartPos;
		Vector2 stickDir = dir;
		SetPlayerMoveDir(dir);
		
		//이미지 이동 최대거리 계산
		if (dir.sqrMagnitude > _bgHalfOfWidth * _bgHalfOfWidth)
			stickDir = dir.normalized * _bgHalfOfWidth;

		_stickImage.rectTransform.anchoredPosition = stickDir;
	}

	private void MoveStickToDebug()
	{
		Vector2 dir = Vector2.zero;
		bool isDubugMove = false;
		if(Input.GetKey(KeyCode.W))
		{
			isDubugMove = true;
			dir.y += 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			isDubugMove = true;
			dir.y -= 1;
		}
		if (Input.GetKey(KeyCode.A))
		{
			isDubugMove = true;
			dir.x -= 1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			isDubugMove = true;
			dir.x += 1;
		}
		if(isDubugMove)
		{
			_playerMoveFlag = true;
			if (dir.sqrMagnitude > _bgHalfOfWidth * _bgHalfOfWidth)
				dir = dir.normalized * _bgHalfOfWidth;

			_stickImage.rectTransform.anchoredPosition = dir;
		}
		else
		{
			_playerMoveFlag = false;
			_stickImage.rectTransform.anchoredPosition = Vector2.zero;
		}
		SetPlayerMoveDir(dir);
	}

	/// <summary> 플레이어 UI 인풋 여부 리턴 </summary>
	public bool IsPlayerMove()
	{
		return _playerMoveFlag;
	}

	/// <summary> 조이스틱으로 움직인 이동 방향 저장 </summary>
	private void SetPlayerMoveDir(Vector2 dir)
	{
		_moveDir = dir;
	}

	/// <summary> 조이스틱으로 움직인 이동 방향 리턴 </summary>
	public Vector2 GetPlayerMoveDir()
	{
		return _moveDir.normalized;
	}

	private void OnDestroy()
	{
		if (_playerMoveTrigger && _playerMoveTrigger.triggers.Count > 0)
			_playerMoveTrigger.triggers.RemoveRange(0, _playerMoveTrigger.triggers.Count - 1);

		//if (_baseAttakButton)
		//	_baseAttakButton.onClick.RemoveAllListeners();
		if (_skillAttackButton)
			_skillAttackButton.onClick.RemoveAllListeners();
	}
}
