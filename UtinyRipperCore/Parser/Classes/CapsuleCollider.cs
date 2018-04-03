using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class CapsuleCollider : Collider
	{
		public CapsuleCollider(AssetInfo assetInfo):
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			Radius = stream.ReadSingle();
			Height = stream.ReadSingle();
			Direction = stream.ReadInt32();
			Center.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Radius", Radius);
			node.Add("m_Height", Height);
			node.Add("m_Direction", Direction);
			node.Add("m_Center", Center.ExportYAML(exporter));
			return node;
		}

		public float Radius { get; private set; }
		public float Height { get; private set; }
		public int Direction { get; private set; }

		public Vector3f Center;

		protected override bool IsReadIsTrigger => true;
		protected override bool IsReadMaterial => true;
	}
}
