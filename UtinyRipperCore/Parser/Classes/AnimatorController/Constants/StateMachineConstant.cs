using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimatorControllers.Editor;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateMachineConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadConstantArray(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetStream stream)
		{
			m_stateConstantArray = stream.ReadArray<OffsetPtr<StateConstant>>();
			m_anyStateTransitionConstantArray = stream.ReadArray<OffsetPtr<TransitionConstant>>();
			if(IsReadConstantArray(stream.Version))
			{
				m_selectorStateConstantArray = stream.ReadArray<OffsetPtr<SelectorStateConstant>>();
			}

			DefaultState = (int)stream.ReadUInt32();
			MotionSetCount = stream.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<AnimatorTransition>[] GetEntryTransitions(VirtualSerializedFile file,
			AnimatorController controller, uint ID, IReadOnlyList<AnimatorState> states)
		{
			if (IsReadConstantArray(controller.File.Version))
			{
				foreach (OffsetPtr<SelectorStateConstant> selectorPtr in SelectorStateConstantArray)
				{
					SelectorStateConstant selector = selectorPtr.Instance;
					if (selector.FullPathID == ID && selector.IsEntry)
					{
						PPtr<AnimatorTransition>[] transitions = new PPtr<AnimatorTransition>[selector.TransitionConstantArray.Count - 1];
						for(int i = 0; i < selector.TransitionConstantArray.Count - 1; i++)
						{
							SelectorTransitionConstant selectorTrans = selector.TransitionConstantArray[i].Instance;
							AnimatorTransition transition = new AnimatorTransition(file, controller, selectorTrans, states);
							transitions[i] = PPtr<AnimatorTransition>.CreateVirtualPointer(transition);
						}
						return transitions;
					}
				}
			}
			return new PPtr<AnimatorTransition>[0];
		}

		public IReadOnlyList<OffsetPtr<StateConstant>> StateConstantArray => m_stateConstantArray;
		public IReadOnlyList<OffsetPtr<TransitionConstant>> AnyStateTransitionConstantArray => m_anyStateTransitionConstantArray;
		public IReadOnlyList<OffsetPtr<SelectorStateConstant>> SelectorStateConstantArray => m_selectorStateConstantArray;
		public int DefaultState { get; private set; }
		public uint MotionSetCount { get; private set; }

		private OffsetPtr<StateConstant>[] m_stateConstantArray;
		private OffsetPtr<TransitionConstant>[] m_anyStateTransitionConstantArray;
		private OffsetPtr<SelectorStateConstant>[] m_selectorStateConstantArray;
	}
}
