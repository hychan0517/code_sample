using GlobalDefine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	#region SingleTon
	private static AudioManager _instance = null;
    private static bool _destroyFlag = false;

    public static AudioManager Ins
    {
        get
        {
            if (_destroyFlag)
                return null;

            if (_instance == null)
            {
                _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = "AudioManager";
                    _instance = container.AddComponent(typeof(AudioManager)) as AudioManager;
                }
            }

            return _instance;
        }
    }
    #endregion

    public enum eSoundType
    {
        None,
    }
    public enum eMusicType
    {
        None,
    }

    private List<AudioSource> _musicTable = new List<AudioSource>();
    private List<AudioSource> _soundTable = new List<AudioSource>();
    private List<AudioSource> _clickSoundList = new List<AudioSource>();
    public Action<float> OnValueChangedMusicVolume;
    public Action<float> OnValueChangedSoundVolume;

    [SerializeField]
    private AudioClip _clickAudioSource;

    private int _nowClickSoundIndex;
    private int ClickSoundIndex
    {
        get
        {
            ++_nowClickSoundIndex;
            if (_nowClickSoundIndex == _clickSoundList.Count)
                _nowClickSoundIndex = 0;

            return _nowClickSoundIndex;
        }
    }
    private const int MAX_CLICK_SOUND = 5;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (_clickAudioSource == null)
            LogManager.LogError("Cannot Find _clickAudioSource");

        RegisterAudioSource();
        RegisterClickAudioSource();
    }

    public void SetMusicVolume(float volume)
	{
        OnValueChangedMusicVolume?.Invoke(volume);
    }

    public void SetSoundVolume(float volume)
	{
        OnValueChangedSoundVolume?.Invoke(volume);
        for (int i = 0; i < _clickSoundList.Count; ++i)
        {
            if(_clickSoundList[i])
                _clickSoundList[i].volume = volume;
        }
    }

    private void RegisterAudioSource()
    {
        //AddMusic("None", true);
        //AddSound("None", true);
    }

    private void RegisterClickAudioSource()
    {
        if (_clickAudioSource)
        {
            for (int i = 0; i < MAX_CLICK_SOUND; ++i)
            {
                AudioSource addAudioSource = gameObject.AddComponent<AudioSource>();
                addAudioSource.clip = _clickAudioSource;
                addAudioSource.volume = App.Ins.optionDataTable.soundVolume;
                addAudioSource.playOnAwake = false;
                _clickSoundList.Add(addAudioSource);
            }
        }
    }

    private void AddMusic(string name, bool isLoop)
    {
        AudioSource addAudioSource = gameObject.AddComponent<AudioSource>();
        try
        {
            addAudioSource.clip = Resources.Load<AudioClip>(string.Format(ResourcesPath.MUSIC_PATH, name));
            addAudioSource.volume = App.Ins.optionDataTable.musicVolume;
            addAudioSource.loop = isLoop;
            _musicTable.Add(addAudioSource);
        }
        catch (Exception e)
        {
            _musicTable.Add(addAudioSource);
            LogManager.LogError(e.Message);
            LogManager.LogError(string.Format("Init Audio Err Sound Name : {0}", name));
        }
    }

    private void AddSound(string name,bool isLoop)
    {
        AudioSource addAudioSource = gameObject.AddComponent<AudioSource>();
        try
        {
            addAudioSource.clip = Resources.Load<AudioClip>(string.Format(ResourcesPath.SOUND_PATH, name));
            addAudioSource.volume = App.Ins.optionDataTable.soundVolume;
            addAudioSource.loop = isLoop;
            _soundTable.Add(addAudioSource);
        }
        catch(Exception e)
        {
            _soundTable.Add(new AudioSource());
            LogManager.LogError(e.Message);
            LogManager.LogError(string.Format("Init Audio Err Sound Name : {0}",name));
        }
    }

    public void PlaySound(eSoundType soundType)
    {
        if (App.Ins.optionDataTable.soundVolume <= 0)
            return;

        if (_soundTable.Count > (int)soundType && _soundTable[(int)soundType].clip)
        {
            if(_soundTable[(int)soundType].isPlaying && _soundTable[(int)soundType].clip.length >= 1.0f)
            {
                GameObject audio = new GameObject("SoundAudio");
                var source = audio.AddComponent<AudioSource>();
                source.clip = Resources.Load(string.Format(ResourcesPath.SOUND_PATH, _soundTable[(int)soundType].clip.name)) as AudioClip;
                source.volume = App.Ins.optionDataTable.soundVolume;
                source.loop = _soundTable[(int)soundType].loop;
                source.Play();
            }
            else
                _soundTable[(int)soundType].Play();
        }
    }

    public void StopSound(eSoundType soundType)
    {
        if (_soundTable[(int)soundType])
        {
            _soundTable[(int)soundType].Stop();
        }
    }

    public void PlayMusic(eMusicType soundType)
    {
        if (App.Ins.optionDataTable.musicVolume <= 0)
            return;
        if (_musicTable.Count > (int)soundType && _musicTable[(int)soundType].clip)
        {
            _musicTable[(int)soundType].Play();
        }
    }

    public void StopMusic(eMusicType soundType)
    {
        int soundIndex = (int)soundType;
        if (_musicTable.Count > soundIndex && _musicTable[soundIndex].isPlaying)
        {
            _musicTable[(int)soundType].Stop();
        }
    }

    public void PlayClick()
    {
        int index = ClickSoundIndex;
        if (_clickSoundList[index] && _clickSoundList[index].volume != 0)
			_clickSoundList[index].Play();
	}

	private void OnDestroy()
	{
        _destroyFlag = true;
	}
}
