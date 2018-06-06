using UnityEngine;
using System.Collections;

public class StatsController : MonoBehaviour
{
    public GameObject StatHolder;
    public BasketballStatsController basketballStatsController;

    [HideInInspector]
    public GameStatus _curGameStatus;
    [HideInInspector]
    public GameStatistics _curGameStatistics;
    [HideInInspector]
    public bool _initStatLoad = true;
    [HideInInspector]
    public bool _initBtnLoad = true;
    public string awayTeamCode;
    [HideInInspector]
    public string homeTeamCode;

    public GameObject snapButton;
    public StatisticsProvider Provider;
    [HideInInspector]

    public GameObject statsUnderlay;

    private DataCursor _cursor;
    private StatisticsUpdatedUnityEvent _statsUpdated;
    private GameStatistics _gameStatistics;

    void Start()
    {
        if (StatHolder != null)
            StatHolder.SetActive(false);
        GlobalVars.Instance.IsAllowControlsOpen = true;
        EventManager.Instance.PanelCloseEvent();
    }

    public void InitStats(DataCursor cursor)
    {
        _cursor = cursor;
        if (_cursor.CurrentVideo.GameID != null)
        {
            if (_cursor.CurrentVideo.StatType != null)
            {
                Provider.StatType = _cursor.CurrentVideo.StatType;
            }

            Provider.GameID = _cursor.CurrentVideo.GameID;

            StatHolder.SetActive(true);
            //Provider.UpdateDataAsync(Provider.GameID, MakeStatPanels);

            //Poll more frequently for VOD
            if (!_cursor.CurrentVideo.IsLive)
            {
                Provider.PollingIntervalSeconds = 90f;
                Provider.IsLive = false;
            }
            else
            {
                Provider.PollingIntervalSeconds = 90f;
                Provider.IsLive = true;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void StatsUpdated(GameStatistics stats)
    {
        _curGameStatistics = stats;
        _curGameStatus = stats.Status;


        switch (Provider.StatType)
        {
            case "Basketball":
                CreateBasketballStats(stats);
                //CreateStats(stats, StatHolder.transform, basketballStatsController);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Instantiates basketball stats
    /// </summary>
    public void CreateBasketballStats(GameStatistics stats)
    {
        if (_initStatLoad)
        {
            _initStatLoad = false;
            basketballStatsController = Instantiate(basketballStatsController) as BasketballStatsController;
            basketballStatsController.transform.SetParent(StatHolder.transform, true);
        }

        basketballStatsController.InitializeStats(this, stats);
    }

    IEnumerator ShowButtons()
    {
        yield return new WaitForSeconds(1.0f);
        yield return null;
    }

    /// <summary>
    /// Shows/Hides the Stats Underlay object
    /// </summary>
    /// <param name="show">whether to show the underlay or not</param>
    public void SetStatsUnderlay(bool show)
    {
        if (statsUnderlay != null)
            statsUnderlay.SetActive(show);
    }

    /// <summary>
    /// Stores the appropriate team codes so that respective stats controllers for the sports can use it once they are instantiated
    /// </summary>
    /// <param name="isHome">Home team or not</param>
    /// <param name="teamCode">Team code/abbreviation string</param>
    public void SetTeamCodes(bool isHome, string teamCode)
    {
        if (isHome)
            homeTeamCode = teamCode;
        else
            awayTeamCode = teamCode;
    }

    private void OnEnable()
    {
        EventManager.OnTeamNameReceived += SetTeamCodes;
    }

    private void OnDisable()
    {
        EventManager.OnTeamNameReceived -= SetTeamCodes;
    }

}
