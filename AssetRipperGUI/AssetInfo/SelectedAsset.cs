using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Terrains;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.Exporters.Textures.Enums;
using AssetRipper.Library.Utils;
using Avalonia.Media;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace AssetRipper.GUI.AssetInfo
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
				bool success = AudioClipDecoder.TryGetDecodedAudioClipData(clip, out byte[] rawClipAudioData, out string _);
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
				YAMLMappingNode yamlRoot = (YAMLMappingNode)Asset.ExportYAML(_uiAssetContainer);

				YamlTree = new[] { new AssetYamlNode(Name ?? Asset.GetType().Name, yamlRoot) };
			}
			catch (Exception e)
			{
				YamlTreeIsSupported = false;

				if (e is NotImplementedException or NotSupportedException)
				{
					YamlTree = new[] { new AssetYamlNode("Asset Doesn't Support YAML Export", new YAMLScalarNode(true)) };
					return;
				}
				Logger.Error(e);
				YamlTree = new[] { new AssetYamlNode($"Asset threw {e.GetType().Name} when exporting as YAML", new YAMLScalarNode(true)) };
			}
		}

		//Read from UI
		public AssetYamlNode[] YamlTree { get; private set; } = { new("Tree loading...", YAMLScalarNode.Empty) };

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
			ITextAsset txt => !txt.Script.IsNullOrEmpty(),
			IHasRawData rawDataAsset => rawDataAsset.RawData.Length > 0,
			_ => false,
		};

		//Read from UI
		public string? TextAssetData => (Asset switch
		{
			IShader shader => DumpShaderDataAsText(shader),
			ITextAsset txt => txt.ParseWithUTF8(),
			IHasRawData rawDataAsset => rawDataAsset.RawData.ToFormattedHex(),
			_ => null
		})?.Replace("\t", "    ");

		//Read from UI
		public IImage? ImageData
		{
			get
			{
				switch (Asset)
				{
					case Texture2D texture:
						{
							DirectBitmap? directBitmap = TextureAssetExporter.ConvertToBitmap(texture);
							return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
						}
					case ITexture2D img:
						{
							DirectBitmap? directBitmap = TextureAssetExporter.ConvertToBitmap(img.TextureFormat, img.Width, img.Height, Asset.SerializedFile.Version, img.ImageData, 0, 0, KTXBaseInternalFormat.RG);
							return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
						}
					case ITerrainData terrain:
						{
							DirectBitmap? directBitmap = TerrainHeatmapExporter.GetBitmap(terrain);
							return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
						}
					default:
						return null;
				}
			}
		}

		private bool HasName => Asset switch
		{
			IShader s => !string.IsNullOrEmpty(s.GetValidShaderName()),
			IGameObject go => !string.IsNullOrEmpty(go.Name),
			INamedObject no => !string.IsNullOrEmpty(no.Name),
			IHasName hasName => !string.IsNullOrEmpty(hasName.Name),
			_ => false
		};

		private string? Name => Asset switch
		{
			IShader s => s.GetValidShaderName(),
			IGameObject go => go.Name,
			INamedObject no => no.Name,
			IHasName hasName => hasName.Name,
			_ => null
		};

		private TextureFormat TextureFormat => Asset switch
		{
			ITexture2D img => img.TextureFormat,
			ITerrainData => TextureFormat.RGBA32,
			_ => TextureFormat.Automatic,
		};

		private int ImageWidth => Asset switch
		{
			ITexture2D img => img.Width,
			ITerrainData terrain => terrain.Heightmap.Width,
			_ => -1,
		};

		private int ImageHeight => Asset switch
		{
			ITexture2D img => img.Height,
			ITerrainData terrain => terrain.Heightmap.Height,
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
				builder.Append($"Asset Class: {Asset.GetType()}\n");

				builder.Append($"Asset Type ID: {Asset.ClassID}\n");

				if (HasName)
				{
					builder.Append($"Name: {Name}\n");
				}

				if (HasImageData)
				{
					builder.Append($"Image Format: {TextureFormat}\n");
					builder.Append($"Image Dimensions (width x height): {ImageWidth} x {ImageHeight} pixels");
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

		private string DumpShaderDataAsText(IShader shader)
		{
			using MemoryStream stream = new();
			DummyShaderTextExporter.ExportShader(shader, _uiAssetContainer, stream);

			return Encoding.UTF8.GetString(stream.GetBuffer());
		}
	}
}
