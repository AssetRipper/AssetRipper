using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using uTinyRipper;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipperGUI.Exporters;
using uTinyRipperGUI.Properties;
using uTinyRipperGUI.Windows;

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

			m_initialIntroText = IntroText.Text;
			m_initialStatusText = (string)StatusText.Content;

			string[] args = Environment.GetCommandLineArgs();
			string[] files = args.Skip(1).ToArray();
			ProcessInputFiles(files);
		}

		public static void OpenExplorerSelectFile(string path)
		{
			string argument = $"/e, /select, \"{path}\"";

			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "explorer";
			info.Arguments = argument;
			Process.Start(info);
		}

		// =====================================================
		// Methods
		// =====================================================

		private bool ProcessInputFiles(string[] files)
		{
			if (files.Length == 0)
			{
				return false;
			}

			foreach (string file in files)
			{
				if (MultiFileStream.Exists(file))
				{
					continue;
				}
				if (DirectoryUtils.Exists(file))
				{
					continue;
				}
				Logger.Log(LogType.Warning, LogCategory.General, MultiFileStream.IsMultiFile(file) ?
					$"File '{file}' doesn't have all parts for combining" :
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

			OnImportStarted();
#if !DEBUG
			try
#endif
			{
				GameStructure = GameStructure.Load(files);
			}
#if !DEBUG
			catch (SerializedFileException ex)
			{
				ReportCrash(ex);
				return;
			}
			catch (Exception ex)
			{
				ReportCrash(ex);
				return;
			}
#endif

			if (GameStructure.IsValid)
			{
				Validate();
			}
			OnImportFinished();

			if (GameStructure.IsValid)
			{
				Dispatcher.Invoke(() =>
					{
						IntroText.Text = "Files have been loaded";
						ExportButton.Visibility = Visibility.Visible;

						Fileview.AddItem(GameStructure.FileCollection);
						Fileview.Refresh();

#if VIRTUAL
						OnExportButtonClicked(null, null);
#endif
					}
				);
			}
			else
			{
				Dispatcher.Invoke(() =>
					{
						OnResetButtonClicked(null, null);
						Logger.Log(LogType.Warning, LogCategory.Import, "No game files found");
					}
				);
			}
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
				ReportCrash(ex);
				return;
			}
			catch (Exception ex)
			{
				ReportCrash(ex);
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
			Version[] versions = GameStructure.FileCollection.GameFiles.Values.Select(t => t.Version).Distinct().ToArray();
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
			if (!PermissionValidator.CheckAccess(directory))
			{
				string arguments = string.Join(" ", m_processingFiles.Select(t => $"\"{t}\""));
				PermissionValidator.RestartAsAdministrator(arguments);
			}

			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}

		private void ReportCrash(SerializedFileException ex)
		{
			ReportCrash(ex.ToString());
			Dispatcher.InvokeAsync(() =>
			{
				BugReportWindow window = new BugReportWindow();
				window.Owner = this;
				window.Fill(ex);
				window.ShowDialog();
			});
		}

		private void ReportCrash(Exception ex)
		{
			ReportCrash(ex.ToString());
			Dispatcher.InvokeAsync(() =>
			{
				BugReportWindow window = new BugReportWindow();
				window.Owner = this;
				window.Fill(ex);
				window.ShowDialog();
			});
		}

		private void ReportCrash(string error)
		{
			Dispatcher.Invoke(() =>
			{
				StatusText.Content = "crashed";
				Logger.Log(LogType.Error, LogCategory.Import, error);
			});
		}

		private void OpenGameFolder(string platformName)
		{
			using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				folderDialog.Description = $"Select {platformName} game folder";
				folderDialog.SelectedPath = Settings.Default.ImportFolderPath;
				System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{

				}
			}
		}

		// =====================================================
		// Callbacks
		// =====================================================

		private void OnImportStarted()
		{
			Dispatcher.Invoke(() => StatusText.Content = "importing...");
		}

		private void OnImportFinished()
		{
			Dispatcher.Invoke(() => StatusText.Content = "import finished");
		}

		private void OnExportPreparationStarted()
		{
			Dispatcher.Invoke(() => StatusText.Content = "analyzing assets...");
		}

		private void OnExportPreparationFinished()
		{
			Dispatcher.Invoke(() => StatusText.Content = "analysis finished");
		}

		private void OnExportStarted()
		{
			Dispatcher.Invoke(() =>
			{
				StatusText.Content = "exporting...";
				Progress.Value = 0.0;
				Progress.Visibility = Visibility.Visible;
			});
		}

		private void OnExportProgressUpdated(int index, int count)
		{
			Dispatcher.InvokeAsync(() =>
			{
				double progress = ((double)index / count) * 100.0;
				StatusText.Content = $"exporting... {index}/{count} - {progress:0.00}%";
				Progress.Value = progress;
			});
		}

		private void OnExportFinished()
		{
			Dispatcher.InvokeAsync(() =>
			{
				StatusText.Content = "export finished";
				Progress.Visibility = Visibility.Hidden;
			});
		}

		// =====================================================
		// Form callbacks
		// =====================================================

		private void OnOpenAndroidClicked(object sender, RoutedEventArgs e)
		{
			OpenGameFolder("Android");
		}

		private void OnResetButtonClicked(object sender, RoutedEventArgs e)
		{
			Fileview.Clear();
			IntroText.Text = m_initialIntroText;
			StatusText.Content = m_initialStatusText;
			MainGrid.AllowDrop = true;
			PostExportButton.Visibility = Visibility.Hidden;
			ResetButton.Visibility = Visibility.Hidden;
			OutputView.Clear();
			m_processingFiles = null;

			GameStructure.Dispose();
		}

		private void OnExitButtonClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnCheckForUpdateButtonClicked(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", ArchivePage);
		}

		private void OnReportBugClicked(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", MainWindow.IssuePage);
		}

		private void OnAboutButtonClicked(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", ReadMePage);
		}

		private void OnDataDroped(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				e.Handled = ProcessInputFiles(files);
			}
		}

		private void OnExportButtonClicked(object sender, RoutedEventArgs e)
		{
			while (true)
			{
				using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
				{
					folderDialog.ShowNewFolderButton = true;
					folderDialog.Description = $"Select export folder. New folder '{GameStructure.Name}' will be created inside selected one";
					folderDialog.SelectedPath = Settings.Default.ExportFolderPath;
#if VIRTUAL
					System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.OK;
#else
					System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
#endif
					if (result == System.Windows.Forms.DialogResult.OK)
					{
						string path = Path.Combine(folderDialog.SelectedPath, GameStructure.Name);
						if (File.Exists(path))
						{
							MessageBox.Show(this, "Unable to export assets into selected folder. Choose another one.",
									"Invalid folder", MessageBoxButton.OK, MessageBoxImage.Warning);
							continue;
						}
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
						Settings.Default.ExportFolderPath = folderDialog.SelectedPath;
						Settings.Default.Save();

						ThreadPool.QueueUserWorkItem(new WaitCallback(ExportFiles), path);
					}
				}
				break;
			}
		}

		private void OnPostExportButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenExplorerSelectFile(m_exportPath);
		}

		// =====================================================
		// Properties
		// =====================================================

		private GameStructure GameStructure
		{
			get => m_gameStructure;
			set
			{
				if (m_gameStructure == value)
				{
					return;
				}
				if (m_gameStructure != null && m_gameStructure.IsValid)
				{
					m_gameStructure.FileCollection.Exporter.EventExportFinished -= OnExportFinished;
					m_gameStructure.FileCollection.Exporter.EventExportProgressUpdated -= OnExportProgressUpdated;
					m_gameStructure.FileCollection.Exporter.EventExportStarted -= OnExportStarted;
					m_gameStructure.FileCollection.Exporter.EventExportPreparationFinished -= OnExportPreparationFinished;
					m_gameStructure.FileCollection.Exporter.EventExportPreparationStarted -= OnExportPreparationStarted;
				}
				m_gameStructure = value;
				if (value != null && value.IsValid)
				{
					value.FileCollection.Exporter.EventExportPreparationStarted += OnExportPreparationStarted;
					value.FileCollection.Exporter.EventExportPreparationFinished += OnExportPreparationFinished;
					value.FileCollection.Exporter.EventExportStarted += OnExportStarted;
					value.FileCollection.Exporter.EventExportProgressUpdated += OnExportProgressUpdated;
					value.FileCollection.Exporter.EventExportFinished += OnExportFinished;
				}
			}
		}

		public const string RepositoryPage = "https://github.com/mafaca/UtinyRipper/";
		public const string ReadMePage = MainWindow.RepositoryPage + "blob/master/README.md";
		public const string IssuePage = MainWindow.RepositoryPage + "issues/new";
		public const string ArchivePage = "https://sourceforge.net/projects/utinyripper/files/";

		private GameStructure m_gameStructure;
		private readonly string m_initialIntroText;
		private readonly string m_initialStatusText;
		private string m_exportPath;
		private string[] m_processingFiles;
	}
}
