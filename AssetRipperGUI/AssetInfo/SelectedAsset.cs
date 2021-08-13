using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Text;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.GUI.AssetInfo
{
	public class SelectedAsset
	{
		private Object _asset;
		private readonly string _name;

		public SelectedAsset(Object asset, string name, IExportContainer uiAssetContainer)
		{
			_name = name;
			_asset = asset;

			try
			{
				YAMLMappingNode yamlRoot = (YAMLMappingNode)asset.ExportYAML(uiAssetContainer);

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

		public AssetYamlNode[] YamlTree { get; }

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

				return builder.ToString();
			}
		}
	}
}