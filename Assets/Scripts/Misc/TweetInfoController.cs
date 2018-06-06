using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VOKE.VokeApp.Util;

public class TweetInfoController : MonoBehaviour
{
    private List<TweetInfo> _tweetInfo;
    private int _index;
    private float timePassed;
    private float timeRefresh;
    private int _tweetIndexVal = 4;
    private List<TweetInfo> _prevTweetInfo;
    private List<TweetInfo> _nextTweetInfo;
    private bool _toggleTweetToShow;

    [HideInInspector]
    public List<TweetInfo> ListOfTweetInfos;
    [HideInInspector]
    public string TwitterURL;

    public TweetData tweetData;
    public GameObject twitterCanvasObject;

    public Text tweetTxt;
    public Text tweetUserInfo;
    public Text tweetUserScreenName;

    public Text nextTweetTxt;
    public Text nextTweetUserInfo;
    public Text nextTweetUserScreenName;

    private DataCursor _cursor;
    private CanvasGroup _tweetCanvas;

    void Awake()
    {
        _tweetCanvas = twitterCanvasObject.GetComponent<CanvasGroup>();
        _tweetCanvas.alpha = 0.0f;
    }

    // Use this for initialization
    IEnumerator Start()
    {
        yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _cursor = cursor);
        TwitterURL = _cursor.CurrentVideo.TwitterURL;

        timePassed = 0.0f;
        timeRefresh = 0.0f;
        ListOfTweetInfos = new List<TweetInfo>();
        _prevTweetInfo = new List<TweetInfo>();
        _nextTweetInfo = new List<TweetInfo>();
        _toggleTweetToShow = true;
        if (!string.IsNullOrEmpty(TwitterURL))
            tweetData.TWITTER_URL = TwitterURL;

        yield return StartCoroutine(tweetData.DownloadJSONFileTweetInfo());

