using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using uTinyRipper;
using uTinyRipper.AssetExporters;
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
			return asset.ClassID == ClassIDType.Mesh;
		}

		public MainWindow()
		{
			InitializeComponent();

			Logger.Instance = new OutputLogger(OutputTextBox);

			m_initialText = IntroText.Text;
		}

		// =====================================================
		// Callbacks
		// =====================================================

		private void OnDataDroped(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

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
					return;
				}

				ThreadPool.QueueUserWorkItem(new WaitCallback(LoadFiles), files);

				IntroText.Text = "Loading files...";
				MainGrid.AllowDrop = false;

				e.Handled = true;
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
			string argument = $"/e, /select, \"{m_exportPath}\"";

			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "explorer";
			info.Arguments = argument;
			Process.Start(info);
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

		// =====================================================
		// Methods
		// =====================================================

		private void LoadFiles(object data)
		{
			string[] files = (string[])data;

			GameStructure = GameStructure.Load(files);
			Validate();

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

			GameStructure.Export(m_exportPath, AssetSelector);
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

		private GameStructure GameStructure { get; set; }

		private string m_initialText;
		private string m_exportPath;
	}
}
