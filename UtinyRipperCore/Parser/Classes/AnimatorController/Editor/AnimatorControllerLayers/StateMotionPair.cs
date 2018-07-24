using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateMotionPair : IYAMLExportable
	{
		public StateMotionPair(AnimatorState state, Motion motion)
		{
			if(state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (motion == null)
			{
				throw new ArgumentNullException(nameof(motion));
			}
			State = new PPtr<AnimatorState>(state);
			Motion = new PPtr<Motion>(motion);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(container));
			node.Add("m_Motion", Motion.ExportYAML(container));
			return node;
		}

		public PPtr<AnimatorState> State;
		public PPtr<Motion> Motion;
	}
}
