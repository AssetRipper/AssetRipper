using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
#if !DEBUG
using uTinyRipper.SerializedFiles;
#endif
using uTinyRipperGUI.Exporters;

using Object = uTinyRipper.Classes.Object;
using Version = uTinyRipper.Version;

namespace uTinyRipperGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static bool AssetSelector(Object asset)
		{
			return true;
		}

		public MainWindow()
		{
			InitializeComponent();

			Logger.Instance = new OutputLogger(OutputTextBox);

			m_initialText = IntroText.Text;

			string[] args = Environment.GetCommandLineArgs();
			string[] files = args.Skip(1).ToArray();
			ProcessInputFiles(files);
		}

		// =====================================================
		// Callbacks
		// =====================================================

		private void OnDataDroped(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				
				e.Handled = ProcessInputFiles(files);
			}
		}
		
		private void OnExportButtonClicked(object sender, RoutedEventArgs e)
		{
			while(true)
			{
				System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
				folderDialog.ShowNewFolderButton = true;
				folderDialog.Description = $"Select export folder. New folder '{GameStructure.Name}' will be created inside selected one";
				System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					string path = Path.Combine(folderDialog.SelectedPath, GameStructure.Name);
					if (Directory.Exists(path))
					{
						if (Directory.EnumerateFiles(path).Any())
						{
							MessageBoxResult mbresult = MessageBox.Show(this, "There are files inside selected folder. They will be deleted.",
								"Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
							if (mbresult == MessageBoxResult.Cancel)
							{
								continue;
							}
						}
					}

					IntroText.Text = "Exporting assets...";
					ExportButton.Visibility = Visibility.Hidden;

					ThreadPool.QueueUserWorkItem(new WaitCallback(ExportFiles), path);
				}
				break;
			}
		}

		private void OnPostExportButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenExplorerSelectFile(m_exportPath);
		}

		private void OnResetButtonClicked(object sender, RoutedEventArgs e)
		{
			IntroText.Text = m_initialText;
			MainGrid.AllowDrop = true;
			PostExportButton.Visibility = Visibility.Hidden;
			ResetButton.Visibility = Visibility.Hidden;

			GameStructure.Dispose();
		}

		private void OnOutputTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			OutputTextBox.ScrollToEnd();
		}

		private void OnHyperlinkClicked(object sender, RequestNavigateEventArgs e)
		{
			if(e.Uri.IsFile)
			{
				OpenExplorerSelectFile(e.Uri.ToString());
			}
			else
			{
				Process.Start("explorer.exe", e.Uri.ToString());
			}
		}

		// =====================================================
		// Visualization
		// =====================================================

		private void AddHyperlinkText(string message, string linkName, string linkURL)
		{
			TextRange rangeOfText = new TextRange(OutputTextBox.Document.ContentEnd, OutputTextBox.Document.ContentEnd);
			rangeOfText.Text = message;
			rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
			rangeOfText.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);

			rangeOfText = new TextRange(OutputTextBox.Document.ContentEnd, OutputTextBox.Document.ContentEnd);
			rangeOfText.Text = linkName;
			rangeOfText.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);

			Hyperlink link = new Hyperlink(rangeOfText.Start, rangeOfText.End);
			link.IsEnabled = true;
			link.NavigateUri = new Uri(linkURL);
			link.RequestNavigate += OnHyperlinkClicked;

			rangeOfText = new TextRange(OutputTextBox.Document.ContentEnd, OutputTextBox.Document.ContentEnd);
			rangeOfText.Text = "\r";
		}

		// =====================================================
		// Methods
		// =====================================================

		private bool ProcessInputFiles(string[] files)
		{
			if(files.Length == 0)
			{
				return false;
			}

			foreach (string file in files)
			{
				if (FileMultiStream.Exists(file))
				{
					continue;
				}
				if (DirectoryUtils.Exists(file))
				{
					continue;
				}
				Logger.Instance.Log(LogType.Warning, LogCategory.General, FileMultiStream.IsMultiFile(file) ?
					$"File '{file}' doesn't has all parts for combining" :
					$"Neither file nor directory with path '{file}' exists");
				return false;
			}

			IntroText.Text = "Loading files...";
			MainGrid.AllowDrop = false;

			ThreadPool.QueueUserWorkItem(new WaitCallback(LoadFiles), files);
			return true;
		}

		private void LoadFiles(object data)
		{
			string[] files = (string[])data;

#if !DEBUG
			try
#endif
			{
				GameStructure = GameStructure.Load(files);
				Validate();
			}
#if !DEBUG
			catch(SerializedFileException ex)
			{
				Dispatcher.Invoke(() =>
				{
					Logger.Instance.Log(LogType.Error, LogCategory.Import, ex.ToString());
					AddHyperlinkText("Go to: ", "Create issue", IssuePage);
					AddHyperlinkText("Attach file: ", ex.FileName, ex.FilePath);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file '{ex.FilePath}'.",
						"An error during loading process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
			catch(Exception ex)
			{
				Dispatcher.Invoke(() =>
				{
					Logger.Instance.Log(LogType.Error, LogCategory.Import, ex.ToString());
					AddHyperlinkText("Go to: ", "Create issue", IssuePage);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file that cause this error.",
						"An error during loading process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
#endif

			Dispatcher.Invoke(() =>
				{
					IntroText.Text = "Files has been loaded";
					ExportButton.Visibility = Visibility.Visible;
				}
			);
		}

		private void ExportFiles(object data)
		{
			m_exportPath = (string)data;
			PrepareExportDirectory(m_exportPath);

			TextureAssetExporter textureExporter = new TextureAssetExporter();
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, new ShaderAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());

			EngineAssetExporter engineExporter = new EngineAssetExporter();
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);

#if !DEBUG
			try
#endif
			{
				GameStructure.Export(m_exportPath, AssetSelector);
			}
#if !DEBUG
			catch (SerializedFileException ex)
			{
				Logger.Instance.Log(LogType.Error, LogCategory.Import, ex.ToString());
				Dispatcher.Invoke(() =>
				{
					AddHyperlinkText("Go to: ", "Create issue", IssuePage);
					AddHyperlinkText("Attach file: ", ex.FileName, ex.FilePath);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file '{ex.FilePath}'.",
						"An error during export process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
			catch (Exception ex)
			{
				Logger.Instance.Log(LogType.Error, LogCategory.Import, ex.ToString());
				Dispatcher.Invoke(() =>
				{
					AddHyperlinkText("Go to: ", "Create issue", IssuePage);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file that cause this error.",
						"An error during loading process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
#endif
			Logger.Instance.Log(LogType.Info, LogCategory.General, "Finished!!!");

			Dispatcher.Invoke(() =>
				{
					IntroText.Text = "Export is finished";
					ExportButton.Visibility = Visibility.Hidden;
					PostExportButton.Visibility = Visibility.Visible;
					ResetButton.Visibility = Visibility.Visible;
				}
			);
		}

		private void Validate()
		{
			Version[] versions = GameStructure.FileCollection.Files.Select(t => t.Version).Distinct().ToArray();
			if (versions.Length > 1)
			{
				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
				foreach (Version version in versions)
				{
					Logger.Instance.Log(LogType.Warning, LogCategory.Import, version.ToString());
				}
			}
		}

		private static void PrepareExportDirectory(string path)
		{
			string directory = Directory.GetCurrentDirectory();
			PermissionValidator.CheckWritePermission(directory);

			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}

		private static void OpenExplorerSelectFile(string path)
		{
			string argument = $"/e, /select, \"{path}\"";

			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "explorer";
			info.Arguments = argument;
			Process.Start(info);
		}

		private GameStructure GameStructure { get; set; }

		private const string IssuePage = "https://github.com/mafaca/UtinyRipper/issues";

		private string m_initialText;
		private string m_exportPath;
	}
}
