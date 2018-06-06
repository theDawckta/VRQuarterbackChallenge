using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BasketballStatsController : MonoBehaviour {

    public FrameController Left;
    public FrameController Center;
    public FrameController Right;

    public Transform LeftFrameTargetAnchor;
    public Transform RightFrameTargetAnchor;
    public Transform CenterFrameTargetAnchor;

    public GridStats Team1StatGrid;
    public GridStats Team2StatGrid;
    public InningStats InningStatsPanel;
    public GridStats GridStatsPrefab;
    public GridStats GridStatsPrefab_5;
    public GridStats GridStatsPrefab_9;
    public GridStats GridStatsPrefab_15;
    public GridStats GridStatsPrefab_20;
    public GameObject GridStatsContainerLeft;
    public GameObject GridStatsContainerRight;
    public FrameController ParentFrameLeft;
    public FrameController ParentFrameRight;
    public TeamStats TeamsStatPanel;

    private GridStats leftGrid, rightGrid;
    private StatsController statsController;
    private UISnapper snapperComponent;
    private int previousPlayersCountLeft;
    private int previousPlayersCountRight;
    protected bool firstLoad = true;

    /// <summary>
    /// Initializes any necessary components before rendering Stats
    /// </summary>
    public void InitializeStats(StatsController controller, GameStatistics basketballStats)
    {        
        if (statsController == null)
        {
            statsController = controller;
            snapperComponent = statsController.snapButton.GetComponent<UISnapper>();
            snapperComponent.AddSnapObject(Left.transform, LeftFrameTargetAnchor, true);
            snapperComponent.AddSnapObject(Right.transform, RightFrameTargetAnchor, true);
            snapperComponent.AddSnapObject(Center.transform, CenterFrameTargetAnchor, true);
        }

        RenderStats(basketballStats);
    }

    /// <summary>
    /// Renders the basketball stats based on the data received
    /// </summary>
    public void RenderStats(GameStatistics basketballStats)
    {
        //Debug.Log("*********************************** Basketball Stats Controller Render Stats ***********************************");

        BasketballGameStatistics bbGameStats = (BasketballGameStatistics)basketballStats;

        if (firstLoad)
        {
            MakeStatPanels(bbGameStats);
        }
        else
        {
            if (bbGameStats.Teams.Count > 1)
            {
                CreateDynamicTeamPanels(true, bbGameStats);
                CreateDynamicTeamPanels(false, bbGameStats);
            }

            ShowHideCenterFrame(true);
            TeamsStatPanel.UpdateTeamStats(basketballStats);
        }
    }

    public void MakeStatPanels(BasketballGameStatistics stats)
    {
        if ((stats.Status == GameStatus.InProgress || stats.Status == GameStatus.PreGame || stats.Status == GameStatus.Complete || stats.Status == GameStatus.Final || stats.Status == GameStatus.Closed || stats.Status == GameStatus.Halftime) && firstLoad && stats.Teams.Count > 1)
        {
            firstLoad = false;
            if (statsController._initBtnLoad)
            {
                statsController._initBtnLoad = false;
                OpenButtonClicked(null);
                statsController.snapButton.SetActive(true);
            }

            CreateDynamicTeamPanels(true, stats);
            CreateDynamicTeamPanels(false, stats);

            ShowHideCenterFrame(true);
            TeamsStatPanel.StartMakeTeamStats(stats);
        }
    }

    public void CreateDynamicTeamPanels(bool isLeftPanel, BasketballGameStatistics stats)
    {
        //PS DEBUG:
        //Note the panel data has been reversed. The data is obtained as Home and Away rather than away and home
        //TODO: Test switched panel information
        //Currently reversed index for StartMakeStatsGrid
        int lIndex = 0;
        int rIndex = 0;
        if (stats.Teams[0].Location.Contains("home"))
        {
            lIndex = 1;
            rIndex = 0;
        }
        else
        {
            lIndex = 0;
            rIndex = 1;
        }

        if (isLeftPanel)
            CreateLeftDynamicTeamPanels(stats, lIndex);
        else
            CreateRightDynamicTeamPanels(stats, rIndex);
    }

    void CreateLeftDynamicTeamPanels(BasketballGameStatistics stats, int index)
    {
        bool reDraw = false;

        int rowCount = stats.Teams[index].Players.Count;
        if (previousPlayersCountLeft != rowCount)
            reDraw = true;

        //GridStats leftGrid;

        if (reDraw)
        {
            //clear out the old panels
            foreach (Transform child in GridStatsContainerLeft.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            if (rowCount <= 5)
                leftGrid = Instantiate(GridStatsPrefab_5) as GridStats;
            else if (rowCount <= 9)
                leftGrid = Instantiate(GridStatsPrefab_9) as GridStats;
            else if (rowCount <= 15)
                leftGrid = Instantiate(GridStatsPrefab_15) as GridStats;
            else
                leftGrid = Instantiate(GridStatsPrefab_20) as GridStats;

            leftGrid.ParentFrameController = ParentFrameLeft;
            leftGrid.transform.SetParent(GridStatsContainerLeft.transform, false);
        }
        else
        {
            leftGrid = GridStatsContainerLeft.GetComponentInChildren<GridStats>();
        }

        IList<PlayerStatistics> playerStats = stats.Teams[index].Players.Cast<PlayerStatistics>().ToList();

        if (reDraw)
        {
            //Debug.Log("MAKING STATS GRID LEFT---------------------------------------------------------------------------------PITCHERS");
            leftGrid.StartMakeStatsGrid(playerStats, "Players");
            previousPlayersCountLeft = stats.Teams[index].Players.Count;
            previousPlayersCountLeft = 0; // Reset player count when toggled
        }
        else
        {
            //Debug.Log("UPDATING STATS GRID LEFT---------------------------------------------------------------------------------PITCHERS");
            leftGrid.UpdateStatGrid(playerStats, "Players");
        }
    }

    void CreateRightDynamicTeamPanels(BasketballGameStatistics stats, int index)
    {
        bool reDraw = false;

        int rowCount;
        rowCount = stats.Teams[index].Players.Count;
        if (previousPlayersCountRight != rowCount)
            reDraw = true;

        //GridStats rightGrid;

        if (reDraw)
        {
            //clear out the old panels
            foreach (Transform child in GridStatsContainerRight.transform)
            {
                GameObject.Destroy(child.gameObject);
            }


            if (rowCount <= 5)
                rightGrid = Instantiate(GridStatsPrefab_5) as GridStats;
            else if (rowCount <= 9)
                rightGrid = Instantiate(GridStatsPrefab_9) as GridStats;
            else if (rowCount <= 15)
                rightGrid = Instantiate(GridStatsPrefab_15) as GridStats;
            else
                rightGrid = Instantiate(GridStatsPrefab_20) as GridStats;

            rightGrid.ParentFrameController = ParentFrameRight;
            rightGrid.transform.SetParent(GridStatsContainerRight.transform, false);
        }
        else
        {
            rightGrid = GridStatsContainerRight.GetComponentInChildren<GridStats>();
        }

        IList<PlayerStatistics> playerStats = stats.Teams[index].Players.Cast<PlayerStatistics>().ToList();

        if (reDraw)
        {
            //Debug.Log("MAKING STATS GRID RIGHT--------------------------------------------------------------------------------- PLAYERS");
            rightGrid.StartMakeStatsGrid(playerStats, "Players");
            previousPlayersCountRight = stats.Teams[index].Players.Count;
            previousPlayersCountRight = 0; //Reset Pitchers count when toggling
        }
        else
        {
            //Debug.Log("UPDATING STATS GRID RIGHT---------------------------------------------------------------------------------PLAYERS");
            rightGrid.UpdateStatGrid(playerStats, "Players");
        }
    }


    public void OpenButtonClicked(object sender)
    {
        Left.ShowStatPanel();
        Right.ShowStatPanel();

        if (!GlobalVars.Instance.IsUISnappedOut)
            statsController.SetStatsUnderlay(true);

        Center.ShowStatPanel();
        //see if we should show paging
        GlobalVars.Instance.IsAllowControlsOpen = false;
    }

    public void CloseButtonClicked(object sender)
    {
        //_provider.PollingIntervalSeconds = 0;
        //Open.FadeButtonIn(0.3f);

        Left.HideStatPanel();
        Center.HideStatPanel();

        statsController.SetStatsUnderlay(false);

        Right.HideStatPanel();
        //Close.FadeButtonOut(0.3f);
        GlobalVars.Instance.IsAllowControlsOpen = true;
        EventManager.Instance.PanelCloseEvent();
        //snapButton.SetActive(false);
    }

    public void ShowHideCenterFrame(bool isShow)
    {
        Center.GetComponent<FrameController>().Frame.SetActive(isShow);
        Center.GetComponent<FrameController>().ContentHolder.SetActive(isShow);
        //Center.transform.GetChild(i).gameObject.SetActive(isShow);
    }
}
