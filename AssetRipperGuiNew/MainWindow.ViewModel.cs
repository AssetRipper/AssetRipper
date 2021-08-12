using AssetRipper.Core.Logging;
using AssetRipperGuiNew.Exceptions;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AssetRipperGuiNew
{
	public class MainWindowViewModel : BaseViewModel
	{
		private bool _hasFile;
		private bool _hasLoaded;
		private string? _loadingText;
		private string _logText = "";

		public ObservableCollection<NewUiFileListItem> AssetFiles { get; } = new();

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

		public MainWindowViewModel()
		{
			Logger.Add(new ViewModelLogger(this));
			Logger.Add(new FileLogger());
			Logger.LogSystemInformation("AssetRipper GUI Version");
		}

		private void UpdateGamePathInUi(string path) => LoadingText = $"Loading Game Content from {path}...";

		public void OnFileDropped(DragEventArgs e)
		{
			string[]? filesDropped = e.Data.GetFileNames()?.ToArray();

			if (filesDropped == null || filesDropped.Length < 1)
			{
				return;
			}

			string gamePath = filesDropped[0];
			
			HasFile = true;
			HasLoaded = false;
			AssetFiles.Clear();

			UpdateGamePathInUi(gamePath);

			UiGameLoader.LoadFromPath(filesDropped, gameStructure =>
			{
				HasLoaded = true;
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
	}
}