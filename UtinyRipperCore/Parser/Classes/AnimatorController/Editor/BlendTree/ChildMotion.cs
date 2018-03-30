using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class ChildMotion : IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Motion", Motion.ExportYAML(exporter));
			node.Add("m_Threshold", Threshold);
			node.Add("m_Position", Position.ExportYAML(exporter));
			node.Add("m_TimeScale", TimeScale);
			node.Add("m_CycleOffset", CycleOffset);
			node.Add("m_DirectBlendParameter", DirectBlendParameter);
			node.Add("m_Mirror", Mirror);
			return node;
		}

		public float Threshold { get; private set; }
		public float TimeScale { get; private set; }
		public float CycleOffset { get; private set; }
		public string DirectBlendParameter { get; private set; }
		public bool Mirror { get; private set; }

		public PPtr<Motion> Motion;
		public Vector2f Position;
	}
}
