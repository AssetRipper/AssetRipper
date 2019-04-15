using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.Materials
{
	public struct UnityTexEnv : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsReadVector2(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (IsReadVector2(reader.Version))
			{
				Scale.Read2(reader);
				Offset.Read2(reader);
			}
			else
			{
				Scale.Read(reader);
				Offset.Read(reader);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Texture.FetchDependency(file, isLog, () => nameof(UnityTexEnv), TextureName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(ScaleName, Scale.ExportYAML2(container));
			node.Add(OffsetName, Offset.ExportYAML2(container));
			return node;
		}

		public const string TextureName = "m_Texture";
		public const string ScaleName = "m_Scale";
		public const string OffsetName = "m_Offset";

		public PPtr<Texture> Texture;
		public Vector3f Scale;
		public Vector3f Offset;
	}
}
