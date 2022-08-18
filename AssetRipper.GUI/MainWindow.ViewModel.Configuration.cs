using AssetRipper.Core.Configuration;
using AssetRipper.Library.Configuration;
using Avalonia.Data;

namespace AssetRipper.GUI
{
	public partial class MainWindowViewModel
	{
		public BundledAssetsExportMode BundledAssetsExportMode
		{
			get => _ripper.Settings.BundledAssetsExportMode;
			set
			{
				_ripper.Settings.BundledAssetsExportMode = value;
				OnPropertyChanged();
			}
		}

		public bool IgnoreStreamingAssets
		{
			get => _ripper.Settings.IgnoreStreamingAssets;
			set
			{
				_ripper.Settings.IgnoreStreamingAssets = value;
				OnPropertyChanged();
			}
		}

		public AudioExportFormat AudioExportFormat
		{
			get => _ripper.Settings.GetSetting<AudioExportFormat>();
			set
			{
				_ripper.Settings.SetSetting<AudioExportFormat>(value);
				OnPropertyChanged();
			}
		}

		public ImageExportFormat ImageExportFormat
		{
			get => _ripper.Settings.GetSetting<ImageExportFormat>();
			set
			{
				_ripper.Settings.SetSetting<ImageExportFormat>(value);
				OnPropertyChanged();
			}
		}

		public MeshExportFormat MeshExportFormat
		{
			get => _ripper.Settings.GetSetting<MeshExportFormat>();
			set
			{
				_ripper.Settings.SetSetting<MeshExportFormat>(value);
				OnPropertyChanged();
			}
		}

		public SpriteExportMode SpriteExportMode
		{
			get => _ripper.Settings.GetSetting<SpriteExportMode>();
			set
			{
				_ripper.Settings.SetSetting<SpriteExportMode>(value);
				OnPropertyChanged();
			}
		}

		public TerrainExportMode TerrainExportMode
		{
			get => _ripper.Settings.GetSetting<TerrainExportMode>();
			set
			{
				_ripper.Settings.SetSetting<TerrainExportMode>(value);
				OnPropertyChanged();
			}
		}

		public TextExportMode TextExportMode
		{
			get => _ripper.Settings.GetSetting<TextExportMode>();
			set
			{
				_ripper.Settings.SetSetting<TextExportMode>(value);
				OnPropertyChanged();
			}
		}

		public ShaderExportMode ShaderExportMode
		{
			get => _ripper.Settings.GetSetting<ShaderExportMode>();
			set
			{
				_ripper.Settings.SetSetting<ShaderExportMode>(value);
				OnPropertyChanged();
			}
		}

		public ScriptExportMode ScriptExportMode
		{
			get => _ripper.Settings.GetSetting<ScriptExportMode>();
			set
			{
				_ripper.Settings.SetSetting<ScriptExportMode>(value);
				OnPropertyChanged();
			}
		}

		public ScriptContentLevel ScriptContentLevel
		{
			get => _ripper.Settings.ScriptContentLevel;
			set
			{
				_ripper.Settings.ScriptContentLevel = value;
				OnPropertyChanged();
			}
		}

		public ScriptLanguageVersion ScriptLanguageVersion
		{
			get => _ripper.Settings.GetSetting<ScriptLanguageVersion>();
			set
			{
				_ripper.Settings.SetSetting<ScriptLanguageVersion>(value);
				OnPropertyChanged();
			}
		}

		public Binding? MeshExportModeBinding { get; set; }
	}
}
