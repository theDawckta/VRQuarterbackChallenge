using System;
using System.Collections.Generic;

public class StatisticDefinitionCollection : Dictionary<string, StatisticDefinition>
{
    private int _CurrentIndex;

    private Dictionary<string, int> _KeyOrder = new Dictionary<string, int>();

    public int IndexOf(string key)
    {
        int i;
        return _KeyOrder.TryGetValue(key, out i) ? i : -1;
    }

    public void Add(string name)
    {
        AddInternal(name, StatisticDefinition.Default);
    }

    public void Add(string name, string label)
    {
        AddInternal(name, new StatisticDefinition(label));
    }

    public void Add(string name, string label, Func<JSONObject, string> converter)
    {
        AddInternal(name, new StatisticDefinition(label, converter));
    }

    private void AddInternal(string name, StatisticDefinition def)
    {
        _KeyOrder.Add(name, _CurrentIndex++);
        Add(name, def);
    }
}