using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializationHelper
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public SerializationHelper(Dictionary<string, string> dict)
    {
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}