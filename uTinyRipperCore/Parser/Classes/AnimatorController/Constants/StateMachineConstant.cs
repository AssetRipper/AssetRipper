using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

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
		public static bool HasConstantArray(Version version) => version.IsGreaterEqual(5);

		public void Read(AssetReader reader)
		{
			StateConstantArray = reader.ReadAssetArray<OffsetPtr<StateConstant>>();
			AnyStateTransitionConstantArray = reader.ReadAssetArray<OffsetPtr<TransitionConstant>>();
			if (HasConstantArray(reader.Version))
			{
				SelectorStateConstantArray = reader.ReadAssetArray<OffsetPtr<SelectorStateConstant>>();
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
			if (HasConstantArray(parameters.Version))
			{
				foreach (OffsetPtr<SelectorStateConstant> selectorPtr in SelectorStateConstantArray)
				{
					SelectorStateConstant selector = selectorPtr.Instance;
					if (selector.FullPathID == parameters.ID && selector.IsEntry)
					{
						PPtr<AnimatorTransition>[] transitions = new PPtr<AnimatorTransition>[selector.TransitionConstantArray.Length - 1];
						for(int i = 0; i < selector.TransitionConstantArray.Length - 1; i++)
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
			return Array.Empty<PPtr<AnimatorTransition>>();
		}

		/// <summary>
		/// All states except Entry and Exit 
		/// </summary>
		public OffsetPtr<StateConstant>[] StateConstantArray { get; set; }
		public OffsetPtr<TransitionConstant>[] AnyStateTransitionConstantArray { get; set; }
		/// <summary>
		/// Entry [StateMachineIndex * 2 + 0] and Exit [StateMachineIndex * 2 + 1] pair for each SubStateMachine
		/// </summary>
		public OffsetPtr<SelectorStateConstant>[] SelectorStateConstantArray { get; set; }
		public int DefaultState { get; set; }
		public uint MotionSetCount { get; set; }
	}
}
