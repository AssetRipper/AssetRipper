using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.Yaml;
using Avalonia.Media;
using LibVLCSharp.Shared;
using System.Text;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.AssetInformation
{
	public sealed class SelectedAsset : BaseViewModel, IDisposable
	{
		private static readonly LibVLC? LibVlc;

		public IUnityObjectBase Asset { get; }
		private readonly IExportContainer? _uiAssetContainer;

		private readonly MemoryStream? _audioStream;
		private readonly MediaPlayer? _mediaPlayer;
		private readonly Media? _media;

		private float _lengthSeconds;
		private float _positionSeconds;
		private string _positionString = "00:00:00/<unknown>";
		private bool _isPaused = true;

		static SelectedAsset()
		{
			try
			{
				LibVLCSharp.Shared.Core.Initialize();
			}
			catch (Exception)
			{
				return;
			}

			LibVlc = new();
			// LibVlc.Log += VlcLogHandler;
		}

		private static void VlcLogHandler(object? _, LogEventArgs e)
		{
			LogType level = e.Level switch
			{
				LogLevel.Debug => LogType.Debug,
				LogLevel.Notice => LogType.Info,
				LogLevel.Warning => LogType.Warning,
				LogLevel.Error => LogType.Error,
				_ => throw new()
			};
			Logger.Log(level, LogCategory.LibVlc, $"[{e.Module ?? "General"}] [{e.Level}] {e.Message}");
		}

		public SelectedAsset(IUnityObjectBase asset, IExportContainer? uiAssetContainer)
		{
			Asset = asset;
			_uiAssetContainer = uiAssetContainer;

			BuildYamlTree();

			if (asset is IAudioClip clip && LibVlc != null)
			{
				DateTime start = DateTime.Now;
				bool success = AudioClipDecoder.TryGetDecodedAudioClipData(clip, out byte[]? rawClipAudioData, out string? _);
				if (!success || rawClipAudioData == null)
				{
					//Unsupported sound type
					return;
				}

				_audioStream = new(rawClipAudioData);

				_media = new(LibVlc, new StreamMediaInput(_audioStream));
				_mediaPlayer = new(_media);

				_mediaPlayer.LengthChanged += (_, e) => AudioLengthSeconds = e.Length / 1000f;
				_mediaPlayer.PositionChanged += (_, e) => AudioPositionSeconds = e.Position * AudioLengthSeconds;
				_mediaPlayer.EndReached += (_, _) => ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Stop());
				_mediaPlayer.Stopped += (_, _) =>
				{
					IsPaused = true;
					AudioPositionSeconds = 0;
					_mediaPlayer.Position = 0;
					UpdatePositionString();
				};
			}
		}

		private void BuildYamlTree()
		{
			try
			{
				YamlMappingNode yamlRoot = (YamlMappingNode)Asset.ExportYaml(_uiAssetContainer ?? throw new NullReferenceException(nameof(_uiAssetContainer)));

				YamlTree = new[] { new AssetYamlNode(Name ?? Asset.GetType().Name, yamlRoot) };
			}
			catch (Exception e)
			{
				YamlTreeIsSupported = false;

				if (e is NotImplementedException or NotSupportedException)
				{
					YamlTree = new[] { new AssetYamlNode("Asset Doesn't Support Yaml Export", new YamlScalarNode(true)) };
					return;
				}
				Logger.Error(e);
				YamlTree = new[] { new AssetYamlNode($"Asset threw {e.GetType().Name} when exporting as Yaml", new YamlScalarNode(true)) };
			}
		}

		//Read from UI
		public AssetYamlNode[] YamlTree { get; private set; } = { new("Tree loading...", new YamlScalarNode()) };

		//Read from UI
		public bool HasImageData => Asset is ITexture2D or ITerrainData;

		//Read from UI
		public bool HasAudioData => Asset is IAudioClip;

		//Read from UI
		public bool YamlTreeIsSupported { get; private set; } = true;

		//Read from UI
		public bool HasTextData => Asset switch
		{
			IShader => true,
			ITextAsset txt => txt.Script_C49.Data.Length > 0,
			RawDataObject rawDataAsset => rawDataAsset.RawData.Length > 0,
			_ => false,
		};

		//Read from UI
		public string? TextAssetData => (Asset switch
		{
			IShader shader => DumpShaderDataAsText(shader),
			ITextAsset txt => txt.Script_C49.String,
			RawDataObject rawDataAsset => rawDataAsset.RawData.ToFormattedHex(),
			_ => null
		})?.Replace("\t", "    ");

		//Read from UI
		public IImage? ImageData
		{
			get
			{
				switch (Asset)
				{
					case ITexture2D texture:
						{
							if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap directBitmap))
							{
								return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
							}
							else
							{
								return null;
							}
						}
					case ITerrainData terrain:
						{
							DirectBitmap directBitmap = TerrainHeatmapExporter.GetBitmap(terrain);
							return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
						}
					default:
						return null;
				}
			}
		}

		private bool HasName => Asset switch
		{
			INamed hasName => !Utf8String.IsNullOrEmpty(hasName.Name),
			_ => false
		};

		private string? Name => Asset switch
		{
			INamed hasName => hasName.Name,
			_ => null
		};

		private SourceGenerated.Enums.TextureFormat TextureFormat => Asset switch
		{
			ITexture2D img => img.Format_C28E,
			ITerrainData => SourceGenerated.Enums.TextureFormat.RGBA32,
			_ => default,
		};

		private int ImageWidth => Asset switch
		{
			ITexture2D img => img.Width_C28,
			ITerrainData terrain => terrain.Heightmap.Width,
			_ => -1,
		};

		private int ImageHeight => Asset switch
		{
			ITexture2D img => img.Height_C28,
			ITerrainData terrain => terrain.Heightmap.Height,
			_ => -1,
		};

		private int ImageSize => Asset switch
		{
			ITexture2D img => img.GetImageData().Length,
			ITerrainData terrain => terrain.Heightmap.Width * terrain.Heightmap.Height * 2,
			_ => -1,
		};

		public float AudioLengthSeconds
		{
			get => _lengthSeconds;
			set
			{
				_lengthSeconds = value;
				OnPropertyChanged();
				UpdatePositionString();
			}
		}

		public float AudioPositionSeconds
		{
			get => _positionSeconds;
			set
			{
				_positionSeconds = value;
				OnPropertyChanged();
				UpdatePositionString();
			}
		}

		public string PositionString
		{
			get => _positionString;
			set
			{
				_positionString = value;
				OnPropertyChanged();
			}
		}

		public bool IsPaused
		{
			get => _isPaused;
			set
			{
				_isPaused = value;
				OnPropertyChanged();
			}
		}

		//Read from UI

		public string BasicInformation
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine($"Asset Class: {Asset.GetType()}");

				builder.AppendLine($"Asset Type ID: {Asset.ClassID}");
				builder.AppendLine($"Asset Path ID: {Asset.PathID}");

				if (HasName)
				{
					builder.AppendLine($"Name: {Name}");
				}

				if (Asset.OriginalDirectory is not null || Asset.OriginalName is not null || Asset.OriginalExtension is not null)
				{
					builder.AppendLine($"Original Path: {Asset.OriginalPath}");
				}

				if (HasImageData)
				{
					builder.AppendLine($"Image Format: {TextureFormat}");
					builder.AppendLine($"Image Dimensions (width x height): {ImageWidth} x {ImageHeight} pixels");
					builder.AppendLine($"Image Size: {ImageSize} bytes");
				}

				IMonoScript? monoScript = (Asset as IMonoScript) ?? (Asset as IMonoBehaviour)?.ScriptP;
				if (monoScript is not null)
				{
					builder.AppendLine($"Script: {monoScript.AssemblyName}, {monoScript.GetFullName()}");
				}

				if (Asset.MainAsset is { } mainAsset)
				{
					builder.AppendLine($"Main Asset: {mainAsset}");
				}

				return builder.ToString();
			}
		}

		public void PlayClip()
		{
			IsPaused = false;
			_mediaPlayer?.Play();
		}

		public void PauseClip()
		{
			IsPaused = true;
			_mediaPlayer?.Pause();
		}

		//Called from UI
		public void TogglePause()
		{
			if (IsPaused)
			{
				PlayClip();
			}
			else
			{
				PauseClip();
			}
		}

		public void Dispose()
		{
			_audioStream?.Dispose();
			_mediaPlayer?.Dispose();
			_media?.Dispose();
		}

		private void UpdatePositionString()
		{
			PositionString = $"{TimeSpan.FromSeconds(AudioPositionSeconds):hh\\:mm\\:ss}/{TimeSpan.FromSeconds(AudioLengthSeconds):g}";
		}

		private static string DumpShaderDataAsText(IShader shader)
		{
			using MemoryStream stream = new();
			DummyShaderTextExporter.ExportShader(shader, stream);

			return Encoding.UTF8.GetString(stream.GetBuffer());
		}
	}
}
