using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorControllerLayers : IYAMLExportable
	{
		public AnimatorControllerLayers(string name)
		{
			Name = name;

		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Name", Name);
			node.Add("m_StateMachine", StateMachine.ExportYAML(container));
			node.Add("m_Mask", Mask.ExportYAML(container));
			node.Add("m_Motions", Motions.ExportYAML(container));
			node.Add("m_Behaviours", Behaviours.ExportYAML(container));
			node.Add("m_BlendingMode", BlendingMode);
			node.Add("m_SyncedLayerIndex", SyncedLayerIndex);
			node.Add("m_DefaultWeight", DefaultWeight);
			node.Add("m_IKPass", IKPass);
			node.Add("m_SyncedLayerAffectsTiming", SyncedLayerAffectsTiming);
			node.Add("m_Controller", Controller.ExportYAML(container));
			return node;
		}

		public string Name { get; private set; }
		public IReadOnlyList<StateMotionPair> Motions => m_motions;
		public IReadOnlyList<StateBehavioursPair> Behaviours => m_behaviours;
		public int BlendingMode { get; private set; }
		public int SyncedLayerIndex { get; private set; }
		public float DefaultWeight { get; private set; }
		public bool IKPass { get; private set; }
		public bool SyncedLayerAffectsTiming { get; private set; }

		public PPtr<AnimatorStateMachine> StateMachine;
		public PPtr<AvatarMask> Mask;
		public PPtr<AnimatorController> Controller;

		private StateMotionPair[] m_motions;
		private StateBehavioursPair[] m_behaviours;
	}
}
