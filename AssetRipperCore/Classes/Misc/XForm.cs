using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class XForm : IAssetReadable, IYamlExportable
	{
		public const string TName = "t";
		public const string QName = "q";
		public const string SName = "s";

		public Vector4f T4 = new();
		public Vector4f Q = new();
		public Vector4f S4 = new();

		//public XForm() { }

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3f(UnityVersion version) => version.IsGreaterEqual(5, 4);

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TName, T.ExportYaml(container));
			node.Add(QName, Q.ExportYaml(container));
			node.Add(SName, S.ExportYaml(container));
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
