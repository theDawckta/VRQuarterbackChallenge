using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamStats : MonoBehaviour
{
    public FrameController ParentFrameController;
    public Image Header;
    public TextMeshProUGUI HeaderText;
    public Image TeamRow;
    public TextMeshProUGUI Team1Name;
    public RawImage Team1Image;
    public TextMeshProUGUI Team2Name;
    public RawImage Team2Image;
    public GameObject StatsHolder;
    public Color Color1;
    public Color Color2;
    public GameObject StatLine;

    private List<StatLine> _statLineData = new List<StatLine>();
    private Texture2D _team1Logo;
    private Texture2D _team2Logo;
    private Image _statsHolderImage;
	//make sure we're not constantly trying to load the same image url
	private string _oldTeam1ImageURL;
	private string _oldTeam2ImageURL;

    void Awake()
    {
        _statsHolderImage = StatsHolder.GetComponent<Image>();
    }

    void Start()
    {
        Header.color = Color2;
        TeamRow.color = Color1;
        _statsHolderImage.color = Color2;
    }

    public void StartMakeTeamStats(GameStatistics team)
    {
        StartCoroutine(MakeTeamStats(team));
    }

    IEnumerator MakeTeamStats(GameStatistics teamData)
    {
        var teamStatsData = ParseTeamStats(teamData);
        Team1Name.text = teamStatsData.Team1Name;
        Team2Name.text = teamStatsData.Team2Name;

        for (int i = 0; i < teamStatsData.StatLines.Count; i++)
        {
            GameObject tempStats = Instantiate(StatLine);
            StatLine tempStatLine = tempStats.GetComponent<StatLine>();
            _statLineData.Add(tempStatLine);

            tempStatLine.StatLineInit(teamStatsData.StatLines[i].LeftColumn,
                                      teamStatsData.StatLines[i].MiddleColumn,
                                      teamStatsData.StatLines[i].RightColumn,
                                      teamStatsData.StatLines[i].LeftBarGraph,
                                      teamStatsData.StatLines[i].RightBarGraph);
            tempStatLine.transform.SetParent(StatsHolder.transform, false);
        }

		Team1Image.gameObject.SetActive(false);
		Team2Image.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(teamStatsData.Team1ImageUrl))
        {
        }
        else
        {
			if (_oldTeam1ImageURL != teamStatsData.Team1ImageUrl)
			{
				_oldTeam1ImageURL = teamStatsData.Team1ImageUrl;
				yield return StartCoroutine (GetTeamImage (teamStatsData.Team1ImageUrl, Team1Image));
			}
        }

        if (string.IsNullOrEmpty(teamStatsData.Team2ImageUrl))
        {
        }
        else
        {
			if (_oldTeam2ImageURL != teamStatsData.Team2ImageUrl)
			{
				_oldTeam2ImageURL = teamStatsData.Team2ImageUrl;
				yield return StartCoroutine(GetTeamImage(teamStatsData.Team2ImageUrl, Team2Image));
			}
        }

        ParentFrameController.StartInitFrameController();
        yield return null;
    }

    public void UpdateTeamStats(GameStatistics team)
    {
        if (!ParentFrameController.Animating && ParentFrameController.FrameOpen)
        {
            var teamStatsData = ParseTeamStats(team);
            for (int i = 0; i < teamStatsData.StatLines.Count; i++)
            {
                _statLineData[i].UpdateStatBar(teamStatsData.StatLines[i].LeftColumn,
                                               teamStatsData.StatLines[i].MiddleColumn,
                                               teamStatsData.StatLines[i].RightColumn,
                                               teamStatsData.StatLines[i].LeftBarGraph,
                                               teamStatsData.StatLines[i].RightBarGraph);
            }
        }
    }

    private class TeamStatsStatLine
    {
        public string LeftColumn { get; set; }
        public string MiddleColumn { get; set; }
        public string RightColumn { get; set; }
        public float LeftBarGraph { get; set; }
        public float RightBarGraph { get; set; }
    }

    private class TeamStatsData
    {
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public string Team1ImageUrl { get; set; }
        public string Team2ImageUrl { get; set; }
        public IList<TeamStatsStatLine> StatLines { get; private set; }

        public TeamStatsData()
        {
            StatLines = new List<TeamStatsStatLine>();
        }
    }

    TeamStatsData ParseTeamStats(GameStatistics game)
    {
        var data = new TeamStatsData();
        IList<TeamStatistics> teams = null;

        if (game.Type == GameType.Basketball)
        {
            BasketballGameStatistics basketballGameStats = (BasketballGameStatistics)game;
            teams = basketballGameStats.Teams.Cast<TeamStatistics>().ToList();
        }

        if (teams.Count != 2)
        {
            Debug.LogErrorFormat("Must have at least 2 teams, found {0}.", teams.Count);
            return data;
        }

        data.Team1Name = teams[0].Name;
        data.Team2Name = teams[1].Name;
		data.Team1ImageUrl = teams[0].LogoURL;
		data.Team2ImageUrl = teams[1].LogoURL;

        var keys = from t in teams
                   from k in t.Keys
                   select k;

        foreach (var key in keys.Distinct())
        {
            var line = new TeamStatsStatLine();

            line.MiddleColumn = key;

            bool isPercentage = key.Contains('%');

            float max = isPercentage ? 100 : teams.Max(t =>
            {
                string s;
                float f;
                return t.TryGetValue(key, out s) && Single.TryParse(s, out f) ? f : 0;
            });

            for (int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];

                string displayValue;
                float numericValue;

                if (!team.TryGetValue(key, out displayValue))
                {
                    displayValue = "0";
                    numericValue = 0;
                }
                else if (!Single.TryParse(displayValue, out numericValue))
                {
                    numericValue = 0;
                }

                float percentage = numericValue / max;

                if (isPercentage)
                {
                    displayValue = numericValue.ToString("0.#");
                }

                if (i == 0) // 2 total teams guaranteed by check at top of method
                {
                    line.LeftBarGraph = percentage;
                    line.LeftColumn = displayValue;
                }
                else
                {
                    line.RightBarGraph = percentage;
                    line.RightColumn = displayValue;
                }
            }

            data.StatLines.Add(line);
        }

        return data;
    }

    IEnumerator GetTeamImage(string imageURL, RawImage image)
    {
        using (WWW www = new WWW(imageURL))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                var unwrapper = TextureUnwrapperQueue.Instance.CreateTextureUnwrapper(www);
                yield return unwrapper.WaitForUnwrapCompletion();

                image.texture = unwrapper.Texture;
                image.gameObject.SetActive(true);
            }
        }
    }
}
