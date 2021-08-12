using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AssetRipperGuiNew
{
	public class App : Application
	{

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public static void AppMain(Application app, string[] args)
		{
			app.Run(new MainWindow { DataContext = new MainWindowViewModel() });
		}
	}
}