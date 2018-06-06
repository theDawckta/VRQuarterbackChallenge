using System;
using System.Linq;

public class StatisticDefinition
{
    public static readonly StatisticDefinition Default = new StatisticDefinition();

    public StatisticDefinition() { }

    public StatisticDefinition(string label) : this(label, null) { }

    public StatisticDefinition(string label, Func<JSONObject, string> converter)
    {
        Label = label;
        Converter = converter;
    }

    public string Label { get; set; }
    public Func<JSONObject, string> Converter { get; set; }

    public string GetLabel(string key)
    {
        if (!String.IsNullOrEmpty(Label))
            return Label;

        return String.Join(" ", key.Split('_').Select(_ => Char.ToUpper(_[0]) + _.Substring(1)).ToArray());
    }

    public string GetValue(JSONObject value)
    {
        if (Converter != null)
            return Converter(value);

        return value.str;
    }
}