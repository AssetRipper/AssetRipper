using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Collider.FetchDependency(file, isLog, () => nameof(SubCollider), "m_Collider");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Collider", Collider.ExportYAML(container));
			if (IsReadDoubleColliderPath(container.ExportVersion))
			{
				node.Add("m_ColliderPaths", ColliderPaths.ExportYAML(container));
			}
			else
			{
				IReadOnlyList<IntPoint> colliderPaths = ColliderPaths.Count == 0 ? new IntPoint[0] : ColliderPaths[0];
				node.Add("m_ColliderPaths", colliderPaths.ExportYAML(container));
			}
			return node;
		}

		public PPtr<Collider2D> Collider;
		public IReadOnlyList<IReadOnlyList<IntPoint>> ColliderPaths => m_colliderPaths;

		private IntPoint[][] m_colliderPaths;
	}
}
