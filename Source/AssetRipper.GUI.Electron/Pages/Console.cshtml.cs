using AssetRipper.Import.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using ILogger = AssetRipper.Import.Logging.ILogger;

namespace AssetRipper.GUI.Electron.Pages;

public class ConsoleModel : PageModel
{
	private static readonly StringBuilder logBuilder = new();

	public string Log => logBuilder.ToString();

	public static void RegisterLogger()
	{
		Logger.Add(new WindowLogger());
	}

	private sealed class WindowLogger : ILogger
	{
		public void BlankLine(int numLines)
		{
			for (int i = 0; i < numLines; i++)
			{
				logBuilder.AppendLine();
			}
		}

		public void Log(LogType type, LogCategory category, string message)
		{
			foreach (string line in message.Replace("\r\n", "\n").Replace('\r', 'n').Split('\n'))
			{
				logBuilder.AppendLine($"{type} {category}: {line}");
			}
		}
	}
}
