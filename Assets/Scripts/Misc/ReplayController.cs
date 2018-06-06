using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine;
using DG.Tweening;

public class ReplayController : MonoBehaviour 
{
	public delegate void OnReplayButtonClickedAction(object sender);
	public event OnReplayButtonClickedAction OnReplayButtonClickedEvent;
	public delegate void OnCloseButtonClickedAction(object sender);
	public event OnCloseButtonClickedAction OnCloseButtonClickedEvent;

	public ButtonController ReplayButton;
	public ButtonController CloseButton;
	public ButtonController SkipBackButton;
	public ButtonController SkipForwardButton;
	public ContentTileController ReplayContentTile;
	public GameObject BtnReplayContainer;
	public GameObject Countdown;
	public GameObject ReplayNextContainer;

	private DataCursor _cursor;
	private CanvasGroup _replayParentCanvasGroup;
	private CanvasGroup _countdownCanvasGroup;
	private List<Image> _countdownImageList = new List<Image>();
	private int _currentItemsIndex = 0;
	private Coroutine _countdownCoroutine;
	private Interactible _contentTileImageInteractible;
	private List<ContentViewModel> _CurrentItems;

	void Awake()
	{
		HideReplayContainer();
		_replayParentCanvasGroup = transform.GetComponent<CanvasGroup>();
		_countdownImageList = Countdown.GetComponentsInChildren<Image>().ToList();
		_replayParentCanvasGroup.alpha = 0.0f;
		_countdownCanvasGroup = Countdown.transform.GetComponent<CanvasGroup>();
		_contentTileImageInteractible = ReplayContentTile.ContentContainer.GetComponent<Interactible>();
		transform.gameObject.SetActive(false);
		DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
		ReplayContentTile.SetTileFade(true);
	}

	void OnEnable()
	{
		_contentTileImageInteractible.OnOver += CancelCountdown;
		CloseButton.OnButtonClicked += OnCloseButtonClicked;
		CloseButton.OnButtonOver += CancelCountdown;
		ReplayButton.OnButtonClicked += OnReplayButtonClicked;
		ReplayButton.OnButtonOver += CancelCountdown;
		SkipBackButton.OnButtonClicked += OnSkipBackButtonClicked;
		SkipBackButton.OnButtonOver += CancelCountdown;
		SkipForwardButton.OnButtonClicked += OnSkipForwardButtonClicked;
		SkipForwardButton.OnButtonOver += CancelCountdown;
	}

    public void ShowReplayContainer()
    {
        gameObject.SetActive(true);
        ReplayContentTile.gameObject.SetActive(true);
        SkipForwardButton.gameObject.SetActive(false);
        SkipBackButton.gameObject.SetActive(false);
        BtnReplayContainer.SetActive(true);

        _CurrentItems = new List<ContentViewModel>(_cursor.CurrentItems);

        for (int i = _CurrentItems.Count - 1; i >= 0; i--)
        {
            if (_CurrentItems[i].Type == ContentType.Channel || _CurrentItems[i].IsUpcoming)
                _CurrentItems.RemoveAt(i);
        }

        for (int i = _CurrentItems.Count - 1; i >= 0; i--)
        {
            if (_CurrentItems[i] == _cursor.CurrentVideo)
            {
                _CurrentItems.RemoveAt(i);
                _currentItemsIndex = i;
                if (_currentItemsIndex >= _CurrentItems.Count)
                    _currentItemsIndex = 0;
            }
        }

        if (_CurrentItems.Count >= 1)
        {
            SetupReplayTile(_CurrentItems[_currentItemsIndex]);
        }
        else
        {
            SetupReplayTile(_cursor.CurrentVideo.GetWatchNextVideo());
        }

        _replayParentCanvasGroup.DOFade(1.0f, 0.3f).OnComplete(() => {
            _countdownCanvasGroup.DOFade(1.0f, 0.3f);
        });
    }

    private void SetupReplayTile(ContentViewModel target)
    {
        ReplayNextContainer.SetActive(true);
        ReplayContentTile.Init(target, _cursor, true);
        Countdown.SetActive(true);
        _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        _countdownImageList[_countdownImageList.Count - 1].DOFade(1.0f, 0.3f);

        if (_currentItemsIndex + 1 >= _CurrentItems.Count)
            SkipForwardButton.gameObject.SetActive(false);
        else
            SkipForwardButton.gameObject.SetActive(true);

        if (_currentItemsIndex <= 0)
            SkipBackButton.gameObject.SetActive(false);
        else
            SkipBackButton.gameObject.SetActive(true);
    }

