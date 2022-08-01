using AssetRipper.Core.Logging;
using Avalonia;

namespace AssetRipper.GUI
{
	static class Program
	{
		//https://docs.microsoft.com/en-us/dotnet/api/system.stathreadattribute?view=net-6.0
		//https://github.com/AvaloniaUI/avalonia-dotnet-templates/pull/58
		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Logger.Add(new FileLogger());
				Logger.Add(new ConsoleLogger());
				Logger.LogSystemInformation("AssetRipper GUI Version");
				RunAvalonia();
			}
			else
			{
				ConsoleApp.ParseArgumentsAndRun(args);
			}
		}

		/// <summary>
		/// Initialization code. Don't use any Avalonia, third-party APIs or any
		/// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		/// yet and stuff might break.
		/// </summary>
		private static void RunAvalonia()
		{
			BuildAvaloniaApp().Start(App.AppMain, Array.Empty<string>());
		}

		/// <summary>
		/// Avalonia configuration, don't remove; also used by visual designer.
		/// </summary>
		/// <returns></returns>
		private static AppBuilder BuildAvaloniaApp()
		{
			return AppBuilder.Configure<App>()
						.UsePlatformDetect()
						.LogToTrace();
		}
	}
}
