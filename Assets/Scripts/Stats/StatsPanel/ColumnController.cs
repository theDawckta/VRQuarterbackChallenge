using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColumnController : MonoBehaviour 
{
    public TextBlockController Header;
    public TextBlockController Value;

    private TextBlockController _header;
    private List<TextBlockController> _values = new List<TextBlockController>();
    private LayoutElement _layoutElement;
    private VerticalLayoutGroup _verticalLayoutGroup;

	private void Awake()
	{
        _layoutElement = gameObject.GetComponent<LayoutElement>();
        _verticalLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>();
	}

    public void CreateColumn(ColumnDataModel columnData, Color altRowBackgroundColor, TextAlignmentOptions headerAlignment = TextAlignmentOptions.Center, TextAlignmentOptions valuesAlignment = TextAlignmentOptions.Center)
    {
        _header = Instantiate(Header);
        _header.transform.SetParent(transform, false);
        _header.SetText(columnData.Header);
        _header.SetAlignment(headerAlignment);

        for (int i = 0; i < columnData.Values.Length; i++)
        {
            TextBlockController newValue = Instantiate(Value);
            newValue.transform.SetParent(transform, false);
            newValue.SetText(columnData.Values[i]);
            newValue.SetAlignment(valuesAlignment);
            if (i % 2 != 0)
                newValue.SetBackgroundColor(altRowBackgroundColor);
            _values.Add(newValue);
        }
    }

    public void UpdateColumn(ColumnDataModel columnData)
    {
        if(columnData.Header != _header.GetText())
            _header.UpdateText(columnData.Header);

        for (int i = 0; i < _values.Count; i++)
        {
            if(columnData.Values[i] != _values[i].GetText())
                _values[i].UpdateText(columnData.Values[i]);
        }
    }

    public void SetColumnWidth(float width)
    {
        _layoutElement.preferredWidth = width;
    }

    public float GetPreferredWidth()
    {
        float columnWidth = _header.GetWidth();

        for (int i = 0; i < _values.Count; i++)
            columnWidth = (_values[i].GetWidth() > columnWidth) ? _values[i].GetWidth() : columnWidth;

        return columnWidth;
    }

    public void SetPadding(float left, float right)
    {
        _header.SetMargin(left, right);

        for (int i = 0; i < _values.Count; i++)
            _values[i].SetMargin(left, right);
    }
}
