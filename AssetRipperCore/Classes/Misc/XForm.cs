using AssetRipper.Project;
using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using AssetRipper.Math;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Classes.Misc
{
	public struct XForm : IAssetReadable, IYAMLExportable
	{
		public const string TName = "t";
		public const string QName = "q";
		public const string SName = "s";

		public Vector4f T4 { get; set; }
		public Quaternionf Q { get; set; }
		public Vector4f S4 { get; set; }

		//public XForm() { }

		public XForm(ObjectReader reader)
		{
			var version = reader.version;
			T4 = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : reader.ReadVector4f();//5.4 and up
			Q = reader.ReadQuaternionf();
			S4 = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : reader.ReadVector4f();//5.4 and up
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3f(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			if (IsVector3f(reader.Version))
			{
				T = reader.ReadAsset<Vector3f>();
			}
			else
			{
				T4.Read(reader);
			}
			Q.Read(reader);
			if (IsVector3f(reader.Version))
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

	}
}
