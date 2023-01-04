using AssetRipper.GUI.Logging;
using AssetRipper.Import.Logging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
					if (e.ExceptionObject is Exception exception)
					{
						Logger.Error(LogCategory.General, "Unhandled app-level fatal exception!", exception);
					}
					else
					{
						Logger.Error(LogCategory.General, "Unhandled app-level fatal exception!");
					}
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
