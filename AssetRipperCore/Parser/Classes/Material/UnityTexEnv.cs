using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Classes.Misc.Serializable;
using AssetRipper.Parser.Files.File.Version;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes.Material
{
	public struct UnityTexEnv : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsVector3(Version version) => version.IsLess(2, 1);

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (IsVector3(reader.Version))
			{
				Scale3.Read(reader);
				Offset3.Read(reader);
			}
			else
			{
				Scale = reader.ReadAsset<Vector2f>();
				Offset = reader.ReadAsset<Vector2f>();
			}
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(ScaleName, Scale.ExportYAML(container));
			node.Add(OffsetName, Offset.ExportYAML(container));
			return node;
		}


		public Vector2f Scale
		{
			get => (Vector2f)Scale3;
			set => Scale3 = value;
		}
		public Vector2f Offset
		{
			get => (Vector2f)Offset3;
			set => Offset3 = value;
		}

		public const string TextureName = "m_Texture";
		public const string ScaleName = "m_Scale";
		public const string OffsetName = "m_Offset";

		public PPtr<Texture> Texture;
		public Vector3f Scale3;
		public Vector3f Offset3;
	}
}
