using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerStatsController : MonoBehaviour 
{
    public delegate void PlayerStatsDownloadError(string errorText);
    public event PlayerStatsDownloadError OnPlayerStatsDownloadError;

    public float DelayStartUpTime = 10.0f;
    public GridDataController GridData;
    public Sprite HeaderBackground;
    public Color altRowBackgroundColor;
    public GameObject GridHolderTeam1;
    public TextMeshProUGUI HeaderTeam1Text;
    public Image[] ColorImagesTeam1;
    public Image Team1Image;
    public GameObject GridHolderTeam2;
    public TextMeshProUGUI HeaderTeam2Text;
    public Image[] ColorImagesTeam2;
    public Image Team2Image;

    private bool _statsDrawn = false;
    private TeamsModel _teams;
    private GridDataModel _gridStartersData;
    private GridDataModel _gridBenchData;
    private GridDataController _gridStartersTeam1;
    private GridDataController _gridBenchTeam1;
    private GridDataController _gridStartersTeam2;
    private GridDataController _gridBenchTeam2;
    private string _statHeaderImageLocation;
    private string _stringModifierStart = "<color=#999999><size=19>";
    private string _stringModifierEnd = "</size></color>";
    private float _valueColumnWidth = 70.0f;
    private Dictionary<string, string> _teamImageLookup = new Dictionary<string, string> { 
        {"1610612737", "ATL"},
        {"1610612738", "BOS"},
        {"1610612739", "CLE"},
        {"1610612740", "NOP"},
        {"1610612741", "CHI"},
        {"1610612742", "DAL"},
        {"1610612743", "DEN"},
        {"1610612744", "GSW"},
        {"1610612745", "HOU"},
        {"1610612746", "LAC"},
        {"1610612747", "LAL"},
        {"1610612748", "MIA"},
        {"1610612749", "MIL"},
        {"1610612750", "MIN"},
        {"1610612751", "BKN"},
        {"1610612752", "NYK"},
        {"1610612753", "ORL"},
        {"1610612754", "IND"},
        {"1610612755", "PHI"},
        {"1610612756", "PHX"},
        {"1610612757", "POR"},
        {"1610612758", "SAC"},
        {"1610612759", "SAS"},
        {"1610612760", "OKC"},
        {"1610612761", "TOR"}, 
        {"1610612762", "UTA"},
        {"1610612763", "MEM"},
        {"1610612764", "WAS"},
        {"1610612765", "DET"},
        {"1610612766", "CHA"}
    };
    private Dictionary<string, string> _teamColorLookup = new Dictionary<string, string> {
        {"1610612737", "#E03A3E"},
        {"1610612738", "#008348"},
        {"1610612739", "#860038"},
        {"1610612740", "#002B5C"},
        {"1610612741", "#CE1141"},
        {"1610612742", "#007DC5"},
        {"1610612743", "#00285E"},
        {"1610612744", "#006BB6"},
        {"1610612745", "#CE1141"},
        {"1610612746", "#ED174C"},
        {"1610612747", "#FDB927"},
        {"1610612748", "#000000"},
        {"1610612749", "#00471B"},
        {"1610612750", "#0C2340"},
        {"1610612751", "#000000"},
        {"1610612752", "#006BB6"},
        {"1610612753", "#0077C0"},
        {"1610612754", "#002D62"},
        {"1610612755", "#006BB6"},
        {"1610612756", "#1D1160"},
        {"1610612757", "#E03A3E"},
        {"1610612758", "#5A2D81"},
        {"1610612759", "#000000"},
        {"1610612760", "#007AC1"},
        {"1610612761", "#CE1141"},
        {"1610612762", "#002B5C"},
        {"1610612763", "#6189B9"},
        {"1610612764", "#002B5C"},
        {"1610612765", "#ED174C"},
        {"1610612766", "#00788C"}
    };

    public void SetTeamImageLocation(string teamImageLocation)
    {
        _statHeaderImageLocation = teamImageLocation;
    }

	public IEnumerator GetPlayerStats(string playerStatsDataLocation)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(playerStatsDataLocation))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                if (OnPlayerStatsDownloadError != null)
                    OnPlayerStatsDownloadError(www.error);

                DestroyStats();

                Debug.Log("Error in PlayerStatsModel GetPlayerStats: " + www.error);
            }
            else
            {
                _teams = JsonUtility.FromJson<TeamsModel>(www.downloadHandler.text);

                if (_teams.Stats.Count == 2 && _teams.Stats.Count != 0)
                {
                    // special logic to pull positions out and adjoin it to player in first column
                    FormatData(_teams.Stats[0]);
                    FormatData(_teams.Stats[1]);

                    if (!_statsDrawn)
                        DrawStats();
                    else
                        UpdateStats();
                }
                else
                {
                    DestroyStats();
                }
            }
        }

        yield return null;
    }

    private void FormatData(TeamModel teamModel)
    {
        int spaceIndex;

        for (int i = 0; i < teamModel.GridData[0].Columns[0].Values.Length; i++)
        {
            spaceIndex = teamModel.GridData[0].Columns[0].Values[i].LastIndexOf(' ');
            teamModel.GridData[0].Columns[0].Values[i] = teamModel.GridData[0].Columns[0].Values[i].Remove(1, spaceIndex);
            teamModel.GridData[0].Columns[0].Values[i] = teamModel.GridData[0].Columns[0].Values[i].Insert(1, ". ");
            teamModel.GridData[0].Columns[0].Values[i] = teamModel.GridData[0].Columns[0].Values[i] + " " + _stringModifierStart + teamModel.GridData[0].Columns[1].Values[i] + _stringModifierEnd;
        }

        for (int i = 0; i < teamModel.GridData[1].Columns[0].Values.Length; i++)
        {
            spaceIndex = teamModel.GridData[1].Columns[0].Values[i].LastIndexOf(' ');
            teamModel.GridData[1].Columns[0].Values[i] = teamModel.GridData[1].Columns[0].Values[i].Remove(1, spaceIndex);
            teamModel.GridData[1].Columns[0].Values[i] = teamModel.GridData[1].Columns[0].Values[i].Insert(1, ". ");
        }

        // removes positions column from starters so it matches bench data
        teamModel.GridData[0].Columns.RemoveAt(1);
    }

    void DrawStats()
    {
        float normalizedWidth;
        Color Team1Color;
        Color Team2Color;

        GridHolderTeam1.SetActive(true);
        HeaderTeam1Text.text = _teams.Stats[0].GridTitle;
        _gridStartersTeam1 = Instantiate(GridData);
        _gridStartersTeam1.transform.SetParent(GridHolderTeam1.transform, false);
        _gridStartersTeam1.CreateGridData(_teams.Stats[0].GridData[0], altRowBackgroundColor);

        _gridBenchTeam1 = Instantiate(GridData);
        _gridBenchTeam1.transform.SetParent(GridHolderTeam1.transform, false);
        _gridBenchTeam1.CreateGridData(_teams.Stats[0].GridData[1], altRowBackgroundColor);

        _gridStartersTeam1.SetColumnPadding(0, 20, 0);
        _gridBenchTeam1.SetColumnPadding(0, 20, 0);

        try
        {
            ColorUtility.TryParseHtmlString(_teamColorLookup[_teams.Stats[0].GridId], out Team1Color);
        }
        catch(KeyNotFoundException e)
        {
            ColorUtility.TryParseHtmlString("#0366AC", out Team1Color);
        }

        for (int i = 0; i < ColorImagesTeam1.Length; i++)
            ColorImagesTeam1[i].color = Team1Color;

        try
        {
            StartCoroutine(LoadHeaderImage(Team1Image, _statHeaderImageLocation + _teamImageLookup[_teams.Stats[0].GridId] + "_right.jpg"));
        }
        catch(KeyNotFoundException e)
        {
            Team1Image.gameObject.SetActive(false);
        }

        GridHolderTeam2.SetActive(true);
        HeaderTeam2Text.text = _teams.Stats[1].GridTitle;
        _gridStartersTeam2 = Instantiate(GridData);
        _gridStartersTeam2.transform.SetParent(GridHolderTeam2.transform, false);
        _gridStartersTeam2.CreateGridData(_teams.Stats[1].GridData[0], altRowBackgroundColor);

        _gridBenchTeam2 = Instantiate(GridData);
        _gridBenchTeam2.transform.SetParent(GridHolderTeam2.transform, false);
        _gridBenchTeam2.CreateGridData(_teams.Stats[1].GridData[1], altRowBackgroundColor);

        _gridStartersTeam2.SetColumnPadding(0, 20, 50);
        _gridBenchTeam2.SetColumnPadding(0, 20, 50);

        try
        {
            ColorUtility.TryParseHtmlString(_teamColorLookup[_teams.Stats[1].GridId], out Team2Color);
        }
        catch (KeyNotFoundException e)
        {
            ColorUtility.TryParseHtmlString("#FF003C", out Team2Color);
        }

        for (int i = 0; i < ColorImagesTeam2.Length; i++)
            ColorImagesTeam2[i].color = Team2Color;

        try
        {
            StartCoroutine(LoadHeaderImage(Team2Image, _statHeaderImageLocation + _teamImageLookup[_teams.Stats[1].GridId] + "_left.jpg")); 
        }
        catch(KeyNotFoundException e)
        {
            Team2Image.gameObject.SetActive(false);
        }

        normalizedWidth = Math.Max(Math.Max(_gridStartersTeam1.GetColumnWidth(0), _gridBenchTeam1.GetColumnWidth(0)), Math.Max(_gridStartersTeam1.GetColumnWidth(0), _gridBenchTeam1.GetColumnWidth(0)));

        _gridStartersTeam1.SetColumnWidth(0, normalizedWidth);
        _gridBenchTeam1.SetColumnWidth(0, normalizedWidth);

        for (int i = 1; i < _gridStartersTeam1.NumOfColumns; i++)
            _gridStartersTeam1.SetColumnWidth(i, _valueColumnWidth);

        for (int i = 1; i < _gridBenchTeam1.NumOfColumns; i++)
            _gridBenchTeam1.SetColumnWidth(i, _valueColumnWidth);
        
        _gridStartersTeam2.SetColumnWidth(0, normalizedWidth);
        _gridBenchTeam2.SetColumnWidth(0, normalizedWidth);

        for (int i = 1; i < _gridStartersTeam2.NumOfColumns; i++)
            _gridStartersTeam2.SetColumnWidth(i, _valueColumnWidth);

        for (int i = 1; i < _gridBenchTeam2.NumOfColumns; i++)
            _gridBenchTeam2.SetColumnWidth(i, _valueColumnWidth);

        _statsDrawn = true;
    }

    IEnumerator LoadHeaderImage(Image img, string spriteUrl)
    {
        WWW www = new WWW(spriteUrl);
        yield return www;
        if (www.error == null)
        {
            img.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            img.sprite.texture.wrapMode = TextureWrapMode.Clamp;
            img.sprite.texture.filterMode = FilterMode.Trilinear;
        }   
        else
            img.gameObject.SetActive(false);
    }

    void UpdateStats()
    {
        _gridStartersTeam1.UpdateGridData(_teams.Stats[0].GridData[0]);
        _gridBenchTeam1.UpdateGridData(_teams.Stats[0].GridData[1]);
        _gridStartersTeam2.UpdateGridData(_teams.Stats[1].GridData[0]);
        _gridBenchTeam2.UpdateGridData(_teams.Stats[1].GridData[1]);
    }

	void DestroyStats()
	{
        foreach (Transform child in GridHolderTeam1.transform)
        {
            if (child.gameObject.name != "FrameItems")
                Destroy(child.gameObject);
        }
        GridHolderTeam1.gameObject.SetActive(false);

        foreach (Transform child in GridHolderTeam2.transform)
        {
            if (child.gameObject.name != "FrameItems")
                Destroy(child.gameObject);
        }
        GridHolderTeam2.gameObject.SetActive(false);

        _statsDrawn = false;
	}

	void PlayerStatsRetrieved(TeamsModel teams)
    {
        _teams = teams;
    }

    void PlayerStatsError()
    {
        //Debug.Log("WE DON'T GOT STATS");
    }
}