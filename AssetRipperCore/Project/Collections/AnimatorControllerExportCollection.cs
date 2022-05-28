using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorState_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateTransition_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour_;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;

namespace AssetRipper.Core.Project.Collections
{
	internal class AnimatorControllerExportCollection : AssetsExportCollection
	{
		public AnimatorControllerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IUnityObjectBase asset) : this(assetExporter, virtualFile, (IAnimatorController)asset) { }

		public AnimatorControllerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IAnimatorController asset) : base(assetExporter, asset)
		{
			IControllerConstant controller = asset.Controller_C91;
			AccessListBase<IOffsetPtr_StateMachineConstant> stateMachinesConst = controller.StateMachineArray;
			StateMachines = new IAnimatorStateMachine[stateMachinesConst.Count];
			for (int i = 0; i < stateMachinesConst.Count; i++)
			{
				IAnimatorStateMachine stateMachine = CreateVirtualStateMachine(virtualFile, asset, i);
				StateMachines[i] = stateMachine;
			}

			for (int i = 0; i < StateMachines.Length; i++)
			{
				IAnimatorStateMachine stateMachine = StateMachines[i];
				IStateMachineConstant stateMachineConstant = asset.Controller_C91.StateMachineArray[i].Data;
				AddAsset(stateMachine);
				AddBehaviours(asset, stateMachine.StateMachineBehaviours_C1107);

				foreach (PPtr_AnimatorStateTransition_ transitionPtr in stateMachine.AnyStateTransitions_C1107)
				{
					IAnimatorStateTransition transition = transitionPtr.GetAsset(virtualFile);
					AddAsset(transition);
				}
				foreach (PPtr_AnimatorTransition_ transitionPtr in stateMachine.EntryTransitions_C1107)
				{
					IAnimatorTransition transition = transitionPtr.GetAsset(virtualFile);
					AddAsset(transition);
				}

				for (int j = 0; j < stateMachine.ChildStates_C1107.Count; j++)
				{
					PPtr_AnimatorState_ statePtr = stateMachine.ChildStates_C1107[j].State;
					IAnimatorState state = statePtr.GetAsset(virtualFile);
					IStateConstant stateConstant = stateMachineConstant.StateConstantArray[j].Data;
					AddAsset(state);
					AddBehaviours(asset, state.StateMachineBehaviours_C1102);

					if (state.Motion_C1102.IsVirtual())
					{
						Motion motion = state.Motion_C1102.GetAsset(virtualFile);
						AddBlendTree(virtualFile, (IBlendTree)motion);
					}

					for (int k = 0; k < state.Transitions_C1102.Count; k++)
					{
						PPtr_AnimatorStateTransition_ transitionPtr = state.Transitions_C1102[k];
						IAnimatorStateTransition transition = transitionPtr.GetAsset(virtualFile);
						ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[k].Data;

						AddAsset(transition);
					}
				}
			}
		}

		private void AddBlendTree(VirtualSerializedFile virtualFile, IBlendTree blendTree)
		{
			AddAsset(blendTree);
			foreach (IChildMotion childMotion in blendTree.Childs_C206)
			{
				if (childMotion.Motion.IsVirtual())
				{
					Motion motion = childMotion.Motion.GetAsset(virtualFile);
					AddBlendTree(virtualFile, (IBlendTree)motion);
				}
			}
		}

		private void AddBehaviours(IAnimatorController asset, AssetList<PPtr_MonoBehaviour__5_0_0_f4> behaviours)
		{
			foreach (PPtr_MonoBehaviour__5_0_0_f4 pbehaviour in behaviours)
			{
				IMonoBehaviour? behaviour = pbehaviour.FindAsset(asset.SerializedFile);
				if (behaviour != null)
				{
#warning HACK: skip duplicates. remove it when AnimatorStateMachine's child StateMachines has been implemented
					if (!m_exportIDs.ContainsKey(behaviour.AssetInfo))
					{
						AddAsset(behaviour);
					}
				}
			}
		}

		private static IAnimatorStateMachine CreateStateMachine(LayoutInfo layout, AssetInfo assetInfo, IAnimatorController controller, int stateMachineIndex)
		{
			IAnimatorStateMachine generatedStateMachine = AnimatorStateMachineFactory.CreateAsset(layout.Version, assetInfo);
			generatedStateMachine.ObjectHideFlags = HideFlags.HideInHierarchy;
			/*
			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;

			int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			ILayerConstant layer = controller.Controller_C91.LayerArray[layerIndex].Data;
			generatedStateMachine.NameString = controller.TOS_C91[layer.Binding];

			IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;

			int stateCount = stateMachine.StateConstantArray.Count;
			int stateMachineCount = 0;
			int count = stateCount + stateMachineCount;
			int side = (int)System.Math.Ceiling(System.Math.Sqrt(count));

			List<IAnimatorState> states = new List<IAnimatorState>();
			generatedStateMachine.ChildStates_C1107.Clear();
			generatedStateMachine.ChildStates_C1107.Capacity = stateCount;
			for (int y = 0, stateIndex = 0; y < side && stateIndex < stateCount; y++)
			{
				for (int x = 0; x < side && stateIndex < stateCount; x++, stateIndex++)
				{
					Vector3f position = new Vector3f(x * StateOffset, y * StateOffset, 0.0f);
					IAnimatorState state = CreateVirtualState(virtualFile, controller, stateMachineIndex, stateIndex, position);
					ChildAnimatorState childState = new ChildAnimatorState(state, position);
					generatedStateMachine.ChildStates_C1107[stateIndex] = childState;
					states.Add(state);
				}
			}

#warning TODO: child StateMachines
			//generatedStateMachine.ChildStateMachines_C1107 = new ChildAnimatorStateMachine[stateMachineCount];

			// set destination state for transitions here because all states has become valid only now
			for (int i = 0; i < stateMachine.StateConstantArray.Count; i++)
			{
				IAnimatorState state = states[i];
				IStateConstant stateConstant = stateMachine.StateConstantArray[i].Data;
				PPtr_AnimatorStateTransition_[] transitions = new PPtr_AnimatorStateTransition_[stateConstant.TransitionConstantArray.Count];
				for (int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
				{
					ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Data;
					AnimatorStateTransition.Parameters parameters = new AnimatorStateTransition.Parameters
					{
						StateMachine = stateMachine,
						States = states,
						TOS = controller.TOS_C91,
						Transition = transitionConstant,
						Version = controller.SerializedFile.Version,
					};
					IAnimatorStateTransition transition = AnimatorStateTransition.CreateVirtualInstance(virtualFile, parameters);
					transitions[j] = new();
					transitions[j].CopyValues(transition.SerializedFile.CreatePPtr(transition));
				}
				state.Transitions_C1102.AddRange(transitions);
			}

			AnyStateTransitions = new PPtr<AnimatorStateTransition>[stateMachine.AnyStateTransitionConstantArray.Count];
			for (int i = 0; i < stateMachine.AnyStateTransitionConstantArray.Count; i++)
			{
				ITransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Data;
				AnimatorStateTransition.Parameters parameters = new AnimatorStateTransition.Parameters
				{
					StateMachine = stateMachine,
					States = states,
					TOS = controller.TOS_C91,
					Transition = transitionConstant,
					Version = controller.SerializedFile.Version,
				};
				IAnimatorStateTransition transition = AnimatorStateTransition.CreateVirtualInstance(virtualFile, parameters);
				AnyStateTransitions[i] = transition.SerializedFile.CreatePPtr(transition);
			}

			StateMachineConstant.Parameters stateParameters = new StateMachineConstant.Parameters
			{
				ID = layer.Binding,
				States = states,
				TOS = controller.TOS_C91,
				Version = controller.SerializedFile.Version,
			};
			generatedStateMachine.EntryTransitions_C1107 = stateMachine.CreateEntryTransitions(virtualFile, stateParameters);
#warning TEMP: remove comment when AnimatorStateMachine's child StateMachines has been implemented
			//StateMachineBehaviours = controller.GetStateBehaviours(layerIndex);
			generatedStateMachine.StateMachineBehaviours_C1107.Clear();

			generatedStateMachine.AnyStatePosition_C1107 = new Vector3f(0.0f, -StateOffset, 0.0f);
			generatedStateMachine.EntryPosition_C1107 = new Vector3f(StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ExitPosition_C1107 = new Vector3f(2.0f * StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ParentStateMachinePosition_C1107 = new Vector3f(0.0f, -2.0f * StateOffset, 0.0f);

			generatedStateMachine.DefaultState = generatedStateMachine.ChildStates_C1107.Count > 0 ? generatedStateMachine.ChildStates_C1107[(int)stateMachine.DefaultState].State : new();
			*/
			return generatedStateMachine;
		}

		public static IAnimatorStateMachine CreateVirtualStateMachine(VirtualSerializedFile virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			return virtualFile.CreateAsset((assetInfo) => CreateStateMachine(virtualFile.Layout, assetInfo, controller, stateMachineIndex));
		}
		/*
		private static IAnimatorState CreateState(LayoutInfo layout, AssetInfo assetInfo, IAnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position)
		{
			IAnimatorState generatedState = AnimatorStateFactory.CreateAsset(layout.Version, assetInfo);
			generatedState.ObjectHideFlags = HideFlags.HideInHierarchy;
			
			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;

			AssetDictionary<uint, Utf8String> TOS = controller.TOS_C91;
			if (!TOS.ContainsKey(0))
			{
				AssetDictionary<uint, Utf8String> tos = new AssetDictionary<uint, Utf8String>() { { 0, new Utf8String() } };
				tos.AddRange(controller.TOS_C91);
				TOS = tos;
			}
			IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;
			IStateConstant state = stateMachine.StateConstantArray[stateIndex].Data;

			generatedState.NameString = TOS[state.NameID];

			generatedState.Speed_C1102 = state.Speed;
			generatedState.CycleOffset_C1102 = state.CycleOffset;

			// skip Transitions because not all state exists at this moment
			
			// exclude StateMachine's behaviours
			int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			PPtr<MonoBehaviour>[] machineBehaviours = controller.GetStateBehaviours(layerIndex);
			PPtr<MonoBehaviour>[] stateBehaviours = controller.GetStateBehaviours(stateMachineIndex, stateIndex);
			List<PPtr<MonoBehaviour>> behaviours = new List<PPtr<MonoBehaviour>>(stateBehaviours.Length);
			foreach (PPtr<MonoBehaviour> ptr in stateBehaviours)
			{
#warning TEMP: remove comment when AnimatorStateMachine's child StateMachines has been implemented
				//if (!machineBehaviours.Contains(ptr))
				{
					behaviours.Add(ptr);
				}
			}
			generatedState.StateMachineBehaviours_C1102 = behaviours.ToArray();

			generatedState.Position_C1102 = position;
			generatedState.IKOnFeet_C1102 = state.IKOnFeet;
			generatedState.WriteDefaultValues_C1102 = state.GetWriteDefaultValues(controller.SerializedFile.Version);
			generatedState.Mirror_C1102 = state.Mirror;
			generatedState.SpeedParameterActive_C1102 = state.SpeedParamID > 0;
			generatedState.MirrorParameterActive_C1102 = state.MirrorParamID > 0;
			generatedState.CycleOffsetParameterActive_C1102 = state.CycleOffsetParamID > 0;
			generatedState.TimeParameterActive_C1102 = state.TimeParamID > 0;

			generatedState.Motion_C1102 = state.CreateMotion(virtualFile, controller, 0);

			generatedState.Tag_C1102 = TOS[state.TagID];
			generatedState.SpeedParameter_C1102 = TOS[state.SpeedParamID];
			generatedState.MirrorParameter_C1102 = TOS[state.MirrorParamID];
			generatedState.CycleOffsetParameter_C1102 = TOS[state.CycleOffsetParamID];
			generatedState.TimeParameter_C1102 = TOS[state.TimeParamID];
			
			return generatedState;
		}
		
		public static IAnimatorState CreateVirtualState(VirtualSerializedFile virtualFile, IAnimatorController controller, int stateMachineIndex,
			int stateIndex, Vector3f position)
		{
			return virtualFile.CreateAsset((assetInfo) => CreateState(virtualFile.Layout, assetInfo, controller, stateMachineIndex, stateIndex, position));
		}
		*/
		public IAnimatorStateMachine[] StateMachines { get; set; }
	}
}
