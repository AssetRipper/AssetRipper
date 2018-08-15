using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.Classes.AnimatorControllers;
using UtinyRipper.Classes.AnimatorControllers.Editor;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.AssetExporters
{
	public class AnimatorControllerExportCollection : AssetsExportCollection
	{
		public AnimatorControllerExportCollection(IAssetExporter assetExporter, Object asset) :
			this(assetExporter, (AnimatorController)asset)
		{
		}

		public AnimatorControllerExportCollection(IAssetExporter assetExporter, AnimatorController asset) :
			base(assetExporter, asset, new NativeFormatImporter(asset))
		{
		}

		protected override string ExportInner(ProjectAssetContainer container, string filePath)
		{
			TryInitialize(container);
			return base.ExportInner(container, filePath);
		}

		private void TryInitialize(ProjectAssetContainer container)
		{
			if(m_initialized)
			{
				return;
			}

			AnimatorController asset = (AnimatorController)Asset;
			ControllerConstant controller = asset.Controller;
			IReadOnlyList<OffsetPtr<StateMachineConstant>> stateMachinesConst = controller.StateMachineArray;
			m_stateMachines = new AnimatorStateMachine[stateMachinesConst.Count];
			for (int i = 0; i < stateMachinesConst.Count; i++)
			{
				StateMachineConstant stateMachineConstant = stateMachinesConst[i].Instance;
				AnimatorStateMachine stateMachine = new AnimatorStateMachine(container.VirtualFile, asset, i);
				m_stateMachines[i] = stateMachine;
			}

			for (int i = 0; i < StateMachines.Count; i++)
			{
				AnimatorStateMachine stateMachine = StateMachines[i];
				StateMachineConstant stateMachineConstant = asset.Controller.StateMachineArray[i].Instance;
				AddAsset(stateMachine);

				foreach(PPtr<AnimatorStateTransition> transitionPtr in stateMachine.AnyStateTransitions)
				{
					AnimatorStateTransition transition = transitionPtr.GetAsset(container.VirtualFile);
					AddAsset(transition);
				}
				foreach (PPtr<AnimatorTransition> transitionPtr in stateMachine.EntryTransitions)
				{
					AnimatorTransition transition = transitionPtr.GetAsset(container.VirtualFile);
					AddAsset(transition);
				}

				for (int j = 0; j < stateMachine.ChildStates.Count; j++)
				{
					PPtr<AnimatorState> statePtr = stateMachine.ChildStates[j].State;
					AnimatorState state = statePtr.GetAsset(container.VirtualFile);
					StateConstant stateConstant = stateMachineConstant.StateConstantArray[j].Instance;
					AddAsset(state);
					
					if(state.Motion.IsVirtual)
					{
						AddBlendTree(container.VirtualFile, state.Motion.CastTo<BlendTree>());
					}

					for(int k = 0; k < state.Transitions.Count; k++)
					{
						PPtr<AnimatorStateTransition> transitionPtr = state.Transitions[k];
						AnimatorStateTransition transition = transitionPtr.GetAsset(container.VirtualFile);
						TransitionConstant transitionConstant = stateConstant.TransitionConstantArray[k].Instance;
						
						AddAsset(transition);
					}
				}
			}

			m_initialized = true;
		}

		private void AddBlendTree(VirtualSerializedFile file, PPtr<BlendTree> blendTreePtr)
		{
			BlendTree blendTree = blendTreePtr.GetAsset(file);
			AddAsset(blendTree);

			foreach (ChildMotion childMotion in blendTree.Childs)
			{
				if(childMotion.Motion.IsVirtual)
				{
					AddBlendTree(file, childMotion.Motion.CastTo<BlendTree>());
				}
			}
		}

		public IReadOnlyList<AnimatorStateMachine> StateMachines => m_stateMachines;

		private bool m_initialized = false;
		private AnimatorStateMachine[] m_stateMachines;
	}
}
