using System.Windows;
using uTinyRipper;

namespace uTinyRipperGUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Config.IsAdvancedLog = true;
			Config.IsGenerateGUIDByContent = false;
			Config.IsExportDependencies = false;
		}
	}
}
