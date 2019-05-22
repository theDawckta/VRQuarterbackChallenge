using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CountdownController : MonoBehaviour 
{
	public delegate void OnCountdownOverEvent();
	public event OnCountdownOverEvent OnCountdownOver;

	public TextMeshPro Down;
	public TextMeshPro Set;
	public TextMeshPro Hike;
	public GameAudioController GameAudio;
	public AudioClip down;
	public AudioClip set;
	public AudioClip hike;
    
	private bool _downDone = false;
	private bool _setDone = false;
	private bool _hikeDone = false;
   
	public void StartGameCoundown()
	{
		Down.DOFade(0.0f, 0.0f);
        Down.transform.DOScale(0.7f, 0.0f);
        Set.DOFade(0.0f, 0.0f);
        Set.transform.DOScale(0.7f, 0.0f);
        Hike.DOFade(0.0f, 0.0f);
		Hike.transform.DOScale(0.7f, 0.0f).OnComplete(() => {
			gameObject.SetActive(true);
            StartCoroutine(StartCountdown());
		});      
	}
    
	IEnumerator StartCountdown()
	{
		yield return new WaitForSeconds(1.5f);
		float time = 4.0f;

		ShowNumber(Down, down);

		while (time > 0.0f && (!_downDone || !_setDone || !_hikeDone))
		{
			time -= Time.deltaTime;         
			if(time < 2.5f && !_downDone)
			{
				_downDone = true;
				HideNumber(Down);
				ShowNumber(Set, set);
			}
			else if(time < 1.0f && !_setDone)
			{
				_setDone = true;
                HideNumber(Set);
                ShowNumber(Hike, hike);
			}
			else if (time < 0.0f && !_hikeDone)
            {
                _hikeDone = true;
                HideNumber(Hike, true);
            }
            
			yield return null;
		}

		OnCountdownOver();

		_downDone = false;
        _setDone = false;
        _hikeDone = false;

		yield return null;
	}

	void ShowNumber(TextMeshPro number, AudioClip audioStr)
	{
		GameAudio.PlayAudioClip(audioStr);
		number.DOFade(1.0f, 0.1f);
		number.transform.DOScale(1.0f, 0.3f);
	}

	void HideNumber(TextMeshPro number, bool last = false)
    {
        number.DOFade(0.0f, 0.3f);
		number.transform.DOScale(0.7f, 0.3f).OnComplete(() => {
			if (last == true)
			    gameObject.SetActive(false);
		});
    }
}
