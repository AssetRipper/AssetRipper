using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using System;

namespace uTinyRipper.Classes
{
	public sealed class AnimatorController : RuntimeAnimatorController
	{
		public AnimatorController(AssetInfo assetsInfo):
			base(assetsInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
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

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasStateMachineBehaviourVectorDescription(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0b2 to 5.1.x and 5.4.0 and greater
		/// </summary>
		public static bool HasMultiThreadedStateMachine(Version version)
		{
			// unknown start version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 2) || version.IsGreaterEqual(5, 4);
		}

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlignMultiThreadedStateMachine(Version version) => version.IsGreaterEqual(5, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			ControllerSize = reader.ReadUInt32();
			Controller.Read(reader);
			m_TOS.Clear();
			m_TOS.Read(reader);
			AnimationClips = reader.ReadAssetArray<PPtr<AnimationClip>>();

			if (HasStateMachineBehaviourVectorDescription(reader.Version))
			{
				StateMachineBehaviourVectorDescription.Read(reader);
				StateMachineBehaviours = reader.ReadAssetArray<PPtr<MonoBehaviour>>();
			}

			if (!IsAlignMultiThreadedStateMachine(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasMultiThreadedStateMachine(reader.Version))
			{
				MultiThreadedStateMachine = reader.ReadBoolean();
			}
			if(IsAlignMultiThreadedStateMachine(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(AnimationClips, AnimationClipsName))
			{
				yield return asset;
			}
			if (HasStateMachineBehaviourVectorDescription(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(StateMachineBehaviours, StateMachineBehavioursName))
				{
					yield return asset;
				}
			}
		}

		public PPtr<MonoBehaviour>[] GetStateBehaviours(int layerIndex)
		{
			if (HasStateMachineBehaviourVectorDescription(File.Version))
			{
				uint layerID = Controller.LayerArray[layerIndex].Instance.Binding;
				StateKey key = new StateKey(layerIndex, layerID);
				if (StateMachineBehaviourVectorDescription.StateMachineBehaviourRanges.TryGetValue(key, out StateRange range))
				{
					return GetStateBehaviours(range);
				}
			}
			return Array.Empty<PPtr<MonoBehaviour>>();
		}

		public PPtr<MonoBehaviour>[] GetStateBehaviours(int stateMachineIndex, int stateIndex)
		{
			if (HasStateMachineBehaviourVectorDescription(File.Version))
			{
				int layerIndex = Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				StateMachineConstant stateMachine = Controller.StateMachineArray[stateMachineIndex].Instance;
				StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;
				uint stateID = state.GetID(File.Version);
				StateKey key = new StateKey(layerIndex, stateID);
				if (StateMachineBehaviourVectorDescription.StateMachineBehaviourRanges.TryGetValue(key, out StateRange range))
				{
					return GetStateBehaviours(range);
				}
			}
			return Array.Empty<PPtr<MonoBehaviour>>();
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

			AnimatorControllerParameter[] @params = new AnimatorControllerParameter[Controller.Values.Instance.ValueArray.Length];
			for(int i = 0; i < Controller.Values.Instance.ValueArray.Length; i++)
			{
				@params[i] = new AnimatorControllerParameter(this, i);
			}

			AnimatorControllerLayer[] layers = new AnimatorControllerLayer[Controller.LayerArray.Length];
			for(int i = 0; i < Controller.LayerArray.Length; i++)
			{
				int stateMachineIndex = Controller.LayerArray[i].Instance.StateMachineIndex;
				AnimatorStateMachine stateMachine = collection.StateMachines[stateMachineIndex];
				layers[i] = new AnimatorControllerLayer(stateMachine, this, i);
			}

			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AnimatorParametersName, @params.ExportYAML(container));
			node.Add(AnimatorLayersName, layers.ExportYAML(container));
			return node;
		}

		private PPtr<MonoBehaviour>[] GetStateBehaviours(StateRange range)
		{
			PPtr<MonoBehaviour>[] stateMachineBehaviours = new PPtr<MonoBehaviour>[range.Count];
			for (int i = 0; i < range.Count; i++)
			{
				int index = (int)StateMachineBehaviourVectorDescription.StateMachineBehaviourIndices[range.StartIndex + i];
				stateMachineBehaviours[i] = StateMachineBehaviours[index];
			}
			return stateMachineBehaviours;
		}

		public override string ExportExtension => "controller";

		public uint ControllerSize { get; set; }
		public IReadOnlyDictionary<uint, string> TOS => m_TOS;
		public PPtr<AnimationClip>[] AnimationClips { get; set; }
		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }
		public bool MultiThreadedStateMachine { get; set; }

		public const string AnimatorParametersName = "m_AnimatorParameters";
		public const string AnimatorLayersName = "m_AnimatorLayers";
		public const string AnimationClipsName = "m_AnimationClips";
		public const string StateMachineBehaviourVectorDescriptionName = "m_StateMachineBehaviourVectorDescription";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";

		public ControllerConstant Controller;
		public StateMachineBehaviourVectorDescription StateMachineBehaviourVectorDescription;

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();
	}
}