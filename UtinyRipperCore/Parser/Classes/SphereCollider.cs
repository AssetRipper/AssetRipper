using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class SphereCollider : Collider
	{
		public SphereCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// min version is 2nd
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			Radius = stream.ReadSingle();
			Center.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Radius", Radius);
			node.Add("m_Center", Center.ExportYAML(exporter));
			return node;
		}

		public float Radius { get; private set; }

		public Vector3f Center;

		protected override bool IsReadIsTrigger => true;
		protected override bool IsReadMaterial => true;
	}
}
