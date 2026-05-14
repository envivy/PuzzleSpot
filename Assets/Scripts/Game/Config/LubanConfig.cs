using System;
using cfg;
using Luban.SimpleJSON;
using UnityEngine;

public static class LubanConfig
{
    public const string ResourceRoot = "LubanData";
    private static Tables _cachedTables;

    public static string LoadJsonText(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("Luban data file name cannot be empty.", nameof(fileName));
        }

        string path = $"{ResourceRoot}/{TrimJsonExtension(fileName)}";
        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.LogError($"[Luban] Missing generated data: Resources/{path}.json");
            return null;
        }

        return asset.text;
    }

    public static Tables LoadTables()
    {
        if (_cachedTables == null)
        {
            _cachedTables = new Tables(file => JSON.Parse(LoadJsonText(file)));
        }

        return _cachedTables;
    }

    private static string TrimJsonExtension(string fileName)
    {
        return fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(0, fileName.Length - 5)
            : fileName;
    }
}
