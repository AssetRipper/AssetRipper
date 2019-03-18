using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
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

		public void Read(AssetReader reader)
		{
			if(IsVector3(reader.Version))
			{
				Min.Read3(reader);
				Max.Read3(reader);
			}
			else
			{
				Min.Read(reader);
				Max.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Min", Min.ExportYAML3(container));
			node.Add("m_Max", Max.ExportYAML3(container));
			return node;
		}

		public override string ToString()
		{
			return $"{Min}-{Max}";
		}

		public Vector4f Min;
		public Vector4f Max;
	}
}
