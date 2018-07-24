using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimatorControllers;
using UtinyRipper.Classes.AnimatorControllers.Editor;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class AnimatorController : RuntimeAnimatorController
	{
		public AnimatorController(AssetInfo assetsInfo):
			base(assetsInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadStateMachineBehaviourVectorDescription(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b2 to 5.1.x and 5.4.0 and greater
		/// </summary>
		public static bool IsReadMultiThreadedStateMachine(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1) && version.IsLess(5, 2) || version.IsGreaterEqual(5, 4);
		}

		/*/// <summary>
		/// Less than 5.1.0
		/// </summary>
		private static bool IsReadAlign(Version version)
		{
			return version.IsLess(5, 1);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlignMultiThreadedStateMachine(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}*/

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 5;
			}
			
#warning unknown
			if (version.IsGreater(5, 0, 0, VersionType.Beta))
			{
				return 5;
			}
			if (version.IsEqual(5, 0, 0, VersionType.Beta))
			{
				return 4;
			}
#warning unknown
			if (version.IsGreaterEqual(5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			ControllerSize = stream.ReadUInt32();
			Controller.Read(stream);
			m_TOS.Clear();
			m_TOS.Read(stream);
			m_animationClips = stream.ReadArray<PPtr<AnimationClip>>();

			if (IsReadStateMachineBehaviourVectorDescription(stream.Version))
			{
				StateMachineBehaviourVectorDescription.Read(stream);
				m_stateMachineBehaviours = stream.ReadArray<PPtr<MonoBehaviour>>();
			}

			if (IsReadMultiThreadedStateMachine(stream.Version))
			{
				MultiThreadedStateMachine = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (PPtr<AnimationClip> clip in AnimationClips)
			{
				yield return clip.FetchDependency(file, isLog, ToLogString, "AnimationClips");
			}
			if (IsReadStateMachineBehaviourVectorDescription(file.Version))
			{
				foreach (PPtr<MonoBehaviour> behaviour in StateMachineBehaviours)
				{
					yield return behaviour.FetchDependency(file, isLog, ToLogString, "StateMachineBehaviours");
				}
			}
		}
		
		public PPtr<MonoBehaviour>[] GetStateBeahviours(int stateMachineIndex, int stateIndex)
		{
			if (IsReadStateMachineBehaviourVectorDescription(File.Version))
			{
				int layerIndex = Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				StateMachineConstant stateMachine = Controller.StateMachineArray[stateMachineIndex].Instance;
				StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;
				uint stateID = state.GetID(File.Version);
				foreach (KeyValuePair<StateKey, StateRange> pair in StateMachineBehaviourVectorDescription.StateMachineBehaviourRanges)
				{
					StateKey key = pair.Key;
					if (key.LayerIndex == layerIndex && key.StateID == stateID)
					{
						StateRange range = pair.Value;
						PPtr<MonoBehaviour>[] stateMachineBehaviours = new PPtr<MonoBehaviour>[range.Count];
						for (int i = 0; i < range.Count; i++)
						{
							int index = (int)StateMachineBehaviourVectorDescription.StateMachineBehaviourIndices[range.StartIndex + i];
							stateMachineBehaviours[i] = StateMachineBehaviours[index];
						}
						return stateMachineBehaviours;
					}
				}
			}
			return new PPtr<MonoBehaviour>[0];
		}

		public override bool IsContainsAnimationClip(AnimationClip clip)
		{
			foreach (PPtr<AnimationClip> clipPtr in AnimationClips)
			{
				if (clipPtr.IsObject(File, clip))
				{
					return true;
				}
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			AnimatorControllerExportCollection collection = (AnimatorControllerExportCollection)container.CurrentCollection;

			AnimatorControllerParameter[] @params = new AnimatorControllerParameter[Controller.Values.Instance.ValueArray.Count];
			for(int i = 0; i < Controller.Values.Instance.ValueArray.Count; i++)
			{
				@params[i] = new AnimatorControllerParameter(this, i);
			}

			AnimatorControllerLayers[] layers = new AnimatorControllerLayers[Controller.LayerArray.Count];
			for(int i = 0; i < Controller.LayerArray.Count; i++)
			{
				int stateMachineIndex = Controller.LayerArray[i].Instance.StateMachineIndex;
				AnimatorStateMachine stateMachine = collection.StateMachines[stateMachineIndex];
				layers[i] = new AnimatorControllerLayers(stateMachine, this, i);
			}

			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_AnimatorParameters", @params.ExportYAML(container));
			node.Add("m_AnimatorLayers", layers.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "controller";

		public uint ControllerSize { get; private set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;
		public IReadOnlyList<PPtr<AnimationClip>> AnimationClips => m_animationClips;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		public bool MultiThreadedStateMachine { get; private set; }

		public ControllerConstant Controller;
		public StateMachineBehaviourVectorDescription StateMachineBehaviourVectorDescription;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();

		private PPtr<AnimationClip>[] m_animationClips;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}