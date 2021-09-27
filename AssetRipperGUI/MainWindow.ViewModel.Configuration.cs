using AssetRipper.Library.Configuration;
using Avalonia.Data;

namespace AssetRipper.GUI
{
	public partial class MainWindowViewModel
	{
		public bool DisableScriptImport
		{
			get => _ripper.Settings.DisableScriptImport;
			set
			{
				_ripper.Settings.DisableScriptImport = value;
				OnPropertyChanged();
			}
		}
		
		public bool IgnoreStreamingAssets
		{
			get => _ripper.Settings.DisableScriptImport;
			set
			{
				_ripper.Settings.DisableScriptImport = value;
				OnPropertyChanged();
			}
		}
		
		public AudioExportFormat AudioExportFormat
		{
			get => _ripper.Settings.AudioExportFormat;
			set
			{
				_ripper.Settings.AudioExportFormat = value;
				OnPropertyChanged();
			}
		}
		
		public ImageExportFormat ImageExportFormat
		{
			get => _ripper.Settings.ImageExportFormat;
			set
			{
				_ripper.Settings.ImageExportFormat = value;
				OnPropertyChanged();
			}
		}
		
		public MeshExportFormat MeshExportFormat
		{
			get => _ripper.Settings.MeshExportFormat;
			set
			{
				_ripper.Settings.MeshExportFormat = value;
				OnPropertyChanged();
			}
		}
		
		public SpriteExportMode SpriteExportMode
		{
			get => _ripper.Settings.SpriteExportMode;
			set
			{
				_ripper.Settings.SpriteExportMode = value;
				OnPropertyChanged();
			}
		}
		
		public TerrainExportMode TerrainExportMode
		{
			get => _ripper.Settings.TerrainExportMode;
			set
			{
				_ripper.Settings.TerrainExportMode = value;
				OnPropertyChanged();
			}
		}
		
		public TextExportMode TextExportMode
		{
			get => _ripper.Settings.TextExportMode;
			set
			{
				_ripper.Settings.TextExportMode = value;
				OnPropertyChanged();
			}
		}
		
		public Binding MeshExportModeBinding { get; set; }
	}
}