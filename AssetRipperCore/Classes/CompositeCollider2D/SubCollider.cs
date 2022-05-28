using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.CompositeCollider2D
{
	public sealed class SubCollider : IAssetReadable, IYamlExportable, IDependent
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ColliderName, Collider.ExportYaml(container));
			if (HasDoubleColliderPath(container.ExportVersion))
			{
				node.Add(ColliderPathsName, ColliderPaths.ExportYaml(container));
			}
			else
			{
				IReadOnlyList<IntPoint> colliderPaths = ColliderPaths.Length == 0 ? System.Array.Empty<IntPoint>() : ColliderPaths[0];
				node.Add(ColliderPathsName, colliderPaths.ExportYaml(container));
			}
			return node;
		}

		public const string ColliderName = "m_Collider";
		public const string ColliderPathsName = "m_ColliderPaths";

		public IntPoint[][] ColliderPaths { get; set; }

		public PPtr<Collider2D> Collider = new();
	}
}
