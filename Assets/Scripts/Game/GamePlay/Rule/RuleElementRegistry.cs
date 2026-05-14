using System;
using System.Collections.Generic;
using UnityEngine;

public class RuleElementRegistry : MonoBehaviour
{
    [Serializable]
    public class RuleElementEntry
    {
        public string effectID;
        public GameObject target;
    }

    public List<RuleElementEntry> entries = new List<RuleElementEntry>();

    private readonly Dictionary<string, RuleElementEntry> _entryMap = new Dictionary<string, RuleElementEntry>();

    private void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        _entryMap.Clear();
        foreach (RuleElementEntry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.effectID) || entry.target == null)
            {
                continue;
            }

            _entryMap[entry.effectID] = entry;
        }
    }

    public bool TryGet(string effectID, out RuleElementEntry entry)
    {
        return _entryMap.TryGetValue(effectID, out entry);
    }
}
