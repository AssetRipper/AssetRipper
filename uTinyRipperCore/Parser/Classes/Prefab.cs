using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// 2018.3 - first introduction
	/// </summary>
	public sealed class Prefab : Object
	{
		public Prefab(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RootGameObject.Read(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			RootGameObject.Write(writer);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(RootGameObject, RootGameObjectName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.Prefab;

		public const string RootGameObjectName = "m_RootGameObject";

		public PPtr<GameObject> RootGameObject;
	}
}
