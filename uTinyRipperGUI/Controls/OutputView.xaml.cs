using System.Windows.Controls;
using uTinyRipper;

namespace uTinyRipperGUI.Controls
{
	/// <summary>
	/// Interaction logic for OutputView.xaml
	/// </summary>
	public partial class OutputView : UserControl
	{
		public OutputView()
		{
			InitializeComponent();

			Logger.Instance = new OutputLogger(Textbox);
		}

		public void Clear()
		{
			Textbox.Clear();
		}

		private void OnOutputTextChanged(object sender, TextChangedEventArgs e)
		{
			if (Textbox.VerticalOffset == 0.0f && Textbox.ViewportHeight >= Textbox.ExtentHeight ||
				   Textbox.VerticalOffset + Textbox.ViewportHeight == Textbox.ExtentHeight)
			{
				Textbox.ScrollToEnd();
			}
		}

		private void OnClearOutputTextClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			Clear();
		}
	}
}
