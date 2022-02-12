using AssetRipper.Core.Logging;
using Avalonia;
using System;

namespace AssetRipper.GUI
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Logger.Add(new FileLogger());
			Logger.Add(new ConsoleLogger());
			Logger.LogSystemInformation("AssetRipper GUI Version");

			// Initialization code. Don't use any Avalonia, third-party APIs or any
			// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
			// yet and stuff might break.
			BuildAvaloniaApp().Start(App.AppMain, args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace();
	}
}
