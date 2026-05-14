using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class LubanEditor
{
    private const string GenerateMenu = "EditorTools/Luban/Generate Client Tables";

    [MenuItem(GenerateMenu, false, 100)]
    public static void GenerateClientTables()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            Debug.LogError("[Luban] Cannot resolve project root.");
            return;
        }

        string scriptPath = Path.Combine(projectRoot, "DataTables", "gen_client.bat");
        if (!File.Exists(scriptPath))
        {
            Debug.LogError($"[Luban] Missing generation script: {scriptPath}");
            return;
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{scriptPath}\"",
            WorkingDirectory = projectRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            if (process == null)
            {
                Debug.LogError("[Luban] Failed to start generation process.");
                return;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Debug.Log(output);
            }

            if (process.ExitCode != 0)
            {
                Debug.LogError($"[Luban] Generate failed with exit code {process.ExitCode}.\n{error}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogWarning(error);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[Luban] Generate client tables finished.");
    }
}
