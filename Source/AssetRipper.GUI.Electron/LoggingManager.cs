using AssetRipper.Import.Logging;
using ElectronNET.API;
using System.Text;
using ElectronAPI = ElectronNET.API.Electron;
using ILogger = AssetRipper.Import.Logging.ILogger;

namespace AssetRipper.GUI.Electron;

internal static class LoggingManager
{
	private static readonly StringBuilder logBuilder = new();
	private static bool registered;
	private static BrowserWindow? consoleWindow;

	public static string Log
	{
		get
		{
			lock (logBuilder)
			{
				return logBuilder.ToString();
			}
		}
	}

	public static void RegisterLogger()
	{
		if (!registered)
		{
			Logger.Add(new WindowLogger());
			registered = true;
		}
	}

	public static async Task LaunchWindow()
	{
		consoleWindow = await ElectronAPI.WindowManager.CreateWindowAsync(LocalHost.BaseUrl + "Console");
	}

	private static void TriggerReload()
	{
		consoleWindow?.Reload();
	}

	private sealed class WindowLogger : ILogger
	{
		public void BlankLine(int numLines)
		{
			lock (logBuilder)
			{
				for (int i = 0; i < numLines; i++)
				{
					logBuilder.AppendLine();
				}
			}
			TriggerReload();
		}

		public void Log(LogType type, LogCategory category, string message)
		{
			lock (logBuilder)
			{
				foreach (string line in message.Replace("\r\n", "\n").Replace('\r', 'n').Split('\n'))
				{
					logBuilder.AppendLine($"{type} {category}: {line}");
				}
			}
			TriggerReload();
		}
	}
}
