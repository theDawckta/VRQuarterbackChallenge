using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridStats : MonoBehaviour
{
    public FrameController ParentFrameController;
    public GameObject Column;
    public GameObject TextPanelHeader;
    public GameObject TextPanelBold;
    public GameObject TextPanel;
    public Color RowColor1;
    public Color RowColor2;
    public Sprite UpperLeft;
    public Sprite UpperRight;
    public Sprite LowerRight;
    public Sprite LowerLeft;

    public Sprite pitcherIcon;
    public Sprite batterIcon;

    private GridStatsData gridStatsData;
    private List<Text> _data = new List<Text>();
    private List<Image> _pbIcon = new List<Image>();

    private string previousPlayerID;

    public enum IconType {
        Pitcher,
        Batter
    };

    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.S))
    //    {
    //        SetIcon("577809",IconType.Pitcher);
    //    }
    //}

    //if we want to show pitchers, we'd need to pass a boolean here
    public void StartMakeStatsGrid(IList<PlayerStatistics> team, string colHeader = "")
    {
		StartCoroutine(MakeStatGrid(team, colHeader));
    }

    //public void StartMakeCustomStatsGrid(IList<PlayerStatistics> team, string colHeader = "")
    //{
    //    StartCoroutine(MakeCustomStatGrid(team, colHeader));
    //}

    //IEnumerator MakeCustomStatGrid(IList<PlayerStatistics> team, string colHeader = "")
    //{
    //    HorizontalLayoutGroup HLG;
    //    GameObject holderObject = new GameObject();
    //    holderObject.AddComponent<HorizontalLayoutGroup>();
    //    var gridStatsData = ParsePlayerStats(team, colHeader);

    //    for (int column = 0; column < gridStatsData.Columns.Count; column++)
    //    {
    //        GameObject tempDataGroup = Instantiate(Column);
    //        GameObject headerPanel = Instantiate(TextPanelHeader);
    //        HLG = headerPanel.GetComponent<HorizontalLayoutGroup>();
    //        HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right, HLG.padding.top + (HLG.padding.top / 2), HLG.padding.bottom);

    //        // Logic for correct padding
    //        if (column == 0)
    //            HLG.padding = new RectOffset(HLG.padding.left + (HLG.padding.left / 2), HLG.padding.right, HLG.padding.top, HLG.padding.bottom);
    //        if (column == gridStatsData.Columns.Count - 1)
    //            HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right + (HLG.padding.right / 2), HLG.padding.top, HLG.padding.bottom);

    //        // Loginc for corner sprites
    //        if (column == 0)
    //            headerPanel.GetComponent<Image>().sprite = UpperLeft;
    //        if (column == gridStatsData.Columns[column].Data.Count - 1)
    //            headerPanel.GetComponent<Image>().sprite = UpperRight;

    //        headerPanel.GetComponent<Image>().color = RowColor1;
    //        headerPanel.GetComponentInChildren<Text>().text = "";
    //        if (column > 0)
    //            headerPanel.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
    //        headerPanel.transform.SetParent(tempDataGroup.transform, false);

    //        for (int i = 0; i < 5; i++)
    //        {
    //            GameObject textHolder;
    //            Color rowColor = (i % 2 == 0) ? RowColor2 : RowColor1;

    //            if (column == 0)
    //                textHolder = Instantiate(TextPanelBold);
    //            else
    //                textHolder = Instantiate(TextPanel);
    //            HLG = textHolder.GetComponent<HorizontalLayoutGroup>();

    //            // Logic for correct padding
    //            if (column == 0)
    //                HLG.padding = new RectOffset(HLG.padding.left + (HLG.padding.left / 2), HLG.padding.right, HLG.padding.top, HLG.padding.bottom);
    //            if (column == gridStatsData.Columns.Count - 1)
    //                HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right + (HLG.padding.right / 2), HLG.padding.top, HLG.padding.bottom);
    //            if (i == gridStatsData.Columns[column].Data.Count - 1)
    //                HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right, HLG.padding.top, HLG.padding.bottom + (HLG.padding.bottom / 2));

    //            // Logic for corner sprites
    //            if (column == gridStatsData.Columns.Count - 1 && i == gridStatsData.Columns[column].Data.Count - 1)
    //                textHolder.GetComponent<Image>().sprite = LowerRight;
    //            if (column == 0 && i == gridStatsData.Columns[column].Data.Count - 1)
    //                textHolder.GetComponent<Image>().sprite = LowerLeft;

    //            textHolder.GetComponent<Image>().color = rowColor;
    //            textHolder.GetComponentInChildren<Text>().text = "";
    //            textHolder.transform.SetParent(tempDataGroup.transform, false);
    //            _data.Add(textHolder.GetComponentInChildren<Text>());
    //        }

    //        tempDataGroup.transform.SetParent(holderObject.transform, false);
    //        holderObject.transform.SetParent(this.gameObject.transform, false);
    //    }

    //    yield return null;
    //    ParentFrameController.StartInitFrameController();
    //}

    IEnumerator MakeStatGrid(IList<PlayerStatistics> team, string colHeader = "")
    {

        ParentFrameController.HideContents();

        GameObject holderObject;
        holderObject = transform.GetChild(0).gameObject;
		//holderObject.AddComponent<HorizontalLayoutGroup> ();
		gridStatsData = ParsePlayerStats(team, colHeader);

        for (int column = 0; column < gridStatsData.Columns.Count-1; column++)
        {
            GameObject tempDataGroup = holderObject.transform.GetChild(column).gameObject; /* = Instantiate(Column);*/
            GameObject headerPanel = tempDataGroup.transform.GetChild(0).gameObject; /*Instantiate(TextPanelHeader);*/

            /*
            HLG = headerPanel.GetComponent<HorizontalLayoutGroup>();
            HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right, HLG.padding.top + (HLG.padding.top / 2), HLG.padding.bottom);

            Logic for correct padding
            if (column == 0)
                    HLG.padding = new RectOffset(HLG.padding.left + (HLG.padding.left / 2), HLG.padding.right, HLG.padding.top, HLG.padding.bottom);
            if (column == gridStatsData.Columns.Count - 1)
                HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right + (HLG.padding.right / 2), HLG.padding.top, HLG.padding.bottom);*/

            // Loginc for corner sprites
            if (column == 0)
                headerPanel.GetComponent<Image>().sprite = UpperLeft;
            if (column == gridStatsData.Columns[column].Data.Count - 1)
                headerPanel.GetComponent<Image>().sprite = UpperRight;

            headerPanel.GetComponent<Image>().color = RowColor1;
            headerPanel.GetComponentInChildren<Text>().text = gridStatsData.Columns[column].Header;
            if(column > 0)
				headerPanel.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            //headerPanel.transform.SetParent(tempDataGroup.transform, false);

            for (int i = 0; i < gridStatsData.Columns[column].Data.Count; i++)
            {
                GameObject textHolder;

                textHolder = tempDataGroup.transform.GetChild(i + 1).gameObject;

                Color rowColor = (i % 2 == 0) ? RowColor2 : RowColor1;

                //if (column == 0)
                //    textHolder = Instantiate(TextPanelBold);
                //else
                //    textHolder = Instantiate(TextPanel);

                /*HLG = textHolder.GetComponent<HorizontalLayoutGroup>();

                // Logic for correct padding
                if (column == 0)
                    HLG.padding = new RectOffset(HLG.padding.left + (HLG.padding.left / 2), HLG.padding.right, HLG.padding.top, HLG.padding.bottom);
                if (column == gridStatsData.Columns.Count - 1)
                    HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right + (HLG.padding.right / 2), HLG.padding.top, HLG.padding.bottom);
                if (i == gridStatsData.Columns[column].Data.Count - 1)
                    HLG.padding = new RectOffset(HLG.padding.left, HLG.padding.right, HLG.padding.top, HLG.padding.bottom + (HLG.padding.bottom / 2));*/

                // Logic for corner sprites
                if (column == gridStatsData.Columns.Count - 1 && i == gridStatsData.Columns[column].Data.Count - 1)
                    textHolder.GetComponent<Image>().sprite = LowerRight;
                if (column == 0 && i == gridStatsData.Columns[column].Data.Count - 1)
                    textHolder.GetComponent<Image>().sprite = LowerLeft;

                textHolder.GetComponent<Image>().color = rowColor;
                textHolder.GetComponentInChildren<Text>().text = gridStatsData.Columns[column].Data[i];
                //textHolder.transform.SetParent(tempDataGroup.transform, false);
                _data.Add(textHolder.GetComponentInChildren<Text>());

                //Adding image components for player names to the list
                if (column == 0)
                {
                    //Debug.Log("GO added for PB Icon :::::::::::::::::::::::::::::: " + textHolder.transform.GetChild(0).gameObject.name);
                    _pbIcon.Add(textHolder.transform.GetChild(0).gameObject.GetComponent<Image>());
                }
            }

            //tempDataGroup.transform.SetParent(holderObject.transform, false);
            //holderObject.transform.SetParent(this.gameObject.transform, false);
        }

        yield return null;
        ParentFrameController.StartInitFrameController();
    }

	public void UpdateStatGrid(IList<PlayerStatistics> team, string colHeader = "")
    {
        if (!ParentFrameController.Animating && ParentFrameController.FrameOpen)
        {
            gridStatsData = ParsePlayerStats(team, colHeader);
            int index = 0;
            for (int i = 0; i < gridStatsData.Columns.Count-1; i++)
            {
                for (int j = 0; j < gridStatsData.Columns[i].Data.Count; j++)
                {
                    if (_data[index].text != gridStatsData.Columns[i].Data[j])
                    {
                        StartCoroutine(UpdateText(_data[index], gridStatsData.Columns[i].Data[j]));
                    }
                    index++;
                }
            }
        }
    }

    IEnumerator UpdateText(Text textControl, string newText)
    {
        textControl.CrossFadeAlpha(0.0f, 0.3f, false);
        yield return new WaitForSeconds(0.3f);
        textControl.text = newText;
        textControl.CrossFadeAlpha(1.0f, 0.3f, false);
    }

    /// <summary>
    /// Looks up the player amongst the list and activates the Pitcher/Batter Icon
    /// Also de activates the icon on the old player
    /// </summary>
    /// <param name="pitcherID">Player ID for the current Pitcher/Batter</param>
    /// <param name="iconType">The icon to be set, Pitcher/Batter</param>
    public void SetIcon(string playerID, IconType iconType)
    {
        // We already have the latest gridStatsData parsed from what is displayed
        //Iterating through that to lookup for the player ID, we can actiavte the icon accordingly

        //Debug.Log("Looking up player ID: " + playerID + " to be set ot icon :" + iconType.ToString());

        if (gridStatsData == null)
        {
            Debug.Log("GRID STATS DATA IS NULL ::::::::::::::::::::::::::::::::::::::::::");
            return;
        }

        if (playerID == previousPlayerID)
            return;
        else
            previousPlayerID = playerID;

        for (int i = 0; i < gridStatsData.Columns[0].Data.Count; i++)
        {
            //Debug.Log(gridStatsData.Columns[gridStatsData.Columns.Count - 1].Data[i]);

            if (gridStatsData.Columns[gridStatsData.Columns.Count - 1].Data[i] == null)
            {
                Debug.Log("PLAYER ID IS NULL ::::::::::::::::::::::::::::::::::::::::::");
                EventManager.Instance.PitcherBatterResetEvent();  //Firing so that previous pitcher and player ID's are cleared to be set again
                return;
            }

            if (gridStatsData.Columns[gridStatsData.Columns.Count - 1].Data[i] != playerID)
                _pbIcon[i].enabled = false;
            else
            {
                //Player ID match found
                if (iconType == IconType.Pitcher)
                    _pbIcon[i].sprite = pitcherIcon;
                else
                    _pbIcon[i].sprite = batterIcon;

                _pbIcon[i].enabled = true;
            }
        }
    }

    GridStatsData ParsePlayerStats(IList<PlayerStatistics> team, string colHeader = "Players")
    {
        var data = new GridStatsData();

		var playerColumn = new GridStatsColumn(colHeader);
		var activePlayers = from p in team
			where p.Played
			select p;

        //		if (isPitchers)
        //		{
        //			playerColumn = new GridStatsColumn("Pitchers");
        //			activePlayers = from p in team.Pitchers
        //				where p.Played
        //				select p;
        //		}

        foreach (var player in activePlayers)
        {
            playerColumn.Data.Add(player.Name);
        }

        data.Columns.Add(playerColumn);
        var keys = from p in activePlayers
                   from k in p.Keys
                   select k;

        foreach (var key in keys.Distinct())
        {
            var column = new GridStatsColumn(key);
            foreach (var player in activePlayers)
            {
                string value;

                if (!player.TryGetValue(key, out value))
                    value = "-";

                column.Data.Add(value);
            }

            data.Columns.Add(column);
        }

        return data;
    }

    private class GridStatsColumn
    {
        public string Header { get; private set; }
        public IList<string> Data { get; private set; }

        public GridStatsColumn(string header)
        {
            Header = header;
            Data = new List<string>();
        }
    }

    private class GridStatsData
    {
        public IList<GridStatsColumn> Columns { get; private set; }

        public GridStatsData()
        {
            Columns = new List<GridStatsColumn>();
        }
    }
}