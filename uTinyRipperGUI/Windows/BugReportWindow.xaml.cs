using System;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using uTinyRipper.SerializedFiles;

namespace uTinyRipperGUI.Windows
{
	/// <summary>
	/// Interaction logic for BugReportWindow.xaml
	/// </summary>
	public partial class BugReportWindow : Window
	{
		public BugReportWindow()
		{
			InitializeComponent();

			FilePanel.Children.Clear();
		}

		public void Fill(SerializedFileException ex)
		{
			string innerMessage = ex.InnerException?.ToString();
			Fill(DefaultGameName, ex.Version.ToString(), ex.Platform.ToString(), ex.FileName, ex.Message, innerMessage, ex.StackTrace);
			AddHyperlink(ex.FileName, ex.FilePath);
			SystemSounds.Beep.Play();
		}

		public void Fill(Exception ex)
		{
			Fill(DefaultGameName, DefaultVersion, DefaultPlatform, null, ex.Message, null, ex.StackTrace);
			SystemSounds.Beep.Play();
		}

		private void Fill(string gameName, string version, string platform, string fileName, string message, string innerMessage, string stackTrace)
		{
			Textbox.Clear();

			Textbox.AppendText("Game name: ");
			Textbox.AppendText(gameName);
			Textbox.AppendText(Environment.NewLine);

			Textbox.AppendText("Engine version: ");
			Textbox.AppendText(version.ToString());
			Textbox.AppendText(Environment.NewLine);

			Textbox.AppendText("Platform: ");
			Textbox.AppendText(platform.ToString());
			Textbox.AppendText(Environment.NewLine);
			
			if (fileName != null)
			{
				Textbox.AppendText("File name: ");
				Textbox.AppendText(fileName.ToString());
				Textbox.AppendText(Environment.NewLine);
			}
			Textbox.AppendText(Environment.NewLine);

			Textbox.AppendText("Error message: ");
			Textbox.AppendText(message);
			Textbox.AppendText(Environment.NewLine);
			Textbox.AppendText(Environment.NewLine);

			if (innerMessage != null)
			{
				Textbox.AppendText("Inner message: ");
				Textbox.AppendText(innerMessage);
				Textbox.AppendText(Environment.NewLine);
				Textbox.AppendText(Environment.NewLine);
			}

			Textbox.AppendText("Stack trace: ");
			Textbox.AppendText(stackTrace);
			Textbox.AppendText(Environment.NewLine);
			Textbox.AppendText(" ");
		}

		private void AddHyperlink(string fileName, string filePath)
		{
			FileGrid.Visibility = Visibility.Visible;
			Hyperlink hyperlink = new Hyperlink();
			hyperlink.Inlines.Add(fileName);
			hyperlink.IsEnabled = true;
			hyperlink.NavigateUri = new Uri(filePath);
			hyperlink.RequestNavigate += OnHyperlinkClicked;
			TextBlock block = new TextBlock(hyperlink);
			FilePanel.Children.Add(block);
		}

		private void OnHyperlinkClicked(object sender, RequestNavigateEventArgs e)
		{
			MainWindow.OpenExplorerSelectFile(e.Uri.ToString());
		}

		private void OnCopyButtonClicked(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Textbox.Text);
		}

		private void OnReportButtonClicked(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", MainWindow.IssuePage);
		}


		private const string DefaultGameName = "(please, specify the game name)";
		private const string DefaultVersion = "(specify engine version, if known)";
		private const string DefaultPlatform = "(specify platform, if known)";
	}
}
