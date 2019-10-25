using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public interface IDependent
	{
		IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog);
	}
}
