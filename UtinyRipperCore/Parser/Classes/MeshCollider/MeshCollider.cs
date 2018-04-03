using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.MeshColliders;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class MeshCollider : Collider
	{
		public MeshCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadSmoothSphereCollisions(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadCookingOptions(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadSkinWidth(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.5.0 to 2017.3 exclusive
		/// </summary>
		public static bool IsReadInflateMesh(Version version)
		{
			return version.IsGreaterEqual(5, 5) && version.IsLess(2017, 3);
		}
		
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsReadMeshFirst(Version version)
		{
			return version.IsLess(2, 1);
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
				return 3;
			}

			if (version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadMeshFirst(stream.Version))
			{
				Mesh.Read(stream);
			}

			if (IsReadSmoothSphereCollisions(stream.Version))
			{
				SmoothSphereCollisions = stream.ReadBoolean();
			}
			Convex = stream.ReadBoolean();
			if (IsReadInflateMesh(stream.Version))
			{
				InflateMesh = stream.ReadBoolean();
			}
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadCookingOptions(stream.Version))
			{
				CookingOptions = (MeshColliderCookingOptions)stream.ReadInt32();
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadSkinWidth(stream.Version))
			{
				SkinWidth = stream.ReadSingle();
			}

			if (!IsReadMeshFirst(stream.Version))
			{
				Mesh.Read(stream);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Convex", Convex);
			node.Add("m_CookingOptions", (int)CookingOptions);
			node.Add("m_SkinWidth", SkinWidth);
			node.Add("m_Mesh", Mesh.ExportYAML(exporter));
			return node;
		}

		public bool Convex { get; private set; }
		public bool SmoothSphereCollisions { get; private set; }
		public bool InflateMesh { get; private set; }
		public MeshColliderCookingOptions CookingOptions { get; private set; }
		public float SkinWidth { get; private set; }

		public PPtr<Mesh> Mesh;

		protected override bool IsReadIsTrigger => true;
		protected override bool IsReadMaterial => true;
	}
}