        HideOnDeckTweetFields();
        HideNextTweetFields();
    }

    void OnEnable()
    {
        TweetData.OnUpdateTweetFields += OnUpdateTweetFields;
    }

    void OnDisable()
    {
        TweetData.OnUpdateTweetFields -= OnUpdateTweetFields;
    }

    void Update()
    {
        //Show new tweet every 5 seconds
        if (timeRefresh > 8.0f && _tweetIndexVal >= 0)
        {
            StartCoroutine(FadeTweetCanvasIn());
            Trace.Log("REFRESH..." + _toggleTweetToShow + "....Index..." + _tweetIndexVal);
            if (_toggleTweetToShow)
            {
                HideNextTweetFields();
                ChangeOnDeckTweetFields(_tweetIndexVal);
                FadeInOnDeckTweetFields();

                _toggleTweetToShow = false;
            }
            else
            {
                HideOnDeckTweetFields();
                ChangeNextTweetFields(_tweetIndexVal);
                FadeInNextTweetFields();

                _toggleTweetToShow = true;
            }
            _tweetIndexVal--;
            timeRefresh = 0.0f;
        }
        //Pull from server every 25 seconds
        if (timePassed > 25.0f)
        {
            ListOfTweetInfos.Clear();
            _nextTweetInfo.Clear();
            StartCoroutine(tweetData.DownloadJSONFileTweetInfo());
            timePassed = 0.0f;
            _tweetIndexVal = 4;
            _toggleTweetToShow = true;
        }
        //Reset time keepers
        timePassed += Time.deltaTime;
        if (_tweetIndexVal >= 0)
            timeRefresh += Time.deltaTime;
    }

    //Subscribed event to update twitter feed
    private void OnUpdateTweetFields(List<TweetInfo> tweetInfo)
    {
        if (!tweetData.isFailedToDownload)
        {
            _tweetInfo = tweetInfo;
            if (_tweetInfo != null)
            {
                UpdateTweetContainer();
            }
        }
        else
            StartCoroutine(FadeTweetCanvasOut());
    }

    private void UpdateTweetContainer()
    {
        _index = _tweetInfo.Count;
        int i = 1;
        bool isFlag = true;
        while (i < 6)
        {
            if (_prevTweetInfo.Count > 0)
            {
                TweetInfo dummy = _tweetInfo[_index - i];
                Trace.Log("Looking for duplicate tweets");
                foreach (TweetInfo ti in _prevTweetInfo)
                {
                    if (ti.TweetID == dummy.TweetID)
                    {
                        isFlag = false;
                    }
                }
                if (isFlag)
                {
                    Trace.Log("Adding new tweet..");
                    ListOfTweetInfos.Add(_tweetInfo[_index - i]);
                    _nextTweetInfo.Add(_tweetInfo[_index - i]);
                    isFlag = true;
                }
                i++;
            }
            else
            {
                ListOfTweetInfos.Add(_tweetInfo[_index - i]);
                _nextTweetInfo.Add(_tweetInfo[_index - i]);
                i++;
            }
        }
        if (ListOfTweetInfos.Count == 0 || _nextTweetInfo.Count == 0)
        {
            Trace.Log("Count is zero...On Deck count=" + ListOfTweetInfos.Count + "....Next Tweet count=" + _nextTweetInfo.Count);
            StartCoroutine(FadeTweetCanvasOut());
        }
    }

    //Method where the UI gets updated.
    private void ChangeOnDeckTweetFields(int tweetIndex)
    {
        if (twitterCanvasObject.activeSelf)
        {
            Trace.Log("Canvas active..." + tweetIndex);
            if (ListOfTweetInfos != null && tweetIndex < ListOfTweetInfos.Count)
            {
                tweetTxt.text = string.Empty;
                tweetUserInfo.text = string.Empty;
                tweetUserScreenName.text = string.Empty;

                tweetTxt.text = ListOfTweetInfos[tweetIndex].TweetTxt;

                tweetUserInfo.text = CheckTruncateNameLength(ListOfTweetInfos[tweetIndex].TweetName, 14);
                tweetUserScreenName.text = "@" + ListOfTweetInfos[tweetIndex].TweetScreenName;

                if (!_prevTweetInfo.Contains(ListOfTweetInfos[tweetIndex]))
                {
                    _prevTweetInfo.Add(ListOfTweetInfos[tweetIndex]);
                }

                ListOfTweetInfos.RemoveAt(tweetIndex);
                _nextTweetInfo.RemoveAt(tweetIndex);
            }
        }
    }

    private void ChangeNextTweetFields(int refIndex)
    {
        int tweetIndex = refIndex - 1;
        if (twitterCanvasObject.activeSelf)
        {
            Trace.Log("Canvas active..." + tweetIndex + ".....Ref... = " + refIndex);
            if (_nextTweetInfo != null)
            {
                nextTweetTxt.text = string.Empty;
                nextTweetUserInfo.text = string.Empty;
                nextTweetUserScreenName.text = string.Empty;

                nextTweetTxt.text = _nextTweetInfo[tweetIndex].TweetTxt;

                nextTweetUserInfo.text = CheckTruncateNameLength(_nextTweetInfo[tweetIndex].TweetName, 14);
                nextTweetUserScreenName.text = "@" + _nextTweetInfo[tweetIndex].TweetScreenName;

                if (!_prevTweetInfo.Contains(_nextTweetInfo[tweetIndex]))
                {
                    _prevTweetInfo.Add(_nextTweetInfo[tweetIndex]);
                }

                _nextTweetInfo.RemoveAt(tweetIndex);
                ListOfTweetInfos.RemoveAt(tweetIndex);
            }
        }
    }

    string CheckTruncateNameLength(string userName, int nameLength)
    {
        if (userName.Length > nameLength)
        {
            string str = userName.Substring(0, nameLength - 3) + "..";
            return str;
        }
        else
            return userName;
    }

    public void ClearPrevTweets()
    {
        _prevTweetInfo.Clear();
    }

    public void HideNextTweetFields()
    {
        if (nextTweetTxt)
        {
            nextTweetTxt.CrossFadeAlpha(0.0f, 0.5f, false);
        }
        if (nextTweetUserInfo)
        {
            nextTweetUserInfo.CrossFadeAlpha(0.0f, 0.5f, false);
        }
        if (nextTweetUserScreenName)
        {
            nextTweetUserScreenName.CrossFadeAlpha(0.0f, 0.5f, false);
        }
    }

    public void FadeInNextTweetFields()
    {
        if (nextTweetTxt)
        {
            nextTweetTxt.CrossFadeAlpha(1.0f, 0.5f, false);
        }
        if (nextTweetUserInfo)
        {
            nextTweetUserInfo.CrossFadeAlpha(1.0f, 0.5f, false);
        }
        if (nextTweetUserScreenName)
        {
            nextTweetUserScreenName.CrossFadeAlpha(1.0f, 0.5f, false);
        }
    }

    public void HideOnDeckTweetFields()
    {
        if (tweetTxt)
        {
            tweetTxt.CrossFadeAlpha(0.0f, 0.5f, false);
        }
        if (tweetUserInfo)
        {
            tweetUserInfo.CrossFadeAlpha(0.0f, 0.5f, false);
        }
        if (tweetUserScreenName)
        {
            tweetUserScreenName.CrossFadeAlpha(0.0f, 0.5f, false);
        }
    }

    public void FadeInOnDeckTweetFields()
    {
        if (tweetTxt)
        {
            tweetTxt.CrossFadeAlpha(1.0f, 0.5f, false);
        }
        if (tweetUserInfo)
        {
            tweetUserInfo.CrossFadeAlpha(1.0f, 0.5f, false);
        }
        if (tweetUserScreenName)
        {
            tweetUserScreenName.CrossFadeAlpha(1.0f, 0.5f, false);
        }
    }

    IEnumerator FadeTweetCanvasOut()
    {
        float time = 0.05f;
        while (_tweetCanvas.alpha > 0)
        {
            _tweetCanvas.alpha -= Time.deltaTime / time;
            yield return null;
        }
        _tweetCanvas.alpha = 0.0f;
    }

    IEnumerator FadeTweetCanvasIn()
    {
        float time = 0.05f;
        while (_tweetCanvas.alpha < 0)
        {
            _tweetCanvas.alpha += Time.deltaTime / time;
            yield return null;
        }
        _tweetCanvas.alpha = 1.0f;
    }
}