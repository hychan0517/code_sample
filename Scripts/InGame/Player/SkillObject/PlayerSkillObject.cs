using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkillObject : MonoBehaviourPunCallbacks
{
	protected PlayerSkillDataTable _skillDataTable;
	protected AudioSound _audioSound;
	protected List<AudioSound> _soundList = new List<AudioSound>();

	/// <summary> 스킬 사용에 필요한 세팅이 있다면 함수 안에 구현, Instantiate할 때만 호출하도록함 </summary>
	public virtual void InitPlayerSkillObject(PlayerSkillDataTable skillDataTable)
	{
		if (skillDataTable != null)
		{
			_skillDataTable = skillDataTable;
			_audioSound = GetComponentInChildren<AudioSound>(true);
			if (_audioSound)
			{
				_audioSound.Init();
				_soundList.Add(_audioSound);
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

	/// <summary> 스킬 발사체, 버프 이펙트 등 스킬 사용에 사용되는 오브젝트 활성화, 초기화 시점은 GetActiveSkillOnCreate로 오브젝트가 생성될 때 </summary>
	public abstract void ActivePlayerSkillObject();


	//Pun Events==============================================================================================
	#region Pun Events
	[PunRPC]
	protected virtual void InActivePlayerSkill()
	{
		gameObject.SetActive(false);
	}

	protected void PlaySound()
	{
		if (_audioSound)
		{
			if (_audioSound.gameObject.activeSelf)
			{
				_audioSound.PlaySound();
			}
			else
			{
				AudioSound inActiveSound = GetInActiveSound();
				if (inActiveSound)
					inActiveSound.PlaySound();
			}
		}
	}

	private AudioSound GetInActiveSound()
	{
		for(int i = 0; i < _soundList.Count; ++i)
		{
			if (_soundList[i] && _soundList[i].gameObject.activeSelf)
				return _soundList[i];
		}

		return InstantiateSound();
	}

	private AudioSound InstantiateSound()
	{
		if(_audioSound)
		{
			GameObject instantiate = Instantiate(_audioSound.gameObject, transform);
			AudioSound audioSound = instantiate.GetComponent<AudioSound>();
			_soundList.Add(audioSound);
			return audioSound;
		}
		else
		{
			return null;
		}
	}
	#endregion
	//==============================================================================================Pun Events
}
