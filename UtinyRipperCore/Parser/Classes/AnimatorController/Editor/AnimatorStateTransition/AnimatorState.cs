using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorState : NamedObject
	{
		public AnimatorState(ISerializedFile file) :
			base(CreateAssetsInfo(file))
		{
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		private static AssetInfo CreateAssetsInfo(ISerializedFile file)
		{
			return new AssetInfo(file, 0, ClassIDType.AnimatorState);
		}

		public override void Read(AssetStream stream)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.InsertSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Speed", Speed);
			node.Add("m_CycleOffset", CycleOffset);
			node.Add("m_Transitions", Transitions.ExportYAML(exporter));
			node.Add("m_StateMachineBehaviours", StateMachineBehaviours.ExportYAML(exporter));
			node.Add("m_Position", Position.ExportYAML(exporter));
			node.Add("m_IKOnFeet", IKOnFeet);
			node.Add("m_WriteDefaultValues", WriteDefaultValues);
			node.Add("m_Mirror", Mirror);
			node.Add("m_SpeedParameterActive", SpeedParameterActive);
			node.Add("m_MirrorParameterActive", MirrorParameterActive);
			node.Add("m_CycleOffsetParameterActive", CycleOffsetParameterActive);
			node.Add("m_TimeParameterActive", TimeParameterActive);
			node.Add("m_Motion", Motion.ExportYAML(exporter));
			node.Add("m_Tag", Tag);
			node.Add("m_SpeedParameter", SpeedParameter);
			node.Add("m_MirrorParameter", MirrorParameter);
			node.Add("m_CycleOffsetParameter", CycleOffsetParameter);
			node.Add("m_TimeParameter", TimeParameter);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public float Speed { get; private set; }
		public float CycleOffset { get; private set; }
		public IReadOnlyList<PPtr<AnimatorStateTransition>> Transitions => m_transition;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		public bool IKOnFeet { get; private set; }
		public bool WriteDefaultValues { get; private set; }
		public bool Mirror { get; private set; }
		public bool SpeedParameterActive { get; private set; }
		public bool MirrorParameterActive { get; private set; }
		public bool CycleOffsetParameterActive { get; private set; }
		public bool TimeParameterActive { get; private set; }
		public string Tag { get; private set; }
		public string SpeedParameter { get; private set; }
		public string MirrorParameter { get; private set; }
		public string CycleOffsetParameter { get; private set; }
		public string TimeParameter { get; private set; }

		public Vector3f Position;
		public PPtr<Motion> Motion;

		private PPtr<AnimatorStateTransition>[] m_transition;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
