using UnityEngine;
using TMPro;

public class LogToTextMeshPro : MonoBehaviour
{
    public TextMeshProUGUI logText; // Reference to the TextMeshProUGUI component
    public int maxLogCount = 100; // Maximum number of logs to keep in the display
    private string logContent = ""; // The log content stored as a single string

    void OnEnable()
    {
        // Register the HandleLog function to be called whenever a log message is received
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // Unregister the HandleLog function to avoid memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Append the log message to the logContent string
        logContent += logString + "\n";

        // Limit the number of logs shown to avoid performance issues
        string[] logLines = logContent.Split('\n');
        if (logLines.Length > maxLogCount)
        {
            // Keep only the last maxLogCount lines
            logContent = string.Join("\n", logLines, logLines.Length - maxLogCount, maxLogCount);
        }

        // Update the TextMeshPro component with the new log content
        logText.text = logContent;
    }
}
