using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.GUI.Exceptions;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AssetRipper.GUI
{
	public class MainWindowViewModel : BaseViewModel
	{
		//Exposed-to-ui properties
		private bool _hasFile;
		private bool _hasLoaded;
		private bool _isExporting;
		private string? _loadingText;
		private string _logText = "";
		private string _exportingText = "";

		public ObservableCollection<NewUiFileListItem> AssetFiles { get; } = new();

		//Not-exposed-to-UI properties
		private GameStructure? _theStructure;
		private string? _lastExportPath;

		public bool HasFile
		{
			get => _hasFile;
			set
			{
				_hasFile = value;
				OnPropertyChanged();
			}
		}

		public bool HasLoaded
		{
			get => _hasLoaded;
			set
			{
				_hasLoaded = value;
				OnPropertyChanged();
			}
		}

		public bool IsExporting
		{
			get => _isExporting;
			set
			{
				_isExporting = value;
				OnPropertyChanged();
			}
		}

		public string? LoadingText
		{
			get => _loadingText;
			set
			{
				_loadingText = value;
				OnPropertyChanged();
			}
		}

		public string LogText
		{
			get => _logText;
			set
			{
				_logText = value;
				OnPropertyChanged();
			}
		}

		public string ExportingText
		{
			get => _exportingText;
			set
			{
				_exportingText = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
			Logger.Add(new ViewModelLogger(this));
			Logger.Add(new FileLogger());
			Logger.LogSystemInformation("AssetRipper GUI Version");
		}

		private void UpdateGamePathInUi(string path) => LoadingText = $"Loading Game Content from {path}...";

		public void OnFileDropped(DragEventArgs e)
		{
			if (IsExporting || (HasFile && !HasLoaded))
				return;

			string[]? filesDropped = e.Data.GetFileNames()?.ToArray();

			DoLoad(filesDropped);
		}

		private void DoLoad(string[]? filesDropped)
		{
			if (filesDropped == null || filesDropped.Length < 1)
			{
				return;
			}

			string gamePath = filesDropped[0];

			HasFile = true;
			HasLoaded = false;
			AssetFiles.Clear();

			UpdateGamePathInUi(gamePath);

			NewUiImportManager.ImportFromPath(filesDropped, gameStructure =>
			{
				HasLoaded = true;
				_theStructure = gameStructure;
				List<NewUiFileListItem> items = NewUiFileListing.GetItemsFromStructure(gameStructure);
				items.ForEach(AssetFiles.Add);
			}, error =>
			{
				HasFile = false;
				HasLoaded = false;

				if (error is NewUiGameNotFoundException)
				{
					this.ShowPopup($"No Unity game was found in the dropped files.", "Error");
					return;
				}

				this.ShowPopup($"Failed to load game content: {error.Message}", "Error");
			});
		}

		public async void ExportAll()
		{
			if (_theStructure == null)
			{
				return;
			}

			OpenFolderDialog openFolderDialog = new();

			string? chosenFolder = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (chosenFolder == null)
			{
				return;
			}

			IsExporting = true;
			ExportingText = "Clearing out existing files...";

			string exportPath = Path.Combine(chosenFolder, _theStructure.Name ?? "AssetRipperExport" + DateTime.Now.Ticks);
			_lastExportPath = exportPath;

			Logger.Log(LogType.Info, LogCategory.General, $"About to begin export to {exportPath}");

			ProjectExporter exporter = _theStructure.FileCollection.Exporter;

			Logger.Log(LogType.Info, LogCategory.General, $"Removing any files from a previous export...");

			await NewUiExportManager.PrepareExportDirectory(exportPath);
			NewUiExportManager.ConfigureExportEvents(exporter, this);

			NewUiExportManager.Export(_theStructure, exportPath, () =>
			{
				IsExporting = false;
				this.ShowPopup("Export Complete!", "Success!");
				Logger.Log(LogType.Info, LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				this.ShowPopup($"Failed to export game content: {error.Message}", "Error");
			});
		}

		public async void ShowOpenFileDialog()
		{
			OpenFolderDialog openFolderDialog = new();
			string result = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (result == null)
				return;

			DoLoad(new[] { result });
		}
	}
}