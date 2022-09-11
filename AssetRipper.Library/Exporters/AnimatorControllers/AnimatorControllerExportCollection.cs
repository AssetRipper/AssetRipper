using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
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
using AssetRipper.SourceGenerated.Subclasses.PPtr_State_;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;

namespace AssetRipper.Library.Exporters.AnimatorControllers
{
	public sealed class AnimatorControllerExportCollection : AssetsExportCollection
	{
		public AnimatorControllerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IUnityObjectBase asset) : this(assetExporter, virtualFile, (IAnimatorController)asset) { }

		public AnimatorControllerExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, IAnimatorController asset) : base(assetExporter, asset)
		{
			IControllerConstant controller = asset.Controller_C91;
			AccessListBase<IOffsetPtr_StateMachineConstant> stateMachinesConst = controller.StateMachineArray;
			StateMachines = new IAnimatorStateMachine[stateMachinesConst.Count];
			for (int i = 0; i < stateMachinesConst.Count; i++)
			{
				IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateAnimatorStateMachine(virtualFile, asset, i);
				StateMachines[i] = stateMachine;
			}

			for (int i = 0; i < StateMachines.Length; i++)
			{
				IAnimatorStateMachine stateMachine = StateMachines[i];
				IStateMachineConstant stateMachineConstant = asset.Controller_C91.StateMachineArray[i].Data;
				AddAsset(stateMachine);
				if (stateMachine.Has_StateMachineBehaviours_C1107())
				{
					AddBehaviours(asset, stateMachine.StateMachineBehaviours_C1107);
				}

				if (stateMachine.Has_AnyStateTransitions_C1107())
				{
					foreach (PPtr_AnimatorStateTransition_ transitionPtr in stateMachine.AnyStateTransitions_C1107)
					{
						IAnimatorStateTransition transition = transitionPtr.GetAsset(virtualFile);
						AddAsset(transition);
					}
				}
				if (stateMachine.Has_EntryTransitions_C1107())
				{
					foreach (PPtr_AnimatorTransition_ transitionPtr in stateMachine.EntryTransitions_C1107)
					{
						IAnimatorTransition transition = transitionPtr.GetAsset(virtualFile);
						AddAsset(transition);
					}
				}

				if (stateMachine.Has_ChildStates_C1107())
				{
					for (int j = 0; j < stateMachine.ChildStates_C1107.Count; j++)
					{
						PPtr_AnimatorState_ statePtr = stateMachine.ChildStates_C1107[j].State;
						IAnimatorState state = statePtr.GetAsset(virtualFile);
						IStateConstant stateConstant = stateMachineConstant.StateConstantArray[j].Data;
						AddAsset(state);
						if (state.Has_StateMachineBehaviours_C1102())
						{
							AddBehaviours(asset, state.StateMachineBehaviours_C1102);
						}

						if (state.Has_Motion_C1102() && state.Motion_C1102.IsVirtual())
						{
							Motion motion = state.Motion_C1102.GetAsset(virtualFile);
							AddBlendTree(virtualFile, (IBlendTree)motion);
						}

						if (state.Has_Transitions_C1102())
						{
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
				else if (stateMachine.Has_States_C1107())
				{
					for (int j = 0; j < stateMachine.States_C1107.Count; j++)
					{
						PPtr_State_ statePtr = stateMachine.States_C1107[j];
						IAnimatorState state = statePtr.GetAsset(virtualFile);
						IStateConstant stateConstant = stateMachineConstant.StateConstantArray[j].Data;
						AddAsset(state);
						if (state.Has_StateMachineBehaviours_C1102())
						{
							AddBehaviours(asset, state.StateMachineBehaviours_C1102);
						}

						if (state.Has_Motion_C1102() && state.Motion_C1102.IsVirtual())
						{
							Motion motion = state.Motion_C1102.GetAsset(virtualFile);
							AddBlendTree(virtualFile, (IBlendTree)motion);
						}

						if (state.Has_Transitions_C1102())
						{
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
				IMonoBehaviour? behaviour = pbehaviour.TryGetAsset(asset.SerializedFile);
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

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			Process((IAnimatorController)Asset);
			return base.ExportInner(container, filePath, dirPath);
		}

		private void Process(IAnimatorController controller)
		{
			controller.AnimatorParameters_C91.Clear();
			controller.AnimatorParameters_C91.Capacity = controller.Controller_C91.Values.Data.ValueArray.Count;
			for (int i = 0; i < controller.Controller_C91.Values.Data.ValueArray.Count; i++)
			{
				controller.AnimatorParameters_C91.AddNew().Initialize(controller, i);
			}

			controller.AnimatorLayers_C91.Clear();
			controller.AnimatorLayers_C91.Capacity = controller.Controller_C91.LayerArray.Count;
			for (int i = 0; i < controller.Controller_C91.LayerArray.Count; i++)
			{
				uint stateMachineIndex = controller.Controller_C91.LayerArray[i].Data.StateMachineIndex;
				IAnimatorStateMachine stateMachine = StateMachines[stateMachineIndex];
				controller.AnimatorLayers_C91.AddNew().Initialize(stateMachine, controller, i);
			}
		}

		public IAnimatorStateMachine[] StateMachines { get; set; }
	}
}
