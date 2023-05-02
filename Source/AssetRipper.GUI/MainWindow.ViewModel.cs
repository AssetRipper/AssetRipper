using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;
using AssetRipper.GUI.AssetInformation;
using AssetRipper.GUI.Exceptions;
using AssetRipper.GUI.Extensions;
using AssetRipper.GUI.Managers;
using AssetRipper.Import.Logging;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AssetRipper.GUI
{
	public partial class MainWindowViewModel : BaseViewModel
	{

		//Exposed-to-ui properties
		private bool _hasFile;
		private bool _hasLoaded;
		private bool _isExporting;
		private string? _loadingText;
		private string? _unityVersionText;
		private string _logText = "";
		private string _exportingText = "";
		private SelectedAsset? _selectedAsset;

		public ObservableCollection<NewUiFileListItem> AssetFiles { get; } = new();

		//Not-exposed-to-UI properties
		private string? _lastExportPath;
		private readonly Ripper _ripper = new();
		private UIAssetContainer? _assetContainer;
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

		public string? UnityVersion
		{
			get => _unityVersionText;
			set
			{
				_unityVersionText = value?.Split("f")?[0];
				OnPropertyChanged(nameof(UnityVersionText));
			}
		}
		public string? UnityVersionText
		{
			get
			{
				string? version = UnityVersion;

				if (version == null)
				{
					return null;
				}

				return "Unity " + version;
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

		public LocalizationManager LocalizationManager => MainWindow.Instance.LocalizationManager;

		public MainWindowViewModel()
		{
			Logger.OnStatusChanged += OnImportStatusUpdated;

			OnPropertyChanged(nameof(AudioExportFormat));
		}

		private void UpdateGamePathInUi(string path)
		{
			LoadingText = string.Format(MainWindow.Instance.LocalizationManager["loading_game_content_from"], _importingFrom, "");
			_importingFrom = path;
		}

		private void OnImportStatusUpdated(string statusKey, object? context)
		{
			// Logger.Log(LogType.Info, LogCategory.System, $"{newStatus} at {DateTime.Now:hh:mm:ss.fff}");

			if (_updatingLoadingText)
			{
				return;
			}

			string newStatus = context == null ? MainWindow.Instance.LocalizationManager[statusKey] : string.Format(MainWindow.Instance.LocalizationManager[statusKey], context);

			_updatingLoadingText = true;
			LoadingText = string.Format(MainWindow.Instance.LocalizationManager["loading_game_content_from"], _importingFrom, newStatus);
			_updatingLoadingText = false;
		}

		public void OnFileDropped(DragEventArgs e)
		{
			if (IsExporting || (HasFile && !HasLoaded))
			{
				return;
			}

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

			_ripper.Settings.LogConfigurationValues();

			UpdateGamePathInUi(gamePath);

			UIImportManager.ImportFromPath(_ripper, filesDropped, gameStructure =>
			{
				HasLoaded = true;
				UnityVersion = _ripper.GameStructure.FileCollection.FetchAssetCollections().Max(t => t.Version).ToString();
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
					this.ShowPopup(MainWindow.Instance.LocalizationManager["no_game_files_found"], MainWindow.Instance.LocalizationManager["error"]);
				}
				else
				{
					//this.ShowPopup(string.Format(MainWindow.Instance.LocalizationManager["error_importing_with_reason"], error.Message), MainWindow.Instance.LocalizationManager["error"]);
					this.ShowPopup(MainWindow.Instance.LocalizationManager["check_log_for_more_details"], MainWindow.Instance.LocalizationManager["error"]);
				}
			});
		}

		public async void ExportAll()
		{
			if (!_ripper.IsLoaded)
			{
				return;
			}

			FolderPickerOpenOptions options = new() { AllowMultiple = false };
			IReadOnlyList<IStorageFolder> folderList = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(options);
			if (folderList.Count == 0)
			{
				return;
			}

			IsExporting = true;
			ExportingText = "Clearing out existing files...";

			string exportPath = Path.Combine(folderList[0].Path.LocalPath, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);

			UIExportManager.Export(_ripper, this, exportPath, () =>
			{
				IsExporting = false;
				this.ShowPopup(MainWindow.Instance.LocalizationManager["export_complete"], MainWindow.Instance.LocalizationManager["success"]);
				Logger.Info(LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				Logger.Error(error);
				//this.ShowPopup(string.Format(MainWindow.Instance.LocalizationManager["error_exporting_with_reason"], error.Message), MainWindow.Instance.LocalizationManager["error"]);
				this.ShowPopup(MainWindow.Instance.LocalizationManager["check_log_for_more_details"], MainWindow.Instance.LocalizationManager["error"]);
			});
		}

		public async void ExportSelectedAssetToProject()
		{
			if (!_ripper.IsLoaded || SelectedAsset == null)
			{
				return;
			}

			if (SelectedAsset.Asset is DummyAssetForLooseResourceFile da)
			{
				string fileName = Path.GetFileName(da.AssociatedFile.Name);

				FilePickerSaveOptions saveOptions = new()
				{
					DefaultExtension = Path.GetExtension(fileName),
					SuggestedFileName = fileName,
				};
				IStorageFile? storageFile = await MainWindow.Instance.StorageProvider.SaveFilePickerAsync(saveOptions);

				if (storageFile is null)
				{
					return;
				}

				await da.SaveToFileAsync(storageFile.Path.LocalPath);

				Logger.Info(LogCategory.ExportProgress, $"Loose file saved at: {storageFile.Path.LocalPath}");

				return;
			}

			FolderPickerOpenOptions options = new() { AllowMultiple = false };
			IReadOnlyList<IStorageFolder> folderList = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(options);
			if (folderList.Count == 0)
			{
				return;
			}

			IsExporting = true;
			ExportingText = MainWindow.Instance.LocalizationManager["export_deleting_old_files"];

			string exportPath = Path.Combine(folderList[0].Path.LocalPath, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);

			UIExportManager.Export(_ripper, this, exportPath, SelectedAsset.Asset, () =>
			{
				IsExporting = false;
				this.ShowPopup(MainWindow.Instance.LocalizationManager["export_complete"], MainWindow.Instance.LocalizationManager["success"]);
				Logger.Info(LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				Logger.Error(error);
				this.ShowPopup(string.Format(MainWindow.Instance.LocalizationManager["error_exporting_with_reason"], error.Message), MainWindow.Instance.LocalizationManager["error"]);
			});
		}

		public async void ExportSelectedAssetTypeToProject()
		{
			if (!_ripper.IsLoaded || SelectedAsset == null || SelectedAsset.Asset is DummyAssetForLooseResourceFile)
			{
				return;
			}

			FolderPickerOpenOptions options = new() { AllowMultiple = false };
			IReadOnlyList<IStorageFolder> folderList = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(options);
			if (folderList.Count == 0)
			{
				return;
			}

			IsExporting = true;
			ExportingText = MainWindow.Instance.LocalizationManager["export_deleting_old_files"];

			string exportPath = Path.Combine(folderList[0].Path.LocalPath, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);

			UIExportManager.Export(_ripper, this, exportPath, SelectedAsset.Asset.GetType(), () =>
			{
				IsExporting = false;
				this.ShowPopup(MainWindow.Instance.LocalizationManager["export_complete"], MainWindow.Instance.LocalizationManager["success"]);
				Logger.Info(LogCategory.General, "Export Complete!");
			}, error =>
			{
				IsExporting = false;
				Logger.Error(error);
				this.ShowPopup(string.Format(MainWindow.Instance.LocalizationManager["error_exporting_with_reason"], error.Message), MainWindow.Instance.LocalizationManager["error"]);
			});
		}

		// Called from UI
		public void OpenUnityDownloadPage()
		{
			string url = "https://unity3d.com/unity/whats-new/" + UnityVersion;
			OpenUrl(url);
		}

		private static void OpenUrl(string url)
		{
			if (OperatingSystem.IsWindows())
			{
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			}
			else if (OperatingSystem.IsLinux())
			{
				Process.Start("xdg-open", url);
			}
			else if (OperatingSystem.IsMacOS())
			{
				Process.Start("open", url);
			}
		}

		//Called from UI
		public async void ShowOpenFileDialog()
		{
			FilePickerOpenOptions options = new() { AllowMultiple = false };
			IReadOnlyList<IStorageFile> fileList = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(options);

			string[] result = fileList.Select(f => f.Path.LocalPath)
				.Where(s => !string.IsNullOrEmpty(s))
				.ToArray();

			if (result.Length > 0)
			{
				DoLoad(result);
			}
		}

		//Called from UI
		public async void ShowOpenFolderDialog()
		{
			FolderPickerOpenOptions options = new() { AllowMultiple = false };
			IReadOnlyList<IStorageFolder> folderList = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(options);
			if (folderList.Count == 0)
			{
				return;
			}

			DoLoad(new[] { folderList[0].Path.LocalPath });
		}

		//Called from UI
		public void Reset()
		{
			if (!HasFile)
			{
				return;
			}

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
		public void OnAssetSelected(NewUiFileListItem selectedItem, IUnityObjectBase selectedAsset)
		{
			if (_assetContainer is not null)
			{
				_assetContainer.LastAccessedAsset = selectedAsset;
			}

			SelectedAsset?.Dispose();
			SelectedAsset = new(selectedAsset, _assetContainer);
		}
	}
}
