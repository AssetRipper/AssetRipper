using AssetRipper.SourceGenerated.Subclasses.SelectorStateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StateMachineConstantExtensions
{
	public static int StateMachineCount(this IStateMachineConstant stateMachineConstant)
	{
		if (!stateMachineConstant.Has_SelectorStateConstantArray())
		{
			// not needed for Unity 4.x- SubStateMachine reconstruction
			return 0;
		}

		// SelectorStateConstantArray contains Entry and Exit points for StateMachines
		// SelectorStateConstantArray = [Entry1, Exit1, Entry2, Exit2, ...]
		// just in case, next code handles StateMachine missing Entry or Exit SelectorStateConstant
		int stateMachineCount = 0;
		uint lastFullPathID = 0;
		foreach (SelectorStateConstant ssc in stateMachineConstant.SelectorStateConstantArray)
		{
			if (lastFullPathID != ssc.FullPathID)
			{
				lastFullPathID = ssc.FullPathID;
				stateMachineCount++;
			}
		}
		return stateMachineCount;
	}
}
