using System.Collections.Generic;

namespace uTinyRipper.Classes
{
	public interface IDependent
	{
		IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog);
	}
}
