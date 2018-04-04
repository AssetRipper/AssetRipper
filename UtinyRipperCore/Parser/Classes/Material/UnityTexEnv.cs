using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.Materials
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

		public void Read(AssetStream stream)
		{
			Texture.Read(stream);
			if (IsReadVector2(stream.Version))
			{
				Scale.Read2(stream);
				Offset.Read2(stream);
			}
			else
			{
				Scale.Read(stream);
				Offset.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Texture", Texture.ExportYAML(exporter));
			node.Add("m_Scale", Scale.ExportYAML2(exporter));
			node.Add("m_Offset", Offset.ExportYAML2(exporter));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Texture.FetchDependency(file, isLog, () => nameof(UnityTexEnv), "m_Texture");
		}

		public PPtr<Texture> Texture;
		public Vector3f Scale;
		public Vector3f Offset;
	}
}
