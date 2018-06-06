using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VOKE.VokeApp.DataModel;


public class StatisticsProvider : MonoBehaviour
{
    private float _TimeSinceLastUpdate;

    public float PollingIntervalSeconds = 0;

    private string UrlFormat = "http://35.164.33.251/vokegamestore/api/1.0/games/getStatisticsByGame?game_id={0}";
    private string BaseballUrlFormat = "http://54.68.21.16/vokemlbgamestore/api/1.0/games/getMlbBoxscoreData?game_id={0}";
    private string PGAUrlFormat = "http://34.209.116.203/vokegolfgamestore/api/1.0/games/geGolfLeaderboardData?tournament_id={0}";

    private string _GameID;
    private string _StatType;

    public bool IsLive;

    private static bool _runningThread = false;
    private readonly AutoResetEvent _triggerStats = new AutoResetEvent(false);
    private readonly Queue<GameStatistics> _queue = new Queue<GameStatistics>();
    private readonly StringBuilder _wwwResponse = new StringBuilder();

    private Thread _statsWorkerThread = null;

    [HideInInspector]
    public string GameID
    {
        get { return _GameID; }
        set
        {
            if (_GameID != value)
            {
                _TimeSinceLastUpdate = PollingIntervalSeconds; // force update data on next cycle
            }

            _GameID = value;
        }
    }

    [HideInInspector]
    public string StatType
    {
        get { return _StatType; }
        set
        {
            _StatType = value;
        }
    }

    public void LogStats(GameStatistics stats)
    {
        string json = JsonUtility.ToJson(stats);
        Debug.Log(json);
    }

    public StatisticsUpdatedUnityEvent Updated;

    private IEnumerator Start()
    {
        if (Updated == null)
            Updated = new StatisticsUpdatedUnityEvent();

        VokeAppConfiguration config = null;
        yield return AppConfigurationLoader.Instance.GetDataAsync(_ => config = _);

        if (config != null)
        {
            UrlFormat = config.GetResourceValueByID("StatUrlFormat") ?? UrlFormat;
            BaseballUrlFormat = config.GetResourceValueByID("BaseballStatUrlFormat") ?? BaseballUrlFormat;
            PGAUrlFormat = config.GetResourceValueByID("PGAStatUrlFormat") ?? PGAUrlFormat;
        }
    }

    private void OnEnable()
    {
        _runningThread = true;
        _statsWorkerThread = new Thread(new ThreadStart(UpdateData));
        _statsWorkerThread.Start();
    }

    private void OnDisable()
    {
        _runningThread = false;
        _triggerStats.Set();
        _statsWorkerThread.Join();
    }

    protected virtual void OnUpdated(GameStatistics stats)
    {
        Updated.Invoke(stats);
    }

    void Update()
    { 
        if (String.IsNullOrEmpty(GameID) || PollingIntervalSeconds <= 0)
        {
            return;
        }

        _TimeSinceLastUpdate += Time.deltaTime;

        if (_TimeSinceLastUpdate >= PollingIntervalSeconds)
        {
            _TimeSinceLastUpdate = 0;
            StartCoroutine(DownloadStats());
        }
        else if (_queue.Count > 0)
        {
            // Update the stats board
            OnUpdated(_queue.Dequeue());
        }
    }

    public void UpdateData()
    {
        while (true)
        {
            _triggerStats.WaitOne();

            if (!_runningThread)
                break;

            string text = _wwwResponse.ToString();

            GameStatistics game = null;
            try
            {
                if (StatType == "PGA")
                {
                    game = new GolfGameStatistics();
                    //game = GameStatistics.PGAParse(text, _StatType);
                }
                else if (StatType == "Baseball")
                {
                    game = new BaseballGameStatistics();
                    //game = GameStatistics.Parse(text, _StatType);
                }
                else if (StatType == "Basketball")
                {
                    game = new BasketballGameStatistics();
                }

                game = game.Parse(text, _StatType);

                _queue.Enqueue(game);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Error parsing game data '{0}'", ex.ToString());
                continue;
            }
        }
    }

    IEnumerator DownloadStats()
    {
        string gameId = _GameID;
        if (String.IsNullOrEmpty(gameId))
        {
            Debug.LogError("Game ID not specified.");
            yield break;
        }

        string url;
        if (StatType == "Baseball")
        {
            url = String.Format(BaseballUrlFormat, gameId);
        }
        else if (StatType == "PGA")
        {
            url = String.Format(PGAUrlFormat, gameId);
        }
        else
        {
            url = String.Format(UrlFormat, gameId);
        }

        if (!IsLive)
        {
            if (!string.IsNullOrEmpty(GlobalVars.Instance.CurrentTimestamp))
            {
                string timeStampParam = "&utc=" + GlobalVars.Instance.CurrentTimestamp;
                url += timeStampParam;
                url = Uri.EscapeUriString(url);
                //Debug.Log("TIMESTAMP APPENDED FOR LEADERBOARD: " + url);
            }
            else
            {
#if !UNITY_EDITOR
                yield break;
#endif
            }
        }

        WWW www = new WWW(url);

        yield return www;

        //while (!www.isDone)
        //{
        //    Thread.Sleep(0);
        //}

        if (!String.IsNullOrEmpty(www.error))
        {
            Debug.LogErrorFormat("Error downloading '{0}': {1}", url, www.error);
            yield break;
        }

        _wwwResponse.Remove(0, _wwwResponse.Length);
        _wwwResponse.Append(www.text);

        // Notify to get the stats data
        _triggerStats.Set();

        yield return null;
    }
}