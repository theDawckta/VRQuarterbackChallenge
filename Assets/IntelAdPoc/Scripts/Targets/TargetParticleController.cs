using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class TargetParticleController : MonoBehaviour {

	public TextMeshProUGUI ScoreText;
	public TextMeshProUGUI MultipleText;
	public List<AudioClip> CollisionAudio;

	private int _streak;
    private Vector3 _endTextSize = Vector3.one;
    private float _textEndPosY;
	private float _multEndPosY;
	private AudioSource _audioSource;

	void Awake()
	{
		if (ScoreText == null)
			throw new Exception ("A ScoreText Gameobject must be defined in TargetParticleController");
		if (MultipleText == null)
			throw new Exception ("A MultipleText Gameobject must be defined in TargetParticleController");
			
		_textEndPosY = ScoreText.transform.localPosition.y;
		_multEndPosY = MultipleText.transform.localPosition.y;
		ScoreText.transform.localPosition = new Vector3 (ScoreText.transform.localPosition.x, ScoreText.transform.localPosition.y - 50, ScoreText.transform.localPosition.z);
		ScoreText.transform.localScale = Vector3.zero;
		MultipleText.transform.localPosition = new Vector3(MultipleText.transform.localPosition.x, MultipleText.transform.localPosition.y - 50, MultipleText.transform.localPosition.z);
		MultipleText.transform.localScale = Vector3.zero;

		if (this.gameObject.GetComponent<AudioSource> () != null)
		{
			_audioSource = this.gameObject.GetComponent<AudioSource> ();
		} else
		{
			_audioSource = this.gameObject.AddComponent<AudioSource> ();
		}

		PlayCollisionAudio ();
	}

	public void SetText(string txtStr, int streak)
	{      
		_streak = streak;
		ScoreText.text = txtStr;
		MultipleText.text = "x" + _streak.ToString();
		AnimIn ();
	}

	void AnimIn()
	{
		ScoreText.transform.LookAt(2.0f * transform.position - Camera.main.transform.position);
		MultipleText.transform.LookAt(2.0f * transform.position - Camera.main.transform.position);
        
		ScoreText.transform.DOScale(_endTextSize, 0.2f).SetEase(Ease.Linear);
		ScoreText.transform.DOLocalMoveY (_textEndPosY, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f).OnComplete(() =>
		{
			ScoreText.transform.DOLocalMoveY(_textEndPosY + 75.0f, 0.2f);
			ScoreText.DOFade(0.0f, 0.2f).OnComplete(() => {
				if (_streak == 1)
					DestroyTarget();
			});
		});      

		if (_streak > 1)
        {
            StartCoroutine(AnimMultipleIn());
        }
	}   
    
	IEnumerator AnimMultipleIn()
	{
		yield return new WaitForSeconds (0.3f);
		MultipleText.transform.DOScale(_endTextSize, 0.2f).SetEase(Ease.Linear);
		MultipleText.transform.DOLocalMoveY (_multEndPosY, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f).OnComplete(() =>
		{
			MultipleText.transform.DOLocalMoveY(_textEndPosY + 75.0f, 0.2f);
			MultipleText.DOFade(0.0f, 0.2f).OnComplete(() => {
				DestroyTarget();
			});
        });   
	}

	void PlayCollisionAudio()
    {
        if (CollisionAudio.Count > 0)
        {
            int ranInt = UnityEngine.Random.Range(0, CollisionAudio.Count);
            AudioClip ranAudioClip = CollisionAudio[ranInt];
            if (ranAudioClip != null)
            {
                if (_audioSource != null)
                {
                    _audioSource.PlayOneShot(ranAudioClip);
                }
            }
        }
    }

	void DestroyTarget()
	{
		DOTween.Kill(ScoreText.transform);
		DOTween.Kill(MultipleText.transform);
		Destroy (this.gameObject);
	}
}
