using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateMachineConstant : IAssetReadable, IYAMLExportable
	{
		public struct Parameters
		{
			public uint ID { get; set; }
			public Version Version { get; set; }
			public IReadOnlyList<AnimatorState> States { get; set; }
			public IReadOnlyDictionary<uint, string> TOS { get; set; }
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadConstantArray(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			m_stateConstantArray = reader.ReadAssetArray<OffsetPtr<StateConstant>>();
			m_anyStateTransitionConstantArray = reader.ReadAssetArray<OffsetPtr<TransitionConstant>>();
			if(IsReadConstantArray(reader.Version))
			{
				m_selectorStateConstantArray = reader.ReadAssetArray<OffsetPtr<SelectorStateConstant>>();
			}

			DefaultState = (int)reader.ReadUInt32();
			MotionSetCount = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<AnimatorTransition>[] CreateEntryTransitions(VirtualSerializedFile file, Parameters parameters)
		{
			if (IsReadConstantArray(parameters.Version))
			{
				foreach (OffsetPtr<SelectorStateConstant> selectorPtr in SelectorStateConstantArray)
				{
					SelectorStateConstant selector = selectorPtr.Instance;
					if (selector.FullPathID == parameters.ID && selector.IsEntry)
					{
						PPtr<AnimatorTransition>[] transitions = new PPtr<AnimatorTransition>[selector.TransitionConstantArray.Count - 1];
						for(int i = 0; i < selector.TransitionConstantArray.Count - 1; i++)
						{
							SelectorTransitionConstant selectorTrans = selector.TransitionConstantArray[i].Instance;
							AnimatorTransition.Parameters transParameters = new AnimatorTransition.Parameters
							{
								StateMachine = this,
								States = parameters.States,
								TOS = parameters.TOS,
								Transition = selectorTrans,
								Version = parameters.Version,
							};
							AnimatorTransition transition = AnimatorTransition.CreateVirtualInstance(file, transParameters);
							transitions[i] = transition.File.CreatePPtr(transition);
						}
						return transitions;
					}
				}
			}
			return new PPtr<AnimatorTransition>[0];
		}

		/// <summary>
		/// All states except Entry and Exit 
		/// </summary>
		public IReadOnlyList<OffsetPtr<StateConstant>> StateConstantArray => m_stateConstantArray;
		public IReadOnlyList<OffsetPtr<TransitionConstant>> AnyStateTransitionConstantArray => m_anyStateTransitionConstantArray;
		/// <summary>
		/// Entry [StateMachineIndex * 2 + 0] and Exit [StateMachineIndex * 2 + 1] pair for each SubStateMachine
		/// </summary>
		public IReadOnlyList<OffsetPtr<SelectorStateConstant>> SelectorStateConstantArray => m_selectorStateConstantArray;
		public int DefaultState { get; private set; }
		public uint MotionSetCount { get; private set; }

		private OffsetPtr<StateConstant>[] m_stateConstantArray;
		private OffsetPtr<TransitionConstant>[] m_anyStateTransitionConstantArray;
		private OffsetPtr<SelectorStateConstant>[] m_selectorStateConstantArray;
	}
}
