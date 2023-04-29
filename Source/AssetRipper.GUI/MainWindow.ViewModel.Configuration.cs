﻿using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
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

		public bool EnablePrefabOutlining
		{
			get => _ripper.Settings.EnablePrefabOutlining;
			set
			{
				_ripper.Settings.EnablePrefabOutlining = value;
				OnPropertyChanged();
			}
		}

		public bool EnableStaticMeshSeparation
		{
			get => _ripper.Settings.EnableStaticMeshSeparation;
			set
			{
				_ripper.Settings.EnableStaticMeshSeparation = value;
				OnPropertyChanged();
			}
		}

		public bool IgnoreEngineAssets
		{
			get => _ripper.Settings.IgnoreEngineAssets;
			set
			{
				_ripper.Settings.IgnoreEngineAssets = value;
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

		public ShaderExportMode ShaderExportMode
		{
			get => _ripper.Settings.ShaderExportMode;
			set
			{
				_ripper.Settings.ShaderExportMode = value;
				OnPropertyChanged();
			}
		}

		public ScriptExportMode ScriptExportMode
		{
			get => _ripper.Settings.ScriptExportMode;
			set
			{
				_ripper.Settings.ScriptExportMode = value;
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
			get => _ripper.Settings.ScriptLanguageVersion;
			set
			{
				_ripper.Settings.ScriptLanguageVersion = value;
				OnPropertyChanged();
			}
		}

		public Binding? MeshExportModeBinding { get; set; }
	}
}
