using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.CompositeCollider2D
{
	/// <summary>
	/// SpriteCollider2D previously
	/// </summary>
	public sealed class CompositeCollider2D : Collider2D
	{
		public CompositeCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasEdgeRadius(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 2019.1.3 and greater
		/// </summary>
		public static bool HasOffsetDistance(UnityVersion version) => version.IsGreaterEqual(2019, 1, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GeometryType = (GeometryType)reader.ReadInt32();
			GenerationType = (GenerationType)reader.ReadInt32();
			if (HasEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
			ColliderPaths = reader.ReadAssetArray<SubCollider>();
			reader.AlignStream();

			CompositePaths.Read(reader);
			VertexDistance = reader.ReadSingle();
			if (HasOffsetDistance(reader.Version))
			{
				OffsetDistance = reader.ReadSingle();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(ColliderPaths, ColliderPathsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(GeometryTypeName, (int)GeometryType);
			node.Add(GenerationTypeName, (int)GenerationType);
			node.Add(EdgeRadiusName, EdgeRadius);
			node.Add(ColliderPathsName, ColliderPaths.ExportYAML(container));
			node.Add(CompositePathsName, CompositePaths.ExportYAML(container));
			node.Add(VertexDistanceName, VertexDistance);
			if (HasOffsetDistance(container.ExportVersion))
			{
				node.Add(OffsetDistanceName, GetOffsetDistance(container.Version));
			}
			return node;
		}

		private float GetOffsetDistance(UnityVersion version)
		{
			return HasOffsetDistance(version) ? OffsetDistance : 0.000005f;
		}

		public GeometryType GeometryType { get; set; }
		public GenerationType GenerationType { get; set; }
		public float EdgeRadius { get; set; }
		public SubCollider[] ColliderPaths { get; set; }
		public float VertexDistance { get; set; }
		public float OffsetDistance { get; set; }

		public const string GeometryTypeName = "m_GeometryType";
		public const string GenerationTypeName = "m_GenerationType";
		public const string EdgeRadiusName = "m_EdgeRadius";
		public const string ColliderPathsName = "m_ColliderPaths";
		public const string CompositePathsName = "m_CompositePaths";
		public const string VertexDistanceName = "m_VertexDistance";
		public const string OffsetDistanceName = "m_OffsetDistance";

		public Polygon2D CompositePaths = new();
	}
}
