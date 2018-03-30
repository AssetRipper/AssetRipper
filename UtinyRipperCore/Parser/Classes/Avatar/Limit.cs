using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct Limit : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public void Read(AssetStream stream)
		{
			if(IsVector3(stream.Version))
			{
				Min.Read3(stream);
				Max.Read3(stream);
			}
			else
			{
				Min.Read(stream);
				Max.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Min", Min.ExportYAML3(exporter));
			node.Add("m_Max", Max.ExportYAML3(exporter));
			return node;
		}

		public Vector4f Min;
		public Vector4f Max;
	}
}
