using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class ChildAnimatorStateMachine : IYAMLExportable
	{
		public ChildAnimatorStateMachine(AnimatorStateMachine stateMachine, Vector3f position)
		{
			if (stateMachine == null)
			{
				throw new ArgumentNullException(nameof(stateMachine));
			}
			StateMachine = stateMachine.File.CreatePPtr(stateMachine);
			Position = position;
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 1;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_StateMachine", StateMachine.ExportYAML(container));
			node.Add("m_Position", Position.ExportYAML(container));
			return node;
		}

		public PPtr<AnimatorStateMachine> StateMachine;
		public Vector3f Position;
	}
}
