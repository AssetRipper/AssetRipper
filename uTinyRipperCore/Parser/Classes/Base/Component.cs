using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Component : EditorExtension
	{
		protected Component(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GameObject.Read(reader);
		}

		public sealed override void ExportBinary(IExportContainer container, Stream stream)
		{
			base.ExportBinary(container, stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			if (!GameObject.IsNull)
			{
				yield return GameObject.GetAsset(file);
			}
		}

		public GameObject GetRoot()
		{
			GameObject go = GameObject.GetAsset(File);
			return go.GetRoot();
		}

		public int GetRootDepth()
		{
			GameObject go = GameObject.GetAsset(File);
			return go.GetRootDepth();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_GameObject", GameObject.ExportYAML(container));
			return node;
		}

		public override bool IsValid => !GameObject.IsNull;

		public override string ExportExtension => throw new NotSupportedException();

		public PPtr<GameObject> GameObject;
	}
}
