using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct XForm : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			if (IsVector3(reader.Version))
			{
				T = reader.ReadAsset<Vector3f>();
			}
			else
			{
				T4.Read(reader);
			}
			Q.Read(reader);
			if (IsVector3(reader.Version))
			{
				S = reader.ReadAsset<Vector3f>();
			}
			else
			{
				S4.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TName, T.ExportYAML(container));
			node.Add(QName, Q.ExportYAML(container));
			node.Add(SName, S.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"T:{T4} Q:{Q} S:{S4}";
		}

		public Vector3f T
		{
			get => (Vector3f)T4;
			set => T4 = value;
		}
		public Vector3f S
		{
			get => (Vector3f)S4;
			set => S4 = value;
		}

		public const string TName = "t";
		public const string QName = "q";
		public const string SName = "s";

		public Vector4f T4;
		public Vector4f Q;
		public Vector4f S4;
	}
}
