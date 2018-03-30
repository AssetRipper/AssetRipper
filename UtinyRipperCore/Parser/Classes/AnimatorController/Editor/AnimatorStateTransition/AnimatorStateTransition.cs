using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorStateTransition : AnimatorTransitionBase
	{
		public AnimatorStateTransition(ISerializedFile file) :
			base(CreateAssetsInfo(file))
		{
		}

		private static AssetInfo CreateAssetsInfo(ISerializedFile file)
		{
			return new AssetInfo(file, 0, ClassIDType.AnimatorStateTransition);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 3;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_TransitionDuration", TransitionDuration);
			node.Add("m_TransitionOffset", TransitionOffset);
			node.Add("m_ExitTime", ExitTime);
			node.Add("m_HasExitTime", HasExitTime);
			node.Add("m_HasFixedDuration", HasFixedDuration);
			node.Add("m_InterruptionSource", InterruptionSource);
			node.Add("m_OrderedInterruption", OrderedInterruption);
			node.Add("m_CanTransitionToSelf", CanTransitionToSelf);
			return node;
		}

		public float TransitionDuration { get; private set; }
		public float TransitionOffset { get; private set; }
		public float ExitTime { get; private set; }
		public bool HasExitTime { get; private set; }
		public bool HasFixedDuration { get; private set; }
		public bool InterruptionSource { get; private set; }
		public bool OrderedInterruption { get; private set; }
		public bool CanTransitionToSelf { get; private set; }
	}
}
