using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct AABB : IScriptStructure
	{
		public AABB(AABB copy)
		{
			Center = copy.Center;
			Extent = copy.Extent;
		}

		public void Read(AssetReader reader)
		{
			Center.Read(reader);
			Extent.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Center", Center.ExportYAML(container));
			node.Add("m_Extent", Extent.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public IScriptStructure CreateCopy()
		{
			return new AABB(this);
		}

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.BoundsName;

		public Vector3f Center;
		public Vector3f Extent;
	}
}
