using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.GUI.AssetInfo;
using AssetRipper.GUI.Exceptions;
using AssetRipper.GUI.Extensions;
using AssetRipper.GUI.Managers;
using AssetRipper.Library;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
				UnityVersion = _ripper.GameStructure.FileCollection.GameFiles.Values.Max(t => t.Version).ToString();
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
			UIExportManager.ConfigureExportEvents(_ripper.GameStructure.Exporter, this);

			UIExportManager.Export(_ripper, exportPath, () =>
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
			if (_ripper.GameStructure == null || SelectedAsset == null)
			{
				return;
			}

			if (SelectedAsset.Asset is DummyAssetForLooseResourceFile da)
			{
				SaveFileDialog saveFileDialog = new();
				string fileName = Path.GetFileName(da.AssociatedFile.Name);
				saveFileDialog.DefaultExtension = Path.GetExtension(fileName);
				saveFileDialog.InitialFileName = fileName;

				string? saveLoc = await saveFileDialog.ShowAsync(MainWindow.Instance);

				if (saveLoc == null)
				{
					return;
				}

				await da.SaveToFileAsync(saveLoc);

				Logger.Info(LogCategory.ExportedFile, $"Loose file saved at: {saveLoc}");

				return;
			}

			OpenFolderDialog openFolderDialog = new();

			string? chosenFolder = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (string.IsNullOrEmpty(chosenFolder))
			{
				return;
			}

			IsExporting = true;
			ExportingText = MainWindow.Instance.LocalizationManager["export_deleting_old_files"];

			string exportPath = Path.Combine(chosenFolder, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);
			UIExportManager.ConfigureExportEvents(_ripper.GameStructure.Exporter, this);

			UIExportManager.Export(_ripper, exportPath, SelectedAsset.Asset, () =>
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
			if (_ripper.GameStructure == null || SelectedAsset == null || SelectedAsset.Asset is DummyAssetForLooseResourceFile)
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
			ExportingText = MainWindow.Instance.LocalizationManager["export_deleting_old_files"];

			string exportPath = Path.Combine(chosenFolder, _ripper.GameStructure.Name ?? ("AssetRipperExport" + DateTime.Now.Ticks));
			_lastExportPath = exportPath;

			Logger.Info(LogCategory.General, $"About to begin export to {exportPath}");

			Logger.Info(LogCategory.General, $"Removing any files from a previous export...");

			await UIExportManager.PrepareExportDirectory(exportPath);
			UIExportManager.ConfigureExportEvents(_ripper.GameStructure.Exporter, this);

			UIExportManager.Export(_ripper, exportPath, SelectedAsset.Asset.GetType(), () =>
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
			OpenFileDialog openFileDialog = new();
			openFileDialog.AllowMultiple = true;
			string[]? result = await openFileDialog.ShowAsync(MainWindow.Instance);

			if (result == null)
			{
				return;
			}

			DoLoad(result);
		}

		//Called from UI
		public async void ShowOpenFolderDialog()
		{
			OpenFolderDialog openFolderDialog = new();
			string? result = await openFolderDialog.ShowAsync(MainWindow.Instance);

			if (string.IsNullOrEmpty(result))
			{
				return;
			}

			DoLoad(new[] { result });
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
