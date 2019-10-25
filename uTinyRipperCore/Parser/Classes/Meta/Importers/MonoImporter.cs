using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.YAML;

namespace uTinyRipper.Importers
{
	public class MonoImporter : DefaultImporter
	{
		public MonoImporter(MonoScript sctipt)
		{
			if(sctipt == null)
			{
				throw new ArgumentNullException(nameof(sctipt));
			}
			m_script = sctipt;
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("defaultReferences", YAMLSequenceNode.Empty);
			node.Add("executionOrder", m_script.ExecutionOrder);

			YAMLMappingNode instanceId = new YAMLMappingNode(MappingStyle.Flow);
			instanceId.Add("instanceID", 0);
			node.Add("icon", instanceId);
		}

		public override string Name => "MonoImporter";

		private readonly MonoScript m_script;
	}
}
