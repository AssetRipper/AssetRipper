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
				T.Read3(reader);
			}
			else
			{
				T.Read(reader);
			}
			Q.Read(reader);
			if (IsVector3(reader.Version))
			{
				S.Read3(reader);
			}
			else
			{
				S.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TName, T.ExportYAML3(container));
			node.Add(QName, Q.ExportYAML(container));
			node.Add(SName, S.ExportYAML3(container));
			return node;
		}

		public override string ToString()
		{
			return $"T:{T} Q:{Q} S:{S}";
		}

		public const string TName = "t";
		public const string QName = "q";
		public const string SName = "s";

		public Vector4f T;
		public Vector4f Q;
		public Vector4f S;
	}
}
