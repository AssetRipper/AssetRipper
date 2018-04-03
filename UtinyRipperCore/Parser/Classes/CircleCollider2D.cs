using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class CircleCollider2D : Collider2D
	{
		public CircleCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadCenter(Version version)
		{
			return version.IsLess(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Radius = stream.ReadSingle();
			if (IsReadCenter(stream.Version))
			{
				Center.Read(stream);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Radius", Radius);
			return node;
		}

		public float Radius { get; private set; }

		public Vector2f Center;
	}
}
