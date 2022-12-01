using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioMusic : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audioSource;

	private void Awake()
	{
		if (_audioSource)
		{
			AudioManager.Ins.OnValueChangedMusicVolume += SetVolume;
			_audioSource.volume = App.Ins.optionDataTable.musicVolume;
		}
		else
		{
			LogManager.LogError(string.Format("Cannot Find {0}._audioSource", gameObject.name));
		}

		if (_audioSource.clip == null)
			LogManager.LogError(string.Format("Cannot Find {0}._audioSource.clip", gameObject.name));
	}

	private void SetVolume(float volume)
	{
		if (_audioSource)
			_audioSource.volume = volume;
	}

	private void OnDestroy()
	{
		if(AudioManager.Ins)
			AudioManager.Ins.OnValueChangedMusicVolume -= SetVolume;
	}
}
