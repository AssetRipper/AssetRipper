using AssetRipper.Core.Project;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Classes.Misc.Serializable.GUIStyle;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.YAML;
using System;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Classes.Misc.Serializable.GUIStyle
{
	public struct GUIStyleState : IAsset
	{
		public GUIStyleState(AssetLayout layout)
		{
			Background = default;
			ScaledBackgrounds = Array.Empty<PPtr<Texture2D.Texture2D>>();
			TextColor = ColorRGBAf.Black;
		}

		public GUIStyleState(GUIStyleState copy)
		{
			Background = copy.Background;
			TextColor = copy.TextColor;
			ScaledBackgrounds = new PPtr<Texture2D.Texture2D>[copy.ScaledBackgrounds.Length];
			for (int i = 0; i < copy.ScaledBackgrounds.Length; i++)
			{
				ScaledBackgrounds[i] = copy.ScaledBackgrounds[i];
			}
		}

		public void Read(AssetReader reader)
		{
			GUIStyleStateLayout layout = reader.Layout.Serialized.GUIStyle.GUIStyleState;
			Background.Read(reader);
			if (layout.HasScaledBackgrounds)
			{
				ScaledBackgrounds = reader.ReadAssetArray<PPtr<Texture2D.Texture2D>>();
			}
			else
			{
				ScaledBackgrounds = Array.Empty<PPtr<Texture2D.Texture2D>>();
			}
			TextColor.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			GUIStyleStateLayout layout = writer.Layout.Serialized.GUIStyle.GUIStyleState;
			Background.Write(writer);
			if (layout.HasScaledBackgrounds)
			{
				writer.WriteAssetArray(ScaledBackgrounds);
			}
			TextColor.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			GUIStyleStateLayout layout = container.ExportLayout.Serialized.GUIStyle.GUIStyleState;
			node.Add(layout.BackgroundName, Background.ExportYAML(container));
			if (layout.HasScaledBackgrounds)
			{
				node.Add(layout.ScaledBackgroundsName, ScaledBackgrounds.ExportYAML(container));
			}
			node.Add(layout.TextColorName, TextColor.ExportYAML(container));
			return node;
		}

		public PPtr<Texture2D.Texture2D>[] ScaledBackgrounds { get; set; }

		public PPtr<Texture2D.Texture2D> Background;
		public ColorRGBAf TextColor;
	}
}
