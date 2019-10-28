using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.CompositeCollider2Ds
{
	public struct SubCollider : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool IsReadDoubleColliderPath(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		public void Read(AssetReader reader)
		{
			Collider.Read(reader);
			m_colliderPaths = reader.ReadAssetArrayArray<IntPoint>();
			reader.AlignStream(AlignType.Align4);
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield return context.FetchDependency(Collider, ColliderName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ColliderName, Collider.ExportYAML(container));
			if (IsReadDoubleColliderPath(container.ExportVersion))
			{
				node.Add(ColliderPathsName, ColliderPaths.ExportYAML(container));
			}
			else
			{
				IReadOnlyList<IntPoint> colliderPaths = ColliderPaths.Count == 0 ? System.Array.Empty<IntPoint>() : ColliderPaths[0];
				node.Add(ColliderPathsName, colliderPaths.ExportYAML(container));
			}
			return node;
		}

		public const string ColliderName = "m_Collider";
		public const string ColliderPathsName = "m_ColliderPaths";

		public PPtr<Collider2D> Collider;
		public IReadOnlyList<IReadOnlyList<IntPoint>> ColliderPaths => m_colliderPaths;

		private IntPoint[][] m_colliderPaths;
	}
}
