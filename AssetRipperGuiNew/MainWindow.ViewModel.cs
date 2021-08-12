using AssetRipperGuiNew.Exceptions;
using Avalonia.Input;
using System.Linq;

namespace AssetRipperGuiNew
{
	public class MainWindowViewModel : BaseViewModel
	{
		private bool _hasFile;
		private bool _hasLoaded;
		private string? _loadingText;

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

			UpdateGamePathInUi(gamePath);

			UiGameLoader.LoadFromPath(filesDropped, gameStructure =>
			{
				HasLoaded = true;
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