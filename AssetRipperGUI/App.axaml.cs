using AssetRipper.Core.Logging;
using AssetRipper.GUI.Logging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace AssetRipper.GUI
{
	public class App : Application
	{

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public static void AppMain(Application app, string[] args)
		{
			Avalonia.Logging.Logger.Sink = new RipperAvaloniaSink();

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				try
				{
					Logger.Error(LogCategory.General, "Unhandled app-level fatal exception!", e.ExceptionObject as Exception);
				}
				catch (Exception)
				{
					//Ignore, that's all we can do.
				}
			};

			app.Run(new MainWindow());
		}
	}
}