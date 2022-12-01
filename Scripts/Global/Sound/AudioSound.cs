using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSound : MonoBehaviour
{
	private AudioSource _audioSource;
	private Transform _parent;

	public void Init()
	{
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource)
		{
			AudioManager.Ins.OnValueChangedSoundVolume += SetVolume;
			_audioSource.volume = App.Ins.optionDataTable.soundVolume;
		}
		else
		{
			LogManager.LogError(string.Format("Cannot Find {0}._audioSource", gameObject.name));
		}

		if(_audioSource.clip == null)
			LogManager.LogError(string.Format("Cannot Find {0}._audioSource.clip", gameObject.name));

		gameObject.SetActive(false);
	}

	public void PlaySound()
	{
		gameObject.SetActive(true);
		if (_audioSource && _audioSource.clip)
		{
			_parent = transform.parent;
			transform.SetParent(null);
			StartCoroutine(Co_DelaySound());
		}
	}

	private IEnumerator Co_DelaySound()
	{
		yield return new WaitForSeconds(_audioSource.clip.length);
		if (_parent)
		{
			transform.SetParent(_parent);
			_parent = null;
			gameObject.SetActive(false);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void SetVolume(float volume)
	{
		if (_audioSource)
			_audioSource.volume = volume;
	}

	private void OnDestroy()
	{
		if (AudioManager.Ins)
			AudioManager.Ins.OnValueChangedSoundVolume -= SetVolume;
	}
}
