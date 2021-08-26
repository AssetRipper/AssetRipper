using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.TextureContainers.KTX;
using AssetRipper.Library.Utils;
using Avalonia.Media;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Text;
using System.Threading;
using GameObject = AssetRipper.Core.Classes.GameObject.GameObject;
using NamedObject = AssetRipper.Core.Classes.NamedObject;
using Object = AssetRipper.Core.Classes.Object.Object;
using Shader = AssetRipper.Core.Classes.Shader.Shader;

namespace AssetRipper.GUI.AssetInfo
{
	public sealed class SelectedAsset : BaseViewModel, IDisposable
	{
		private static readonly LibVLC? LibVlc;
		
		private readonly Object _asset;

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

		public SelectedAsset(Object asset, IExportContainer? uiAssetContainer)
		{
			_asset = asset;

			BuildYamlTree(uiAssetContainer);

			if (asset is AudioClip clip && LibVlc != null)
			{
				DateTime start = DateTime.Now;
				byte[] rawClipAudioData = AudioClipDecoder.GetDecodedAudioClipData(clip);
				if(rawClipAudioData == null)
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

		private void BuildYamlTree(IExportContainer? uiAssetContainer)
		{
			try
			{
				YAMLMappingNode yamlRoot = (YAMLMappingNode)_asset.ExportYAML(uiAssetContainer);

				YamlTree = new[] { new AssetYamlNode(Name ?? _asset.GetType().Name, yamlRoot) };
			}
			catch (Exception e)
			{
				if (e is NotImplementedException or NotSupportedException)
				{
					YamlTree = new[] { new AssetYamlNode("Asset Doesn't Support YAML Export", new YAMLScalarNode(true)) };
					return;
				}
				Logger.Error(e);
				YamlTree = new[] { new AssetYamlNode($"Asset Threw {e.GetType().Name} when exporting as YAML", new YAMLScalarNode(true)) };
			}
		}

		//Read from UI
		public AssetYamlNode[] YamlTree { get; private set; } = { new("Tree loading...", YAMLScalarNode.Empty) };

		//Read from UI
		public bool HasImageData => _asset is IHasImageData;

		//Read from UI
		public bool HasAudioData => _asset is AudioClip;

		//Read from UI
		public IImage? ImageData
		{
			get
			{
				switch(_asset)
				{
					case IHasImageData img:
						{
							if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
							{
								return null;
							}

							DirectBitmap? directBitmap = TextureAssetExporter.ConvertToBitmap(img.TextureFormat, img.Width, img.Height, _asset.File.Version, img.ImageDataByteArray, 0, 0, KTXBaseInternalFormat.RG);
							return AvaloniaBitmapFromDirectBitmap.Make(directBitmap);
						}
					default:
						return null;
				}
			}
		}


		private bool SupportsName => _asset is Shader or GameObject or NamedObject;

		private bool HasName => _asset switch
		{
			Shader s => !string.IsNullOrEmpty(s.ValidName),
			GameObject go => !string.IsNullOrEmpty(go.Name),
			NamedObject no => !string.IsNullOrEmpty(no.Name),
			_ => false
		};

		private string? Name => _asset switch
		{
			Shader s => s.ValidName,
			GameObject go => go.Name,
			NamedObject no => no.Name,
			_ => null
		};

		private TextureFormat TextureFormat => _asset switch
		{
			IHasImageData img => img.TextureFormat,
			_ => TextureFormat.Automatic,
		};
		
		private int ImageWidth => _asset switch
		{
			IHasImageData img => img.Width,
			_ => -1,
		};
		
		private int ImageHeight => _asset switch
		{
			IHasImageData img => img.Height,
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
				builder.Append($"Asset Type: {_asset.GetType()}\n");

				builder.Append($"Supports Name: {SupportsName}\n");

				if (SupportsName)
				{
					builder.Append($"Has Name: {HasName}\n");

					if (HasName)
					{
						builder.Append($"Name: {Name}\n");
					}
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
			if(IsPaused)
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
	}
}