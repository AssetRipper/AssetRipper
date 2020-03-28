using System.Collections.Generic;
using uTinyRipper.Classes.MeshColliders;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class MeshCollider : Collider
	{
		public MeshCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// MeshColliderCookingOptions.UseFastMidphase is a force option for old versions (even though it was introduced in 2019.3.0)
			if (version.IsGreaterEqual(2019, 3, 7))
			{
				return 4;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasSmoothSphereCollisions(Version version) => version.IsLess(5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasCookingOptions(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 5.5.0 to 2018.3 exclusive
		/// </summary>
		public static bool HasSkinWidth(Version version) => version.IsGreaterEqual(5, 5) && version.IsLess(2018, 3);
		/// <summary>
		/// 5.5.0 to 2017.3 exclusive
		/// </summary>
		public static bool HasInflateMesh(Version version) => version.IsGreaterEqual(5, 5) && version.IsLess(2017, 3);
		
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsMeshFirst(Version version) => version.IsLess(2, 1);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (HasSmoothSphereCollisions(reader.Version))
			{
				SmoothSphereCollisions = reader.ReadBoolean();
			}
			Convex = reader.ReadBoolean();
			if (HasInflateMesh(reader.Version))
			{
				InflateMesh = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCookingOptions(reader.Version))
			{
				CookingOptions = (MeshColliderCookingOptions)reader.ReadInt32();
				reader.AlignStream();
			}
			if (HasSkinWidth(reader.Version))
			{
				SkinWidth = reader.ReadSingle();
			}

			if (!IsMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(Mesh, MeshName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ConvexName, Convex);
			node.Add(CookingOptionsName, (int)GetCookingOptions(container.Version, container.ExportVersion));
			node.Add(SkinWidthName, GetSkinWidth(container.Version));
			node.Add(MeshName, Mesh.ExportYAML(container));
			return node;
		}

		private MeshColliderCookingOptions GetCookingOptions(Version origin, Version export)
		{
			if (HasCookingOptions(origin))
			{
				return CookingOptions;
			}
			else
			{
				MeshColliderCookingOptions options = MeshColliderCookingOptions.CookForFasterSimulation |
					MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
				if (HasInflateMesh(origin))
				{
					if (InflateMesh)
					{
						options |= MeshColliderCookingOptions.InflateConvexMesh;
					}
				}
				if (ToSerializedVersion(origin) < 4 && ToSerializedVersion(export) >= 4)
				{
					options |= MeshColliderCookingOptions.UseFastMidphase;
				}
				return options;
			}
		}

		private float GetSkinWidth(Version version)
		{
			return HasSkinWidth(version) ? SkinWidth : 0.01f;
		}

		public bool Convex { get; set; }
		public bool SmoothSphereCollisions { get; set; }
		public bool InflateMesh { get; set; }
		public MeshColliderCookingOptions CookingOptions { get; set; }
		public float SkinWidth { get; set; }

		public const string ConvexName = "m_Convex";
		public const string CookingOptionsName = "m_CookingOptions";
		public const string SkinWidthName = "m_SkinWidth";
		public const string MeshName = "m_Mesh";

		public PPtr<Mesh> Mesh;

		protected override bool IncludesIsTrigger => true;
		protected override bool IncludesMaterial => true;
	}
}
