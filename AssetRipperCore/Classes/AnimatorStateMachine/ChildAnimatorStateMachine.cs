using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;


namespace AssetRipper.Core.Classes.AnimatorStateMachine
{
	public sealed class ChildAnimatorStateMachine : IYamlExportable
	{
		public ChildAnimatorStateMachine(AnimatorStateMachine stateMachine, Vector3f position)
		{
			if (stateMachine == null)
			{
				throw new ArgumentNullException(nameof(stateMachine));
			}
			StateMachine = stateMachine.SerializedFile.CreatePPtr(stateMachine);
			Position = position;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO: ToSerializedVersion
			return 1;
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(StateMachineName, StateMachine.ExportYaml(container));
			node.Add(PositionName, Position.ExportYaml(container));
			return node;
		}

		public const string StateMachineName = "m_StateMachine";
		public const string PositionName = "m_Position";

		public PPtr<AnimatorStateMachine> StateMachine = new();
		public Vector3f Position = new();
	}
}
