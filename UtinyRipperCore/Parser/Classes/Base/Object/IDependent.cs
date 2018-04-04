using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public interface IDependent
	{
		IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog);
	}
}
