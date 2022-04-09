using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer;
using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerParameter;
using AssetRipper.Core.Classes.AnimatorController.State;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.AnimatorController
{
	public sealed class AnimatorController : RuntimeAnimatorController
	{
		public AnimatorController(AssetInfo assetsInfo) : base(assetsInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final))
			{
				return 5;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta))
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
		public static bool HasStateMachineBehaviourVectorDescription(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0b2 to 5.1.x and 5.4.0 and greater
		/// </summary>
		public static bool HasMultiThreadedStateMachine(UnityVersion version)
		{
			// unknown start version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final) && version.IsLess(5, 2) || version.IsGreaterEqual(5, 4);
		}

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlignMultiThreadedStateMachine(UnityVersion version) => version.IsGreaterEqual(5, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			ControllerSize = reader.ReadUInt32();
			Controller.Read(reader);
			m_TOS.Clear();
			m_TOS.Read(reader);
			AnimationClips = reader.ReadAssetArray<PPtr<AnimationClip.AnimationClip>>();

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
			if (IsAlignMultiThreadedStateMachine(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(AnimationClips, AnimationClipsName))
			{
				yield return asset;
			}
			if (HasStateMachineBehaviourVectorDescription(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(StateMachineBehaviours, StateMachineBehavioursName))
				{
					yield return asset;
				}
			}
		}

		public PPtr<MonoBehaviour>[] GetStateBehaviours(int layerIndex)
		{
			if (HasStateMachineBehaviourVectorDescription(SerializedFile.Version))
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
			if (HasStateMachineBehaviourVectorDescription(SerializedFile.Version))
			{
				int layerIndex = Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				StateMachineConstant stateMachine = Controller.StateMachineArray[stateMachineIndex].Instance;
				StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;
				uint stateID = state.GetID(SerializedFile.Version);
				StateKey key = new StateKey(layerIndex, stateID);
				if (StateMachineBehaviourVectorDescription.StateMachineBehaviourRanges.TryGetValue(key, out StateRange range))
				{
					return GetStateBehaviours(range);
				}
			}
			return Array.Empty<PPtr<MonoBehaviour>>();
		}

		public override bool IsContainsAnimationClip(AnimationClip.AnimationClip clip)
		{
			foreach (PPtr<AnimationClip.AnimationClip> clipPtr in AnimationClips)
			{
				if (clipPtr.IsAsset(SerializedFile, clip))
				{
					return true;
				}
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			if (container is null)
				throw new ArgumentNullException(nameof(container));

			AnimatorControllerExportCollection collection = container.CurrentCollection as AnimatorControllerExportCollection;

			if (collection == null)
				throw new NotSupportedException($"Container is of type {container.GetType()}. It must be an animator controller export collection.");

			AnimatorControllerParameter[] @params = new AnimatorControllerParameter[Controller.Values.Instance.ValueArray.Length];
			for (int i = 0; i < Controller.Values.Instance.ValueArray.Length; i++)
			{
				@params[i] = new AnimatorControllerParameter(this, i);
			}

			AnimatorControllerLayer[] layers = new AnimatorControllerLayer[Controller.LayerArray.Length];
			for (int i = 0; i < Controller.LayerArray.Length; i++)
			{
				int stateMachineIndex = Controller.LayerArray[i].Instance.StateMachineIndex;
				AnimatorStateMachine.AnimatorStateMachine stateMachine = collection.StateMachines[stateMachineIndex];
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
		public PPtr<AnimationClip.AnimationClip>[] AnimationClips { get; set; }
		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }
		public bool MultiThreadedStateMachine { get; set; }

		public const string AnimatorParametersName = "m_AnimatorParameters";
		public const string AnimatorLayersName = "m_AnimatorLayers";
		public const string AnimationClipsName = "m_AnimationClips";
		public const string StateMachineBehaviourVectorDescriptionName = "m_StateMachineBehaviourVectorDescription";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";

		public ControllerConstant Controller = new();
		public StateMachineBehaviourVectorDescription StateMachineBehaviourVectorDescription = new();

		private readonly Dictionary<uint, string> m_TOS = new Dictionary<uint, string>();
	}
}
