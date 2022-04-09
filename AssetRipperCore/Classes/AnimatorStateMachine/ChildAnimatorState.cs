using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;


namespace AssetRipper.Core.Classes.AnimatorStateMachine
{
	public sealed class ChildAnimatorState : IYAMLExportable
	{
		public ChildAnimatorState(AnimatorState state, Vector3f position)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			State = state.SerializedFile.CreatePPtr(state);
			Position = position;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
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

		public PPtr<AnimatorState> State = new();
		public Vector3f Position = new();
	}
}
