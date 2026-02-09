using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;

namespace AssetRipper.DocExtraction.DataStructures;

public abstract class ComplexTypeHistory : TypeHistory<DataMemberHistory, DataMemberDocumentation>
{
	public virtual IReadOnlyDictionary<string, DataMemberHistory> GetAllMembers(HistoryFile historyFile)
	{
		return Members;
	}

	public virtual IReadOnlyDictionary<string, DataMemberHistory> GetAllMembers(UnityVersion version, HistoryFile historyFile)
	{
		return Members;
	}
}
