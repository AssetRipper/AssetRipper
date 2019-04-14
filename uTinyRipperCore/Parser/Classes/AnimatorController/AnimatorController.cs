using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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
			// unknown start version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 2) || version.IsGreaterEqual(5, 4);
		}

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlignMultiThreadedStateMachine(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 5;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Beta))
			{
				return 4;
			}
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			ControllerSize = reader.ReadUInt32();
			Controller.Read(reader);
			m_TOS.Clear();
			m_TOS.Read(reader);
			m_animationClips = reader.ReadAssetArray<PPtr<AnimationClip>>();

			if (IsReadStateMachineBehaviourVectorDescription(reader.Version))
			{
				StateMachineBehaviourVectorDescription.Read(reader);
				m_stateMachineBehaviours = reader.ReadAssetArray<PPtr<MonoBehaviour>>();
			}

			if (!IsAlignMultiThreadedStateMachine(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadMultiThreadedStateMachine(reader.Version))
			{
				MultiThreadedStateMachine = reader.ReadBoolean();
			}
			if(IsAlignMultiThreadedStateMachine(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
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
				if (clipPtr.IsAsset(File, clip))
				{
					return true;
				}
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(AnimatorParametersName, @params.ExportYAML(container));
			node.Add(AnimatorLayersName, layers.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "controller";

		public uint ControllerSize { get; private set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;
		public IReadOnlyList<PPtr<AnimationClip>> AnimationClips => m_animationClips;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		public bool MultiThreadedStateMachine { get; private set; }

		public const string AnimatorParametersName = "m_AnimatorParameters";
		public const string AnimatorLayersName = "m_AnimatorLayers";

		public ControllerConstant Controller;
		public StateMachineBehaviourVectorDescription StateMachineBehaviourVectorDescription;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();

		private PPtr<AnimationClip>[] m_animationClips;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}