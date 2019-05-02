using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;
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

			m_initialIntroText = IntroText.Text;
			m_initialStatusText = StatusText.Text;
			m_outputContentStart = new TextRange(OutputTextBox.Document.Blocks.FirstBlock.ElementEnd, OutputTextBox.Document.Blocks.FirstBlock.ElementEnd);

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
#if VIRTUAL
				System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.OK;
#else
				System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
#endif
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
			IntroText.Text = m_initialIntroText;
			StatusText.Text = m_initialStatusText;
			MainGrid.AllowDrop = true;
			PostExportButton.Visibility = Visibility.Hidden;
			ResetButton.Visibility = Visibility.Hidden;
			m_processingFiles = null;

			GameStructure.Dispose();
		}

		private void OnOutputTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (OutputTextBox.VerticalOffset == 0.0f && OutputTextBox.ViewportHeight >= OutputTextBox.ExtentHeight ||
				OutputTextBox.VerticalOffset + OutputTextBox.ViewportHeight == OutputTextBox.ExtentHeight)
			{
				OutputTextBox.ScrollToEnd();
			}
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

		private void OnClearOutputTextClicked(object sender, RoutedEventArgs e)
		{
			ClearConsole();
		}

		// =====================================================
		// Visualization
		// =====================================================

		private void AddHyperlinkToConsole(string message, string linkName, string linkURL)
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

		private void ClearConsole()
		{
			TextRange range = new TextRange(m_outputContentStart.Start, OutputTextBox.Document.ContentEnd);
			range.Text = string.Empty;
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
				Logger.Log(LogType.Warning, LogCategory.General, FileMultiStream.IsMultiFile(file) ?
					$"File '{file}' doesn't has all parts for combining" :
					$"Neither file nor directory with path '{file}' exists");
				return false;
			}

			IntroText.Text = "Loading files...";
			MainGrid.AllowDrop = false;
			m_processingFiles = files;

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
				OnImportStarted();
				GameStructure = GameStructure.Load(files);
				Validate();
				OnImportFinished();
			}
#if !DEBUG
			catch(SerializedFileException ex)
			{
				Dispatcher.Invoke(() =>
				{
					Logger.Log(LogType.Error, LogCategory.Import, ex.ToString());
					AddHyperlinkToConsole("Go to: ", "Create issue", IssuePage);
					AddHyperlinkToConsole("Attach file: ", ex.FileName, ex.FilePath);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file '{ex.FilePath}'.",
						"An error during loading process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
			catch(Exception ex)
			{
				Dispatcher.Invoke(() =>
				{
					Logger.Log(LogType.Error, LogCategory.Import, ex.ToString());
					AddHyperlinkToConsole("Go to: ", "Create issue", IssuePage);
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

#if VIRTUAL
					ButtonAutomationPeer peer = new ButtonAutomationPeer(ExportButton);
					IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
					invokeProv.Invoke();
#endif
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
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

			EngineAssetExporter engineExporter = new EngineAssetExporter();
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);

#if !DEBUG
			try
#endif
			{
				GameStructure.Export(m_exportPath, AssetSelector);
			}
#if !DEBUG
			catch (SerializedFileException ex)
			{
				Logger.Log(LogType.Error, LogCategory.Import, ex.ToString());
				Dispatcher.Invoke(() =>
				{
					AddHyperlinkToConsole("Go to: ", "Create issue", IssuePage);
					AddHyperlinkToConsole("Attach file: ", ex.FileName, ex.FilePath);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file '{ex.FilePath}'.",
						"An error during export process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.Import, ex.ToString());
				Dispatcher.Invoke(() =>
				{
					AddHyperlinkToConsole("Go to: ", "Create issue", IssuePage);
					MessageBox.Show(this, $"Please, create an issue on github page {IssuePage} with attached file that cause this error.",
						"An error during loading process has occuered!", MessageBoxButton.OK, MessageBoxImage.Error);
				});
				return;
			}
#endif
			Logger.Log(LogType.Info, LogCategory.General, "Finished!!!");

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
				Logger.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
				foreach (Version version in versions)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, version.ToString());
				}
			}
		}

		private void PrepareExportDirectory(string path)
		{
			string directory = Directory.GetCurrentDirectory();
			if(!PermissionValidator.CheckAccess(directory))
			{
				string arguments = string.Join(" ", m_processingFiles.Select(t => $"\"{t}\""));
				PermissionValidator.RestartAsAdministrator(arguments);
			}

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

		private void OnImportStarted()
		{
			Dispatcher.Invoke(() => StatusText.Text = "Status: importing...");
		}

		private void OnImportFinished()
		{
			Dispatcher.Invoke(() => StatusText.Text = "Status: import finished");
		}

		private void OnExportPreparationStarted()
		{
			Dispatcher.Invoke(() => StatusText.Text = "Status: analyzing assets...");
		}

		private void OnExportPreparationFinished()
		{
			Dispatcher.Invoke(() => StatusText.Text = "Status: analysis finished");
		}

		private void OnExportStarted()
		{
			Dispatcher.Invoke(() => StatusText.Text = "Status: exporting...");
		}

		private void OnExportProgressUpdated(int index, int count)
		{
			Dispatcher.InvokeAsync(() => StatusText.Text = $"Status: exporting... {index}/{count} - {((float)index / (float)count) * 100.0f:0.00}%");
		}

		private void OnExportFinished()
		{
			Dispatcher.InvokeAsync(() => StatusText.Text = "Status: export finished");
		}

		private GameStructure GameStructure
		{
			get => m_gameStructure;
			set
			{
				if (m_gameStructure == value)
				{
					return;
				}
				if (m_gameStructure != null)
				{
					m_gameStructure.FileCollection.Exporter.EventExportFinished -= OnExportFinished;
					m_gameStructure.FileCollection.Exporter.EventExportProgressUpdated -= OnExportProgressUpdated;
					m_gameStructure.FileCollection.Exporter.EventExportStarted -= OnExportStarted;
					m_gameStructure.FileCollection.Exporter.EventExportPreparationFinished -= OnExportPreparationFinished;
					m_gameStructure.FileCollection.Exporter.EventExportPreparationStarted -= OnExportPreparationStarted;
				}
				m_gameStructure = value;
				if (value != null)
				{
					value.FileCollection.Exporter.EventExportPreparationStarted += OnExportPreparationStarted;
					value.FileCollection.Exporter.EventExportPreparationFinished += OnExportPreparationFinished;
					value.FileCollection.Exporter.EventExportStarted += OnExportStarted;
					value.FileCollection.Exporter.EventExportProgressUpdated += OnExportProgressUpdated;
					value.FileCollection.Exporter.EventExportFinished += OnExportFinished;
				}
			}
		}

		private const string IssuePage = "https://github.com/mafaca/UtinyRipper/issues";

		private GameStructure m_gameStructure;
		private string m_initialIntroText;
		private string m_initialStatusText;
		private TextRange m_outputContentStart;
		private string m_exportPath;
		private string[] m_processingFiles;
	}
}
