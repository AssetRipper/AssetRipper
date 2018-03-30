using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public abstract class Component : EditorExtension
	{
		protected Component(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			GameObject.Read(stream);
		}

		public sealed override void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			base.ExportBinary(exporter, stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return GameObject.GetObject(file);
		}

		public GameObject GetRoot()
		{
			GameObject go = GameObject.GetObject(File);
			return go.GetRoot();
		}

		public int GetRootDepth()
		{
			GameObject go = GameObject.GetObject(File);
			return go.GetRootDepth();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_GameObject", GameObject.ExportYAML(exporter));
			return node;
		}

		public sealed override string ExportExtension => throw new NotSupportedException();

		public PPtr<GameObject> GameObject;
	}
}
