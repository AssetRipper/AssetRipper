using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Limit : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			if (IsVector3(reader.Version))
			{
				Min = reader.ReadAsset<Vector3f>();
				Max = reader.ReadAsset<Vector3f>();
			}
			else
			{
				Min4.Read(reader);
				Max4.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MinName, Min.ExportYAML(container));
			node.Add(MaxName, Max.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"{Min4}-{Max4}";
		}

		public Vector3f Min
		{
			get => (Vector3f)Min4;
			set => Min4 = value;
		}
		public Vector3f Max
		{
			get => (Vector3f)Max4;
			set => Max4 = value;
		}

		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";

		public Vector4f Min4;
		public Vector4f Max4;
	}
}
