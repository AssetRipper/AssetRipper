using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.MeshColliders;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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
		/// 5.5.0 to 2018.3 exclusive
		/// </summary>
		public static bool IsReadSkinWidth(Version version)
		{
			return version.IsGreaterEqual(5, 5) && version.IsLess(2018, 3);
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
			if (version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (IsReadSmoothSphereCollisions(reader.Version))
			{
				SmoothSphereCollisions = reader.ReadBoolean();
			}
			Convex = reader.ReadBoolean();
			if (IsReadInflateMesh(reader.Version))
			{
				InflateMesh = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadCookingOptions(reader.Version))
			{
				CookingOptions = (MeshColliderCookingOptions)reader.ReadInt32();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadSkinWidth(reader.Version))
			{
				SkinWidth = reader.ReadSingle();
			}

			if (!IsReadMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Mesh.FetchDependency(file, isLog, ToLogString, MeshName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(ConvexName, Convex);
			node.Add(CookingOptionsName, (int)GetCookingOptions(container.Version));
			node.Add(SkinWidthName, GetSkinWidth(container.Version));
			node.Add(MeshName, Mesh.ExportYAML(container));
			return node;
		}

		private MeshColliderCookingOptions GetCookingOptions(Version version)
		{
			if(IsReadCookingOptions(version))
			{
				return CookingOptions;
			}
			else
			{
				MeshColliderCookingOptions options = MeshColliderCookingOptions.CookForFasterSimulation |
					MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
				if (IsReadInflateMesh(version))
				{
					options |= InflateMesh ? MeshColliderCookingOptions.InflateConvexMesh : MeshColliderCookingOptions.None;
				}
				return options;
			}
		}

		private float GetSkinWidth(Version version)
		{
			return IsReadSkinWidth(version) ? SkinWidth : 0.01f;
		}

		public bool Convex { get; private set; }
		public bool SmoothSphereCollisions { get; private set; }
		public bool InflateMesh { get; private set; }
		public MeshColliderCookingOptions CookingOptions { get; private set; }
		public float SkinWidth { get; private set; }

		public const string ConvexName = "m_Convex";
		public const string CookingOptionsName = "m_CookingOptions";
		public const string SkinWidthName = "m_SkinWidth";
		public const string MeshName = "m_Mesh";

		public PPtr<Mesh> Mesh;

		protected override bool IsReadIsTrigger => true;
		protected override bool IsReadMaterial => true;
	}
}
