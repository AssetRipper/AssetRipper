using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.Misc.Serializable.GUIStyle
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/IMGUI/GUIStyle.cs"/>
	/// </summary>
	public sealed class GUIStyleState : IAsset
	{
		public GUIStyleState()
		{
			Background = new();
			ScaledBackgrounds = Array.Empty<PPtr<Texture2D.ITexture2D>>();
			TextColor = ColorRGBAf.Black;
		}

		public GUIStyleState(GUIStyleState copy)
		{
			Background = copy.Background;
			TextColor = copy.TextColor.Clone();
			ScaledBackgrounds = new PPtr<Texture2D.ITexture2D>[copy.ScaledBackgrounds.Length];
			for (int i = 0; i < copy.ScaledBackgrounds.Length; i++)
			{
				ScaledBackgrounds[i] = copy.ScaledBackgrounds[i];
			}
		}

		public void Read(AssetReader reader)
		{
			Background.Read(reader);
			if (HasScaledBackgrounds(reader.Version, reader.Flags))
			{
				ScaledBackgrounds = reader.ReadAssetArray<PPtr<Texture2D.ITexture2D>>();
			}
			else
			{
				ScaledBackgrounds = Array.Empty<PPtr<Texture2D.ITexture2D>>();
			}
			TextColor.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Background.Write(writer);
			if (HasScaledBackgrounds(writer.Version, writer.Flags))
			{
				writer.WriteAssetArray(ScaledBackgrounds);
			}
			TextColor.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(BackgroundName, Background.ExportYAML(container));
			if (HasScaledBackgrounds(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ScaledBackgroundsName, ScaledBackgrounds.ExportYAML(container));
			}
			node.Add(TextColorName, TextColor.ExportYAML(container));
			return node;
		}

		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasScaledBackgrounds(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(5, 4) && !flags.IsRelease();

		public PPtr<Texture2D.ITexture2D>[] ScaledBackgrounds { get; set; }

		public PPtr<Texture2D.ITexture2D> Background = new();
		public ColorRGBAf TextColor = new();

		public const string BackgroundName = "m_Background";
		public const string ScaledBackgroundsName = "m_ScaledBackgrounds";
		public const string TextColorName = "m_TextColor";
	}
}
