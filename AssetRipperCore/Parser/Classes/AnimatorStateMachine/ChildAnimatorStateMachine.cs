using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Classes.Misc.Serializable;
using AssetRipper.Parser.Classes.Utils.Extensions;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.YAML;
using System;
using Version = AssetRipper.Parser.Files.File.Version.Version;

namespace AssetRipper.Parser.Classes.AnimatorStateMachine
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

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 1;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(StateMachineName, StateMachine.ExportYAML(container));
			node.Add(PositionName, Position.ExportYAML(container));
			return node;
		}

		public const string StateMachineName = "m_StateMachine";
		public const string PositionName = "m_Position";

		public PPtr<AnimatorStateMachine> StateMachine;
		public Vector3f Position;
	}
}
