using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	public sealed class TextureImportOutput : IAsset
	{
		public TextureImportOutput() { }

		public TextureImportOutput(LayoutInfo layout)
		{
			TextureImportInstructions = new TextureImportInstructions(layout);
			SourceTextureInformation = new SourceTextureInformation(layout);
			ImportInspectorWarnings = string.Empty;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasImportInspectorWarnings(UnityVersion version) => version.IsGreaterEqual(5);

		public void Read(AssetReader reader)
		{
			TextureImportInstructions.Read(reader);
			SourceTextureInformation.Read(reader);
			if (HasImportInspectorWarnings(reader.Version))
			{
				ImportInspectorWarnings = reader.ReadString();
			}
		}

		public void Write(AssetWriter writer)
		{
			TextureImportInstructions.Write(writer);
			SourceTextureInformation.Write(writer);
			if (HasImportInspectorWarnings(writer.Version))
			{
				writer.Write(ImportInspectorWarnings);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureImportInstructionsName, TextureImportInstructions.ExportYAML(container));
			node.Add(SourceTextureInformationName, SourceTextureInformation.ExportYAML(container));
			if (HasImportInspectorWarnings(container.ExportVersion))
			{
				node.Add(ImportInspectorWarningsName, ImportInspectorWarnings);
			}
			return node;
		}

		public string ImportInspectorWarnings { get; set; }

		public const string TextureImportInstructionsName = "textureImportInstructions";
		public const string SourceTextureInformationName = "sourceTextureInformation";
		public const string ImportInspectorWarningsName = "importInspectorWarnings";

		public TextureImportInstructions TextureImportInstructions = new();
		public SourceTextureInformation SourceTextureInformation = new();
	}
}
