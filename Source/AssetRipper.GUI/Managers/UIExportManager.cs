using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;
using AssetRipper.GUI.Utils;
using AssetRipper.Import.Logging;

namespace AssetRipper.GUI.Managers
{
	public static class UIExportManager
	{
		public static async Task PrepareExportDirectory(string path)
		{
			string directory = Directory.GetCurrentDirectory();
			if (!PermissionValidator.CheckAccess(directory))
			{
				PermissionValidator.RestartAsAdministrator();
			}

			if (Directory.Exists(path))
			{
				await Task.Run(() => Directory.Delete(path, true));
			}
		}

		public static void Export(Ripper ripper, MainWindowViewModel vm, string toRoot, Action onSuccess, Action<Exception> onError) => new Thread(() => ExportInternal(ripper, vm, toRoot, onSuccess, onError))
		{
			Name = "Background Game Export Thread",
			IsBackground = true
		}.Start();

		public static void Export(Ripper ripper, MainWindowViewModel vm, string toRoot, IUnityObjectBase asset, Action onSuccess, Action<Exception> onError) => new Thread(() => ExportInternal(ripper, vm, toRoot, asset, onSuccess, onError))
		{
			Name = "Background Game Export Thread",
			IsBackground = true
		}.Start();

		public static void Export(Ripper ripper, MainWindowViewModel vm, string toRoot, Type assetType, Action onSuccess, Action<Exception> onError) => new Thread(() => ExportInternal(ripper, vm, toRoot, assetType, onSuccess, onError))
		{
			Name = "Background Game Export Thread",
			IsBackground = true
		}.Start();

		private static void ExportInternal(Ripper ripper, MainWindowViewModel vm, string toRoot, Action onSuccess, Action<Exception> onError)
		{
			try
			{
				ripper.ExportProject(toRoot, e => ConfigureExportEvents(e, vm));
			}
			catch (Exception ex)
			{
				onError(ex);
				return;
			}

			onSuccess();
		}

		private static void ExportInternal(Ripper ripper, MainWindowViewModel vm, string toRoot, IUnityObjectBase asset, Action onSuccess, Action<Exception> onError)
		{
			try
			{
				ripper.ExportProject(toRoot, a => a == asset, e => ConfigureExportEvents(e, vm));
			}
			catch (Exception ex)
			{
				onError(ex);
				return;
			}

			onSuccess();
		}

		private static void ExportInternal(Ripper ripper, MainWindowViewModel vm, string toRoot, Type assetType, Action onSuccess, Action<Exception> onError)
		{
			try
			{
				ripper.ExportProject(toRoot, a => a.GetType().IsAssignableTo(assetType), e => ConfigureExportEvents(e, vm));
			}
			catch (Exception ex)
			{
				onError(ex);
				return;
			}

			onSuccess();
		}

		public static void ConfigureExportEvents(ProjectExporter exporter, MainWindowViewModel vm)
		{
			exporter.EventExportPreparationStarted += () =>
			{
				vm.ExportingText = MainWindow.Instance.LocalizationManager["export_preparing"];
				Logger.Info(LogCategory.Export, "Preparing for export...");
			};

			exporter.EventExportPreparationFinished += () =>
			{
				vm.ExportingText = MainWindow.Instance.LocalizationManager["export_in_progress_no_file_count_yet"];
				Logger.Info(LogCategory.Export, "Preparation complete. Starting to export now...");
			};

			exporter.EventExportProgressUpdated += (index, count) =>
			{
				double progress = (double)index / count * 100.0;
				vm.ExportingText = string.Format(MainWindow.Instance.LocalizationManager["export_in_progress"], progress.ToString("f0"), index, count);
			};
		}
	}
}