    public void HideReplayContainer()
	{
		_replayParentCanvasGroup.DOFade(0.0f, 0.3f).OnComplete(ResetButtons);
	}

	void RedrawReplayContainer()
	{
		ContentViewModel newContentViewModel = _CurrentItems[_currentItemsIndex];
		ReplayContentTile.Init(newContentViewModel, _cursor, true);

		if(_currentItemsIndex + 1 >= _CurrentItems.Count)
			SkipForwardButton.gameObject.SetActive(false);
		else
			SkipForwardButton.gameObject.SetActive(true);

		if(_currentItemsIndex <= 0)
			SkipBackButton.gameObject.SetActive(false);
		else
			SkipBackButton.gameObject.SetActive(true);
	}

	void ResetButtons()
	{
		gameObject.SetActive(false);
		BtnReplayContainer.SetActive(false);
		ReplayContentTile.gameObject.SetActive(false);
		CloseButton.gameObject.SetActive(false);
		SkipBackButton.gameObject.SetActive(false);
		SkipForwardButton.gameObject.SetActive(false);
	}

	IEnumerator CountdownCoroutine()
	{
		yield return new WaitForSeconds(0.6f);
		int numOfItems = _countdownImageList.Count;
		yield return new WaitForSeconds(1.0f);
		for(int i = numOfItems - 1; i >= 1; i--)
		{
			if(i == 1)
			{
				_countdownCanvasGroup.DOFade(0.0f, 0.3f);
				_countdownImageList[i].DOFade(0.0f, 0.3f);
				_replayParentCanvasGroup.DOFade(0.0f, 0.3f).SetDelay(0.4f).OnComplete(() => {
					ReplayContentTile.DoMove();
				});
			}
			else
			{
				_countdownImageList[i].DOFade(0.0f, 0.3f);
				_countdownImageList[i - 1].DOFade(1.0f, 0.3f);
			}

			yield return new WaitForSeconds(1.0f);
		}
		yield return null;
	}

	void OnCloseButtonClicked(object sender)
	{
		if(OnCloseButtonClickedEvent != null)
			OnCloseButtonClickedEvent(sender);
		HideReplayContainer();
	}

	void OnReplayButtonClicked(object sender)
	{
		HideReplayContainer();
		#region Analytics Call
		Analytics.CustomEvent("ReplayButtonClicked", new Dictionary<string, object>
		{
			{ "Operation", "Replay" }
		});
		#endregion
		if(OnReplayButtonClickedEvent != null)
			OnReplayButtonClickedEvent(sender);
	}

	void OnSkipBackButtonClicked(object sender)
	{
		_currentItemsIndex--;
		RedrawReplayContainer();
	}

	void OnSkipForwardButtonClicked(object sender)
	{
		_currentItemsIndex++;
		RedrawReplayContainer();
	}

	void CancelCountdown(object sender)
	{
		if(_countdownCoroutine != null)
			StopCoroutine(_countdownCoroutine);
		_countdownCanvasGroup.DOFade(0.0f, 0.3f);
		for(int i = 0; i < _countdownImageList.Count; i++)
			_countdownImageList[i].DOFade(0.0f, 0.0f);
	}

	void CancelCountdown()
	{
		if(_countdownCoroutine != null)
			StopCoroutine(_countdownCoroutine);
		_countdownCanvasGroup.DOFade(0.0f, 0.3f);
		for(int i = 0; i < _countdownImageList.Count; i++)
			_countdownImageList[i].DOFade(0.0f, 0.0f);
	}

	void OnDisable()
	{
		_contentTileImageInteractible.OnOver -= CancelCountdown;
		CloseButton.OnButtonClicked -= OnCloseButtonClicked;
		CloseButton.OnButtonOver -= CancelCountdown;
		ReplayButton.OnButtonClicked -= OnReplayButtonClicked;
		ReplayButton.OnButtonOver -= CancelCountdown;
		SkipBackButton.OnButtonClicked -= OnSkipBackButtonClicked;
		SkipBackButton.OnButtonOver -= CancelCountdown;
		SkipForwardButton.OnButtonClicked -= OnSkipForwardButtonClicked;
		SkipForwardButton.OnButtonOver -= CancelCountdown;
	}
}