﻿using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.GUI.AssetInfo;
using AssetRipper.GUI.Exceptions;
using AssetRipper.GUI.Extensions;
using AssetRipper.GUI.Json;
using AssetRipper.GUI.Logging;
using AssetRipper.GUI.Managers;
using AssetRipper.Library;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.GUI
{
	public partial class MainWindowViewModel : BaseViewModel
	{
		private const string websiteURL = "https://ds5678.github.io/AssetRipper/";
		
		//Exposed-to-ui properties
		private bool _hasFile;
		private bool _hasLoaded;
		private bool _isExporting;
		private string? _loadingText;
		private string _logText = "";
		private string _exportingText = "";
		private SelectedAsset? _selectedAsset;

		public ObservableCollection<NewUiFileListItem> AssetFiles { get; } = new();

		//Not-exposed-to-UI properties
		private string? _lastExportPath;
		private readonly Ripper _ripper = new();
		private UIAssetContainer? _assetContainer;
		private HttpClient client = new HttpClient();
		private string? _importingFrom;
		private bool _updatingLoadingText = false;

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

		public SelectedAsset? SelectedAsset
		{
			get => _selectedAsset;
			set
			{
				_selectedAsset = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
			Logger.Add(new ViewModelLogger(this));
			Logger.Add(new FileLogger());
			Logger.LogSystemInformation("AssetRipper GUI Version");

			Logger.OnStatusChanged += OnImportStatusUpdated;
			
			ProductInfoHeaderValue product = new (BuildInfo.Name, BuildInfo.Version);
			ProductInfoHeaderValue comment = new ($"(+{websiteURL})");
			client.DefaultRequestHeaders.UserAgent.Add(product);
			client.DefaultRequestHeaders.UserAgent.Add(comment);
			
			OnPropertyChanged(nameof(AudioExportFormat));
		}

		private void UpdateGamePathInUi(string path)
		{
			LoadingText = $"Loading Game Content from {path}...";
			_importingFrom = path;
		}
		
		private void OnImportStatusUpdated(string newStatus)
		{
			// Logger.Log(LogType.Info, LogCategory.System, $"{newStatus} at {DateTime.Now:hh:mm:ss.fff}");

			if(_updatingLoadingText)
				return;

			_updatingLoadingText = true;
			LoadingText = $"Loading Game Content from {_importingFrom}...\n{newStatus}...";
			_updatingLoadingText = false;
		}

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

			_ripper.ResetData();
			SelectedAsset?.Dispose();
			SelectedAsset = null;
			_assetContainer = null;

			string gamePath = filesDropped[0];

			HasFile = true;
			HasLoaded = false;
			Dispatcher.UIThread.Post(() => AssetFiles.Clear(), DispatcherPriority.Send);

			UpdateGamePathInUi(gamePath);

			UIImportManager.ImportFromPath(_ripper, filesDropped, gameStructure =>
			{
				HasLoaded = true;
				_assetContainer = new UIAssetContainer(_ripper);

				Dispatcher.UIThread.Post(() =>
				{
					List<NewUiFileListItem> items = UIFileListing.GetItemsFromStructure(gameStructure);
					items.ForEach(AssetFiles.Add);
				}, DispatcherPriority.Send);
			}, error =>
			{
				HasFile = false;
				HasLoaded = false;

				Logger.Error(error);

				if (error is GameNotFoundException)
				{
					this.ShowPopup($"No Unity game was found in the dropped files.", "Error");
					return;
				}

				this.ShowPopup($"Failed to load game content: {error.Message}", "Error");
			});
		}

		public async void ExportAll()
		{
			if (_ripper.GameStructure == null)
			{
				return;
			}

			OpenFolderDialog openFolderDialog = new();

			string? chosenFolder = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (string.IsNullOrEmpty(chosenFolder))
			{
				return;
			}

			IsExporting = true;
			ExportingText = "Clearing out existing files...";

			string exportPath = Path.Combine(chosenFolder, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);
			UIExportManager.ConfigureExportEvents(_ripper.GameStructure.FileCollection.Exporter, this);

			UIExportManager.Export(_ripper, exportPath, () =>
			{
				IsExporting = false;
				this.ShowPopup("Export Complete!", "Success!");
				Logger.Info(LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				Logger.Error(error);
				this.ShowPopup($"Failed to export game content: {error.Message}", "Error");
			});
		}

		public async void ExportSelectedAssetToProject()
		{
			if (_ripper.GameStructure == null || SelectedAsset == null)
			{
				return;
			}

			OpenFolderDialog openFolderDialog = new();

			string? chosenFolder = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (string.IsNullOrEmpty(chosenFolder))
			{
				return;
			}

			IsExporting = true;
			ExportingText = "Clearing out existing files...";

			string exportPath = Path.Combine(chosenFolder, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);
			UIExportManager.ConfigureExportEvents(_ripper.GameStructure.FileCollection.Exporter, this);
			
			UIExportManager.Export(_ripper, exportPath, SelectedAsset.Asset, () =>
			{
				IsExporting = false;
				this.ShowPopup("Export Complete!", "Success!");
				Logger.Info(LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				Logger.Error(error);
				this.ShowPopup($"Failed to export game content: {error.Message}", "Error");
			});
		}

		//Called from UI
		public async void ShowOpenFileDialog()
		{
			OpenFileDialog openFileDialog = new();
			openFileDialog.AllowMultiple = true;
			string[] result = await openFileDialog.ShowAsync(MainWindow.Instance);

			if (result == null)
				return;

			DoLoad(result);
		}

		//Called from UI
		public async void ShowOpenFolderDialog()
		{
			OpenFolderDialog openFolderDialog = new();
			string result = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (string.IsNullOrEmpty(result))
				return;

			DoLoad(new[] { result });
		}

		//Called from UI
		public void Reset()
		{
			if(!HasFile)
				return;
			
			_ripper.ResetData();
			AssetFiles.Clear();
			_assetContainer = null;
			HasFile = false;
			HasLoaded = false;
			IsExporting = false;
			
			SelectedAsset?.Dispose();
			SelectedAsset = null;
			
			LogText = "";
			Logger.Log(LogType.Info, LogCategory.General, "UI Reset");
		}

		//Called from UI indirectly
		public void OnAssetSelected(NewUiFileListItem selectedItem, Object selectedAsset)
		{
			_assetContainer.LastAccessedAsset = selectedAsset;

			SelectedAsset?.Dispose();
			SelectedAsset = new(selectedAsset, _assetContainer);
		}
		
		// Called from UI
		private async void CheckforUpdates()
		{
			const string url = "https://api.github.com/repos/ds5678/AssetRipper/releases";
			List<GithubRelease> releases = await client.GetFromJsonAsync<List<GithubRelease>>(url);

			if (releases == null)
			{
				return;
			}

			Version release = Version.Parse(releases[0].TagName);
			Version current = Version.Parse(BuildInfo.Version);
			
			if (release > current)
			{
				MessageBox.Popup(
					"Update Available", 
					$"Update {release} is available.\nDo you want to open the releases page?",
					MessageBoxViewModel.Buttons.YesNo,
					UpdatePopupClosed);
			}
			else
			{
				MessageBox.Popup("No Updates Available",$"You are already on the latest version ({current})");
			}
			
		}

		private void UpdatePopupClosed(MessageBoxViewModel.Result result)
		{
			if (result == MessageBoxViewModel.Result.Yes)
			{
				OpenUrl("https://github.com/ds5678/AssetRipper/releases");
			}
		}

		// Called from UI
		private void GithubClicked() => OpenUrl("https://github.com/ds5678/AssetRipper");

		// Called from UI
		private void WebsiteClicked() => OpenUrl(websiteURL);
		
		private static void OpenUrl(string url) =>
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
	}
}