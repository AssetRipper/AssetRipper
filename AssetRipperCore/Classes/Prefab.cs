using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class Prefab : Object.Object
	{
		public Prefab(LayoutInfo layout) : base(layout) { }

		public Prefab(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasHideFlagsBehavior(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasContainsMissingSerializeReferenceTypes(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RootGameObject.Read(reader);

			if (HasHideFlagsBehavior(reader.Version))
			{
				HideFlagsBehavior = reader.ReadInt32();
			}

			if (HasContainsMissingSerializeReferenceTypes(reader.Version))
			{
				reader.ReadBoolean();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			RootGameObject.Write(writer);

			if (HasHideFlagsBehavior(writer.Version))
			{
				writer.Write(HideFlagsBehavior);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
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

		public PPtr<GameObject.GameObject> RootGameObject = new();
		public int HideFlagsBehavior { get; set; }
		public const string RootGameObjectName = "m_RootGameObject";
	}
}
