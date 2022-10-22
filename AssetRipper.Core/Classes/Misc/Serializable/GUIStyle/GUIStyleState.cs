using AssetRipper.Assets.Export;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;
using AssetRipper.Yaml;

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
			ScaledBackgrounds = Array.Empty<SerializablePointer<ITexture2D>>();
			TextColor.SetAsBlack();
		}

		public void Read(AssetReader reader)
		{
			Background.Read(reader);
			if (HasScaledBackgrounds(reader.Version, reader.Flags))
			{
				ScaledBackgrounds = reader.ReadAssetArray<SerializablePointer<ITexture2D>>();
			}
			else
			{
				ScaledBackgrounds = Array.Empty<SerializablePointer<ITexture2D>>();
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new();
			node.Add(BackgroundName, Background.ExportYaml(container));
			if (HasScaledBackgrounds(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ScaledBackgroundsName, ScaledBackgrounds.ExportYaml(container));
			}
			node.Add(TextColorName, TextColor.ExportYaml(container));
			return node;
		}

		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasScaledBackgrounds(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(5, 4) && !flags.IsRelease();

		public SerializablePointer<ITexture2D>[] ScaledBackgrounds { get; set; }

		public SerializablePointer<ITexture2D> Background { get; set; }

		public ColorRGBAf_3_5_0_f5 TextColor { get; } = new();

		public const string BackgroundName = "m_Background";
		public const string ScaledBackgroundsName = "m_ScaledBackgrounds";
		public const string TextColorName = "m_TextColor";
	}
}
