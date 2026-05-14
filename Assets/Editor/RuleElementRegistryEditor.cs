using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RuleElementRegistry))]
public class RuleElementRegistryEditor : Editor
{
    private static readonly Regex EffectIdRegex = new Regex(@"^(?<prefix>.*?)(?<number>\d+)$");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        if (FillEmptyEffectIds(serializedObject.FindProperty("entries")))
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private static bool FillEmptyEffectIds(SerializedProperty entries)
    {
        if (entries == null || !entries.isArray)
        {
            return false;
        }

        string prefix = "L01_E";
        int maxNumber = 0;
        int digitCount = 2;
        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            SerializedProperty effectId = entry.FindPropertyRelative("effectID");
            if (effectId == null || string.IsNullOrEmpty(effectId.stringValue))
            {
                continue;
            }

            Match match = EffectIdRegex.Match(effectId.stringValue);
            if (!match.Success || usedIds.Contains(effectId.stringValue))
            {
                continue;
            }

            usedIds.Add(effectId.stringValue);
            prefix = match.Groups["prefix"].Value;
            string numberText = match.Groups["number"].Value;
            digitCount = numberText.Length;
            if (int.TryParse(numberText, out int number) && number > maxNumber)
            {
                maxNumber = number;
            }
        }

        usedIds.Clear();
        bool changed = false;
        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            SerializedProperty effectId = entry.FindPropertyRelative("effectID");
            if (effectId == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(effectId.stringValue) || usedIds.Contains(effectId.stringValue))
            {
                effectId.stringValue = NextEffectId(prefix, digitCount, usedIds, ref maxNumber);
                changed = true;
            }

            usedIds.Add(effectId.stringValue);
        }

        return changed;
    }

    private static string NextEffectId(string prefix, int digitCount, HashSet<string> usedIds, ref int maxNumber)
    {
        string id;
        do
        {
            maxNumber++;
            id = prefix + maxNumber.ToString(new string('0', digitCount));
        }
        while (usedIds.Contains(id));

        return id;
    }
}
