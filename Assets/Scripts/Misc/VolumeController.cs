using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class VolumeController : MonoBehaviour
{

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _audioManager;
    private int STREAM_MUSIC = 3; // Constant Value: 3 (0x00000003) in Android
    private int _MaxVolume;
    private int _SavedVolume;
	private float _AudioCheckInterval = 0.5f;
	private const string _PLAYERPREFSKEY = "SavedVolume";
#endif

    public bool IsAudioOn
    {
        get { return !AudioListener.pause; }
    }

    /// <summary>
    /// Set the global application volume
    /// </summary>
    /// <param name="volume">Volume percentage between 0.0 and 1.0 - 0% to 100%</param>
    public void SetAudioPlaying(bool audioOn)
    {
        bool mute = !audioOn;
#if UNITY_ANDROID && !UNITY_EDITOR
        int integerVolume = mute ? 0 : _SavedVolume;
        if(mute)
        {
            _SavedVolume = _audioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
			if(_SavedVolume ==0)
			{
				_SavedVolume = Mathf.FloorToInt(_MaxVolume/2);
			}
        }
        _audioManager.Call("setStreamVolume", STREAM_MUSIC, integerVolume, 0);
#endif

        AudioListener.pause = mute;

		if (ResourceManager.Instance.MediaPlayer != null)
        {
			ResourceManager.Instance.MediaPlayer.SetMute(mute);
        }

        OnAudioStatusChanged(audioOn);
    }

    public event EventHandler<AudioStatusEventArgs> AudioStatusChanged;

    protected void OnAudioStatusChanged(bool status)
    {
        var ev = AudioStatusChanged;
        if (ev != null)
        {
            var args = new AudioStatusEventArgs(IsAudioOn);
            ev(this, args);
        }
    }

    private void Start()
    {
#if (UNITY_ANDROID) && (!UNITY_EDITOR)
        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");

        _audioManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "audio");
        _MaxVolume = _audioManager.Call<int>("getStreamMaxVolume", STREAM_MUSIC);
		int newVolume = _audioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
		if(newVolume <= 0)
		{
			//if a volume exists in playerprefs, default to that
			if(PlayerPrefs.GetInt(_PLAYERPREFSKEY) != 0)
			{
				_SavedVolume = PlayerPrefs.GetInt("SavedVolume");
			}else {
				_SavedVolume = Mathf.FloorToInt(_MaxVolume/2);
				PlayerPrefs.SetInt(_PLAYERPREFSKEY, _SavedVolume);
				PlayerPrefs.Save ();
			}

		}else{
			_SavedVolume = _audioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
			PlayerPrefs.SetInt(_PLAYERPREFSKEY, _SavedVolume);
			PlayerPrefs.Save ();
		}
		InvokeRepeating("CheckVolume", _AudioCheckInterval, _AudioCheckInterval);
#endif
    }

    private void CheckVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		int newVolume = _audioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
		if(newVolume <= 0) {
			EventManager.Instance.HardwareAudioMuteEvent(true);
		}else{
			_SavedVolume = newVolume;
			EventManager.Instance.HardwareAudioMuteEvent(false);
		}
#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		PlayerPrefs.SetInt(_PLAYERPREFSKEY, _SavedVolume);
		PlayerPrefs.Save ();
#endif
    }
}