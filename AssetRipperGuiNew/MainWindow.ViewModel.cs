using Avalonia.Input;
using MessageBox.Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipperGuiNew
{
	public class MainWindowViewModel : BaseViewModel
	{
		private bool _hasFile;
		private bool _hasLoaded;
		private string _loadingText;

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
		
		public string LoadingText
		{
			get => _loadingText;
			set
			{
				_loadingText = value;
				OnPropertyChanged();
			}
		}

		private void SetGamePath(string path) => LoadingText = $"Loading Game Content from {path}...";

		public void FileDropped(DragEventArgs e)
		{
			List<string>? filesDropped = e.Data.GetFileNames()?.ToList();

			if (filesDropped == null || filesDropped.Count < 1)
				return;

			var gamePath = filesDropped[0];
			
			HasFile = true;
			HasLoaded = false;

			SetGamePath(gamePath);
		}
	}
}