using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.GUI.Utils;
using AssetRipper.Library;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
				await Task.Factory.StartNew(s_path => Directory.Delete((string)s_path, true), path);
			}
		}

		public static void Export(Ripper ripper, string toRoot, Action onSuccess, Action<Exception> onError) => new Thread(() => ExportInternal(ripper, toRoot, onSuccess, onError))
		{
			Name = "Background Game Export Thread",
			IsBackground = true,
		}.Start();

		public static void Export(Ripper ripper, string toRoot, Core.Classes.Object.Object asset, Action onSuccess, Action<Exception> onError) => new Thread(() => ExportInternal(ripper, toRoot, asset, onSuccess, onError))
		{
			Name = "Background Game Export Thread",
			IsBackground = true,
		}.Start();

		private static void ExportInternal(Ripper ripper, string toRoot, Action onSuccess, Action<Exception> onError)
		{
			try
			{
				ripper.ExportProject(toRoot);
			}
			catch (Exception ex)
			{
				onError(ex);
				return;
			}

			onSuccess();
		}

		private static void ExportInternal(Ripper ripper, string toRoot, Core.Classes.Object.Object asset, Action onSuccess, Action<Exception> onError)
		{
			try
			{
				ripper.ExportProject(toRoot, asset);
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
				vm.ExportingText = "Preparing for Export...\nThis might take a minute.";
				Logger.Info(LogCategory.Export, "Preparing for export...");
			};

			exporter.EventExportPreparationFinished += () =>
			{
				vm.ExportingText = "Exporting Asset Files\n0.0%\n?/?";
				Logger.Info(LogCategory.Export, "Preparation complete. Starting to export now...");
			};

			exporter.EventExportProgressUpdated += (index, count) =>
			{
				double progress = (double)index / count * 100.0;
				vm.ExportingText = $"Exporting Asset Files\n{progress:f1}%\n{index}/{count}";

				Dispatcher.UIThread.Post(() => MainWindow.Instance.LogText.CaretIndex = MainWindow.Instance.LogText.Text.Length - 1, DispatcherPriority.Background);
			};
		}
	}
}