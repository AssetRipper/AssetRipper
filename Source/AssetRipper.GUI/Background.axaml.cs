using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AssetRipper.GUI
{
	public partial class Background : UserControl
	{
		public Background()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
