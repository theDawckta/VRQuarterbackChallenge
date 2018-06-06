using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class AudioController : MonoBehaviour
{
    public AudioSource BackgroundAudio;
    public AudioClip GenericClickSound;
    public AudioClip ArrowClickSound;
    public AudioClip SmallClickSound;
    public AudioClip NotificationSound;
    public AudioClip HomeBackgroundAudio;
	public AudioClip GameBackgroundAudio;
    public bool IsHomeEnvironment = false;
	public bool IsGameEnvironment = false;

    private VolumeController _VolumeController;
    private float _UserPresentDelay = 1.0f;
    private bool _IsApplicationQuitting;
    private bool _isCurHome = true;
    private AudioSource _source;

    [HideInInspector]
    public enum AudioClips
    {
        GenericClick,
        ArrowClick,
        SmallClick,
        Notification
    }

    void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _VolumeController = Extensions.GetRequired<VolumeController>();

        if (BackgroundAudio != null)
        {
            SetBackgroundPlaying(UserPresenceController.Instance.IsMounted && _VolumeController.IsAudioOn);
        }

		if (IsGameEnvironment && GameBackgroundAudio)
		{
			BackgroundAudio.clip = GameBackgroundAudio;
			BackgroundAudio.volume = 0.0f;
			BackgroundAudio.Play();
			BackgroundAudio.DOFade(1.0f, 2.0f);
		}
    }

    void OnEnable()
    {
        EventManager.OnDrawTilesComplete += OnDrawTilesComplete;
        EventManager.OnBackgroundAudioTrigger += EventManager_OnBackgroundAudioTrigger;
        EventManager.OnSwitchToConsumption += EventManager_OnSwitchToConsumption;
        _VolumeController.AudioStatusChanged += VolumeController_AudioStatusChanged;
        UserPresenceController.Instance.PresenceChanged += Instance_PresenceChanged;
    }

    void OnDisable()
    {
        if (!_IsApplicationQuitting)
        {
            _VolumeController.AudioStatusChanged -= VolumeController_AudioStatusChanged;
            UserPresenceController.Instance.PresenceChanged -= Instance_PresenceChanged;
        }
        EventManager.OnDrawTilesComplete -= OnDrawTilesComplete;
        EventManager.OnBackgroundAudioTrigger -= EventManager_OnBackgroundAudioTrigger;
        EventManager.OnSwitchToConsumption -= EventManager_OnSwitchToConsumption;
    }

    /// <summary>
    /// check to see if we're in a channel in order to modify the background music
    /// </summary>
    /// <param name="isHome">If set to <c>true</c> is home.</param>
    void OnDrawTilesComplete(bool isHome)
    {

        if (BackgroundAudio == null)
            return;

        AudioClip newAudioClip;

        //only affect these in the home environ
        if (IsHomeEnvironment)
        {
            newAudioClip = HomeBackgroundAudio;
            if (newAudioClip != BackgroundAudio.clip)
            {
                BackgroundAudio.clip = newAudioClip;
                BackgroundAudio.Play();
                BackgroundAudio.volume = 0.0f;
                BackgroundAudio.DOFade(1.0f, 2.0f);
            }
        }
    }

    void EventManager_OnBackgroundAudioTrigger(string audioStr, bool isHome)
    {
        if (BackgroundAudio == null)
            return;

        //only play in our home environment
        if (IsHomeEnvironment)
        {
            //if we have a value for our background audio, respect it
            if (!String.IsNullOrEmpty(audioStr))
            {
                string clipName = audioStr + "BackgroundAudio";
                try
                {
                    AudioClip newAudioClip = (AudioClip)this.GetType().GetField(clipName).GetValue(this);
                    NewBackgroundAudio(newAudioClip);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Error playing background audio clip: " + ex.ToString());
                }
            }
            else
            {
                //no value set for background audio, use defaults
                NewBackgroundAudio(HomeBackgroundAudio);
            }
        }
    }

    void NewBackgroundAudio(AudioClip newAudioClip)
    {
        if (newAudioClip != null)
        {
            //only change our audio clip if it's not already playing
            if (newAudioClip != BackgroundAudio.clip)
            {
                BackgroundAudio.clip = newAudioClip;
                BackgroundAudio.Play();
                BackgroundAudio.volume = 0.0f;
                BackgroundAudio.DOFade(1.0f, 2.0f);
            }
        }
    }

    /// <summary>
    /// we are switching to consumption, fade out the background audio if available
    /// </summary>
    void EventManager_OnSwitchToConsumption()
    {
        if (BackgroundAudio == null)
            return;
        if (IsHomeEnvironment)
        {
            BackgroundAudio.DOFade(0.0f, 0.6f);
        }
    }

    private void OnApplicationQuit()
    {
        _IsApplicationQuitting = true;
    }

    private void Instance_PresenceChanged(object sender, UserPresenceEventArgs e)
    {
        if (BackgroundAudio == null)
            return;

        if (e.IsPresent)
        {
            StartCoroutine(DelayAudioStart(e.IsPresent));
        }
        else
        {
            SetBackgroundPlaying(e.IsPresent);
        }

    }

    private void VolumeController_AudioStatusChanged(object sender, AudioStatusEventArgs e)
    {
        SetBackgroundPlaying(e.IsAudioOn);
    }

    IEnumerator DelayAudioStart(bool isAudioOn)
    {
        yield return new WaitForSeconds(_UserPresentDelay);
        SetBackgroundPlaying(isAudioOn);
    }

    public void SetBackgroundPlaying(bool isAudioOn)
    {
        if (BackgroundAudio == null)
            return;

        if (isAudioOn == BackgroundAudio.isPlaying)
            return;

        if (isAudioOn)
        {
            BackgroundAudio.Play();
			Debug.Log ("play the background audio whatever it is");
        }
        else
        {
            BackgroundAudio.Pause();
			Debug.Log ("pause backgroudn audio");
        }
    }

    public void PlayAudio(AudioClips curAudioClip)
    {
        switch (curAudioClip)
        {
            case AudioClips.GenericClick:
                PlayAudioClip(GenericClickSound);
                break;
            case AudioClips.ArrowClick:
                PlayAudioClip(ArrowClickSound);
                break;
            case AudioClips.SmallClick:
                PlayAudioClip(SmallClickSound);
                break;
            case AudioClips.Notification:
                PlayAudioClip(NotificationSound);
                break;
        }

    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            if (_source != null)
            {
                _source.clip = audioClip;
                _source.PlayOneShot(audioClip);
            }
        }
    }
}
