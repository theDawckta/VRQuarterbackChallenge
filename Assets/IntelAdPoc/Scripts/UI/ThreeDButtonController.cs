using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThreeDButtonController : MonoBehaviour 
{
	public delegate void OnButtonClickedEvent();
	public event OnButtonClickedEvent OnButtonClicked;

	public SpriteRenderer GroundText;
	public GameObject Model;
	public GameObject GroundEffectHolder;
	public ParticleSystem SpikesSmall;

	private Interactible _interactible;
	private ParticleSystem[] _particles;
	private bool _buttonLocked = false;
	private Vector3 _groundTextOriginalSize;
	private Vector3 _groundTextOriginalLocation;
	private Vector3 _modelOriginalSize;
	private AudioSource _audio;
    
	void Awake () 
	{
		_audio = gameObject.GetComponentInChildren<AudioSource>();
		_interactible = gameObject.GetComponentInChildren<Interactible>();
		_interactible.GazeSelectable = false;
		_particles = GroundEffectHolder.GetComponentsInChildren<ParticleSystem>();
		_groundTextOriginalSize = GroundText.transform.localScale;
		_modelOriginalSize = Model.transform.localScale;
		_groundTextOriginalLocation = GroundText.transform.localPosition;
		GroundText.transform.localScale = Vector3.zero;
		Model.transform.localScale = Vector2.zero;
		Model.transform.DORotate(new Vector3(0.0f, -360.0f, 0.0f), 12.0f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
	}
    
    public void ShowButton(float delay = 0.0f)
	{
		gameObject.SetActive(true);
		SpikesSmall.Play();
		Model.transform.DORotate(new Vector3(0.0f, -360.0f, 0.0f), 12.0f, RotateMode.WorldAxisAdd).SetDelay(delay).SetEase(Ease.Linear).SetLoops(-1);
		Model.transform.DOLocalMove(new Vector3(0.0f, 1.0f, 0.0f), 0.5f).SetDelay(delay);
		Model.transform.DOScale(_modelOriginalSize, 0.5f).SetEase(Ease.OutQuart).SetDelay(delay);
		GroundText.transform.DOScale(_groundTextOriginalSize, 0.5f).SetEase(Ease.OutQuart).SetDelay(delay).OnComplete(() => {
            _buttonLocked = false;
        });
	}
    
	public void HideButton()
    {
		if (gameObject.activeSelf)
		{
			_buttonLocked = true;
			for (int i = 0; i < _particles.Length; i++)
				_particles[i].Stop();
			SpikesSmall.Stop();

			Model.transform.DOLocalMove(new Vector3(0.0f, 0.7f, 0.0f), 0.5f).SetEase(Ease.InQuart);
			Model.transform.DOScale(0.0f, 0.5f).SetEase(Ease.InQuart);
			GroundText.transform.DOLocalMove(new Vector3(0.0f, 0.7f, 0.0f), 0.5f).SetEase(Ease.InQuart).SetDelay(0.3f);
			GroundText.transform.DOScale(0.0f, 0.5f).SetDelay(0.3f).SetEase(Ease.InQuart).OnComplete(() =>
			{
				StartCoroutine(WaitForParticles());
				GroundText.transform.localPosition = _groundTextOriginalLocation;
			});
		}
    }

	IEnumerator WaitForParticles()
	{
		yield return new WaitForSeconds(1.0f);
		gameObject.SetActive(false);
	}
    
	void ButtonClicked()
	{
		if(!_buttonLocked)
		{
			_buttonLocked = true;
			_audio.Play();
            Model.transform.DOLocalMove(new Vector3(0.0f, 0.8f, 0.0f), 0.1f).SetEase(Ease.OutQuart).OnComplete(() => {
				Model.transform.DOLocalMove(new Vector3(0.0f, 1.0f, 0.0f), 0.2f).SetEase(Ease.OutBack).OnComplete(() => {
					_buttonLocked = false;
					if (OnButtonClicked != null)
					{
						HideButton();
						OnButtonClicked();
					}
				});
            });
		}      
	}

    void ButtonOver()
    {
		if(_buttonLocked != true)
		{
			for (int i = 0; i < _particles.Length; i++)
             _particles[i].Play();

            Model.transform.DORotate(new Vector3(0.0f, -22.5f, 0.0f), 0.5f, RotateMode.WorldAxisAdd).SetEase(Ease.InQuad).OnComplete(() => {
                Model.transform.DORotate(new Vector3(0.0f, -360.0f, 0.0f), 4.0f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
            });

			GroundText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -22.5f), 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad).OnComplete(() => {
				GroundText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -360.0f), 4.0f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
            });      
		}      
    }

	void ButtonOut()
	{
		if (_buttonLocked != true)
		{
			for (int i = 0; i < _particles.Length; i++)
				_particles[i].Stop();

			DOTween.Kill(GroundText.transform);
			DOTween.Kill(Model.transform);

		    Model.transform.DORotate(new Vector3(0.0f, -45.0f, 0.0f), 0.5f, RotateMode.WorldAxisAdd).SetEase(Ease.OutQuad);
			GroundText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -22.5f), 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);
			Model.transform.DORotate(new Vector3(0.0f, -360.0f, 0.0f), 12.0f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
	    }
	}
    
	void OnEnable()
	{
		_interactible.OnClick += ButtonClicked;
		_interactible.OnOver += ButtonOver;
		_interactible.OnOut += ButtonOut;
	}

	void OnDisable()
	{
		_interactible.OnClick -= ButtonClicked;
        _interactible.OnOver -= ButtonOver;
        _interactible.OnOut -= ButtonOut;
		DOTween.Kill(GroundText.transform, true);
        DOTween.Kill(Model.transform, true);
	}
}