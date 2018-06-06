using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TeamsModel
{
    public List<TeamModel> Stats;

    public TeamsModel(List<TeamModel> teams)
    {
        Stats = teams;
    }
}

[Serializable]
public class TeamModel
{
    public string GridTitle;
    public string GridId;

    public List<GridDataModel> GridData;

    public TeamModel(GridDataModel starters, GridDataModel bench)
    {
        GridData.Add(starters);
        GridData.Add(bench);
    }
}