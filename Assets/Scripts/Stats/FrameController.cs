using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class FrameController : MonoBehaviour
{
    public GameObject Frame;
    public GameObject NamePlatePanel;
    public GameObject BackPanel;
    public GameObject BackPanelPattern;
    public GameObject ContentHolder;
    public Color BackPanelColor;
    public Color BackPanelPatternColor;
    public bool Animating = false;
    public bool FrameOpen = false;
	public GameObject FrameHolder;

    private CanvasGroup _namePlatePanelCanvasGroup;
    private CanvasGroup _contentHolderCanvasGroup;
    private RectTransform _frameRectTransform;
    private CanvasGroup _frameCanvasGroup;
    private ContentSizeFitter _frameContentSizeFitter;
    private Vector2 _originalSize = new Vector2();
    private Vector2 _expandedSize = new Vector2();
	private BoxCollider _boxCollider;
	private float _animStatsFrameSpeed = 0.35f;
	private float _delayAnimFrame = 0.2f;

    void Awake()
    {
        _namePlatePanelCanvasGroup = NamePlatePanel.GetComponent<CanvasGroup>();
        _contentHolderCanvasGroup = ContentHolder.GetComponent<CanvasGroup>();
        _frameRectTransform = Frame.GetComponent<RectTransform>();
        _frameCanvasGroup = Frame.GetComponent<CanvasGroup>();
        _frameContentSizeFitter = Frame.GetComponent<ContentSizeFitter>();
        _originalSize = _frameRectTransform.sizeDelta;
        _frameContentSizeFitter.enabled = true;

		//TODO
		_contentHolderCanvasGroup.alpha = 0.0f;
		_frameCanvasGroup.alpha = 0.0f;

		if (FrameHolder != null)
		{
			_boxCollider = FrameHolder.GetComponent<BoxCollider> ();
		}

		SetColliderSize (false);
    }

    void Start()
    {
		//TODO
        //_frameCanvasGroup.alpha = 0;
    }

	public void StartInitFrameController()
    {
        StartCoroutine(InitFrameController());
    }

	IEnumerator InitFrameController(bool setColliderSize = true)
    {
        yield return new WaitForEndOfFrame();

		//set to whatever size our contents are
		ContentHolder.transform.SetParent(Frame.transform);
		_frameContentSizeFitter.enabled = true;

		//if(_expandedSize == Vector2.zero)
		_expandedSize = _frameRectTransform.sizeDelta;

		yield return new WaitForEndOfFrame();
		//now unparent and turn off sizing for perf
		ContentHolder.transform.SetParent(transform);
		_frameContentSizeFitter.enabled = false;

        //Resetting z position to 0 as this incurs more draw calls if its non zero
        ContentHolder.GetComponent<RectTransform>().localPosition = new Vector3(ContentHolder.GetComponent<RectTransform>().localPosition.x, ContentHolder.GetComponent<RectTransform>().localPosition.y, 0f);
       
        SetColliderSize (setColliderSize);

        ShowContents();
    }

	//our box 
	void SetColliderSize(bool enableBox = true)
	{
		if (_boxCollider != null)
		{
			_boxCollider.size = new Vector3 (_frameRectTransform.sizeDelta.x, _frameRectTransform.sizeDelta.y, _boxCollider.size.z);
			if (!enableBox)
			{
				_boxCollider.enabled = false;
			}
		}
	}

    /// <summary>
    /// Fades out the Contents inside the Frame
    /// </summary>
    public void HideContents()
    {
        _contentHolderCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fades In the Contents inside the Frame
    /// </summary>
    public void ShowContents()
    {
        _contentHolderCanvasGroup.alpha = 1f;
    }

    public void HideStatPanel()
    {
    	if(!Animating)
    	{
	        Animating = true;
			_contentHolderCanvasGroup.DOFade(0.0f, 0.15f);
			_namePlatePanelCanvasGroup.DOFade(1.0f, 0.15f).OnComplete(HideStatsPanelComplete);
        }
    }

    void HideStatsPanelComplete()
    {
		_frameCanvasGroup.DOFade(0.0f, _animStatsFrameSpeed).SetDelay(_delayAnimFrame);
		_frameRectTransform.DOSizeDelta(new Vector2(_originalSize.x, _frameRectTransform.sizeDelta.y),_animStatsFrameSpeed).SetDelay(_delayAnimFrame).SetEase(Ease.InQuad);
		_frameRectTransform.DOSizeDelta(new Vector2(_frameRectTransform.sizeDelta.x, _originalSize.y), _animStatsFrameSpeed).SetEase(Ease.InQuad).OnComplete(AnimationDone);

		if(_boxCollider != null)
			_boxCollider.enabled = false;

		StartCoroutine(InitFrameController(false));
    }

    public void ShowStatPanel()
    {
    	if(!Animating)
    	{
	        Animating = true;
			StartCoroutine(InitFrameController(true));
			_frameCanvasGroup.DOFade(1.0f, _animStatsFrameSpeed).SetDelay(_delayAnimFrame);
			_frameRectTransform.DOSizeDelta(new Vector2(_expandedSize.x, _frameRectTransform.sizeDelta.y), _animStatsFrameSpeed).SetDelay(_delayAnimFrame).SetEase(Ease.OutQuad);
			_frameRectTransform.DOSizeDelta(new Vector2(_frameRectTransform.sizeDelta.x, _expandedSize.y), _animStatsFrameSpeed).SetDelay(_delayAnimFrame).SetEase(Ease.OutQuad).OnComplete(ShowStatsPanelComplete);
        }
    }

    void ShowStatsPanelComplete()
    {
		StartCoroutine(InitFrameController(true));
		_namePlatePanelCanvasGroup.DOFade(0.0f, 0.15f);
		_contentHolderCanvasGroup.DOFade(1.0f, 0.15f).OnComplete(AnimationDone);

		if(_boxCollider != null)
			_boxCollider.enabled = true;
    }

    void AnimationDone()
    {
        Animating = false;
        FrameOpen = !FrameOpen;
    }
}