using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GameAudioController : MonoBehaviour
{
    public AudioSource UIAudioSource;
    public AudioSource BackgroundAudioSource;
	public AudioSource EffectsAudioSource;
	public AudioSource CrowdLast10Seconds;
    public AudioClip StartGameBackgroundAudio;
    public AudioClip EndGameBackgroundAudio;
    public AudioClip CrowdNoiseBackgroundAudio;
	public AudioClip ThrowSoundAudio;   
	public AudioClip ThrowSoundAudioCan;
	public AudioClip EndGameHornAudio;  
    
    private VolumeController _VolumeController;
    private float _UserPresentDelay = 1.0f;
    private bool _IsApplicationQuitting;

    void Awake()
    {
        _VolumeController = Extensions.GetRequired<VolumeController>();

		BackgroundAudioSource.clip = CrowdNoiseBackgroundAudio;
		BackgroundAudioSource.volume = 0.0f;
		BackgroundAudioSource.Play();
		BackgroundAudioSource.DOFade(0.5f, 2.0f);

		UIAudioSource.clip = StartGameBackgroundAudio;
        UIAudioSource.Play();
    }
   
	public void PlayEndMusic()
    {
		BackgroundAudioSource.DOFade(0.5f, 2.0f);
		UIAudioSource.clip = EndGameBackgroundAudio;
		UIAudioSource.volume = 1.0f;
        UIAudioSource.Play();
    }

    public void StopUIAudioSource()
    {
		BackgroundAudioSource.DOFade(1.0f, 1.0f);
        UIAudioSource.DOFade(0.0f, 3.0f).OnComplete(() => {
            UIAudioSource.Stop();
            UIAudioSource.clip = null;
			UIAudioSource.volume = 0.0f;
        });
    }

    public void PlayThrowSound()
	{
		EffectsAudioSource.PlayOneShot(ThrowSoundAudio);
	}

	public void PlayOpenCanSound()
    {
        EffectsAudioSource.PlayOneShot(ThrowSoundAudioCan);
    }
    
    public void PlayLast10Seconds()
	{
		CrowdLast10Seconds.volume = 1.0f;
		CrowdLast10Seconds.Play();
	}

	public void StopLast10Seconds()
    {
		CrowdLast10Seconds.DOFade(0.0f, 1.0f).OnComplete(() => {
			CrowdLast10Seconds.Stop();
		});
    }

    public void PlayEndHorn()
	{
		PlayAudioClip(EndGameHornAudio);
	}

    private void Instance_PresenceChanged(object sender, UserPresenceEventArgs e)
    {      
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
		if (isAudioOn == BackgroundAudioSource.isPlaying)
            return;

        if (isAudioOn)
        {
			BackgroundAudioSource.Play();
            Debug.Log("play the background audio whatever it is");
        }
        else
        {
			BackgroundAudioSource.Pause();
            Debug.Log("pause backgroudn audio");
        }
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
		EffectsAudioSource.clip = audioClip;
		EffectsAudioSource.PlayOneShot(audioClip);
    }

	void OnEnable()
    {
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
    }
}
