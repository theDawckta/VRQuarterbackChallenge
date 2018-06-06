using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridDataController : MonoBehaviour 
{
    public ColumnController Column;
    public int NumOfColumns { get { return _columns.Count; } private set{} }

    private List<ColumnController> _columns = new List<ColumnController>();

    public void CreateGridData(GridDataModel gridData, Color altRowBackgroundColor)
    {
        for (int i = 0; i < gridData.Columns.Count; i++)
        {
            ColumnController newColumn = Instantiate(Column);

            if (i == 0)
            {
                newColumn.CreateColumn(gridData.Columns[i], altRowBackgroundColor, TextAlignmentOptions.CaplineLeft, TextAlignmentOptions.CaplineLeft);
                _columns.Add(newColumn);
                newColumn.transform.SetParent(transform, false);
            }
            else
            {
                newColumn.CreateColumn(gridData.Columns[i], altRowBackgroundColor, TextAlignmentOptions.Capline, TextAlignmentOptions.Capline);
                _columns.Add(newColumn);
                newColumn.transform.SetParent(transform, false);
            }
        }
	}

    public void UpdateGridData(GridDataModel gridData)
    {
        for (int i = 0; i < gridData.Columns.Count; i++)
            _columns[i].UpdateColumn(gridData.Columns[i]);
    }

    public float GetColumnWidth(int columnIndex)
    {
        if (columnIndex >= 0 && columnIndex < _columns.Count)
            return _columns[columnIndex].GetPreferredWidth();
        else
        {
            Debug.Log("Zero returned for GetColumnWidth in GridDataController, index out of range");
            return 0.0f;
        }
    }

    public void SetColumnWidth(int columnIndex, float newWidth)
    {
        if (columnIndex >= 0 && columnIndex < _columns.Count)
            _columns[columnIndex].gameObject.GetComponent<LayoutElement>().preferredWidth = newWidth;
        else
            Debug.Log("Index out of range for SetColumnWidth in GridDataController");
    }

    public void SetColumnPadding(int columnIndex, int left, int right)
    {
        if (columnIndex >= 0 && columnIndex < _columns.Count)
            _columns[columnIndex].SetPadding(left, right);
        else
            Debug.Log("Index out of range for SetColumnWidth in GridDataController");
    }
}
