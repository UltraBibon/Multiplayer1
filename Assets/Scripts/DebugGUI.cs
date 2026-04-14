using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class DebugGUI : MonoBehaviour
{
    public static DebugGUI Instance { get; private set; }
    private const int MAX_LINES = 40;
    private List<string> logs = new List<string>();
    private Vector2 scrollPos;
    private bool clearOnPlay = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (clearOnPlay) logs.Clear();
    }

    public static void Log(string message) => Instance?.AddLog(message);
    public static void LogWarning(string message) => Log($"<color=yellow>⚠️ {message}</color>");
    public static void LogError(string message) => Log($"<color=red>❌ {message}</color>");

    private void AddLog(string message)
    {
        logs.Add($"[{Time.time:F1}s] {message}");
        if (logs.Count > MAX_LINES) logs.RemoveAt(0);
    }

    private void OnGUI()
    {
        // Окно 30x30, размер 380x350
        GUILayout.Window(999, new Rect(10, 10, 380, 350), DrawWindow, "🐛 Network Debug");
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", GUILayout.Width(60))) logs.Clear();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(290));
        foreach (var line in logs)
            GUILayout.Label(line, new GUIStyle { wordWrap = true });
        GUILayout.EndScrollView();

        GUILayout.Space(5);
        GUILayout.Label($"FPS: {1f / Time.deltaTime:F0} | Clients: {logs.Count}");
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}