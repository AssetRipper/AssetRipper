using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct AABB : IScriptStructure
	{
		public AABB(Vector3f center, Vector3f extent)
		{
			Center = center;
			Extent = extent;
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
			return this;
		}

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.BoundsName;

		public Vector3f Center;
		public Vector3f Extent;
	}
}
