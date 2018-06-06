using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class GridDataModel
{
    public List<ColumnDataModel> Columns;

    public GridDataModel(List<ColumnDataModel> columns)
    {
        Columns = columns;
    }
}

[Serializable]
public class ColumnDataModel
{
    public string Header;
    public string[] Values;

    public ColumnDataModel(string header, string[] values)
    {
        Header = header;
        Values = values;
    }
}