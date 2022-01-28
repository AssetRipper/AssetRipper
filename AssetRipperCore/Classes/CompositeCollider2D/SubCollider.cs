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
	public sealed class SubCollider : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool HasDoubleColliderPath(UnityVersion version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		public void Read(AssetReader reader)
		{
			Collider.Read(reader);
			ColliderPaths = reader.ReadAssetArrayArray<IntPoint>();
			reader.AlignStream();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Collider, ColliderName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ColliderName, Collider.ExportYAML(container));
			if (HasDoubleColliderPath(container.ExportVersion))
			{
				node.Add(ColliderPathsName, ColliderPaths.ExportYAML(container));
			}
			else
			{
				IReadOnlyList<IntPoint> colliderPaths = ColliderPaths.Length == 0 ? System.Array.Empty<IntPoint>() : ColliderPaths[0];
				node.Add(ColliderPathsName, colliderPaths.ExportYAML(container));
			}
			return node;
		}

		public const string ColliderName = "m_Collider";
		public const string ColliderPathsName = "m_ColliderPaths";

		public IntPoint[][] ColliderPaths { get; set; }

		public PPtr<Collider2D> Collider = new();
	}
}
