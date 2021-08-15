using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.TextureContainers.KTX;
using Avalonia.Media;
using System;
using System.Text;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.GUI.AssetInfo
{
	public class SelectedAsset
	{
		private Object _asset;
		private readonly string _name;
		private readonly IExportContainer _uiAssetContainer;

		public SelectedAsset(Object asset, string name, IExportContainer uiAssetContainer)
		{
			_name = name;
			_uiAssetContainer = uiAssetContainer;
			_asset = asset;

			BuildYamlTree(uiAssetContainer);
		}

		private void BuildYamlTree(IExportContainer uiAssetContainer)
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

				YamlTree = new[] { new AssetYamlNode($"Asset Threw {e.GetType().Name} when exporting as YAML", new YAMLScalarNode(true)) };
			}
		}

		public AssetYamlNode[] YamlTree { get; private set; } = { new("Tree loading...", YAMLScalarNode.Empty) };

		public bool HasImageData => _asset is IHasImageData;

		public IImage? ImageData
		{
			get
			{
				return _asset switch
				{
					IHasImageData img => AvaloniaBitmapFromDirectBitmap.Make(TextureAssetExporter.ConvertToBitmap(img.TextureFormat, img.Width, img.Height, _asset.File.Version, img.ImageDataByteArray, 0, 0, KTXBaseInternalFormat.RG)),
					_ => null
				};
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
	}
}