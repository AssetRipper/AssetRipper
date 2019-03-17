using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			
			Radius = reader.ReadSingle();
			Height = reader.ReadSingle();
			Direction = reader.ReadInt32();
			Center.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Radius", Radius);
			node.Add("m_Height", Height);
			node.Add("m_Direction", Direction);
			node.Add("m_Center", Center.ExportYAML(container));
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
