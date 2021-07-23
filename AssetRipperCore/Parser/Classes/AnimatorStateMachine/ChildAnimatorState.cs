using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Classes.Misc.Serializable;
using AssetRipper.Parser.Classes.Utils.Extensions;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System;
using Version = AssetRipper.Parser.Files.Version;

namespace AssetRipper.Parser.Classes.AnimatorStateMachine
{
	public sealed class ChildAnimatorState : IYAMLExportable
	{
		public ChildAnimatorState(AnimatorState state, Vector3f position)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			State = state.File.CreatePPtr(state);
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
			node.ForceAddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(StateName, State.ExportYAML(container));
			node.Add(PositionName, Position.ExportYAML(container));
			return node;
		}

		public const string StateName = "m_State";
		public const string PositionName = "m_Position";

		public PPtr<AnimatorState> State;
		public Vector3f Position;
	}
}
