using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper
{
	public interface IAssetCollection
	{
		ISerializedFile GetSerializedFile(FileIdentifier fileRef);
		ISerializedFile FindSerializedFile(FileIdentifier fileRef);
		ResourcesFile FindResourcesFile(ISerializedFile file, string fileName);
		
		IEnumerable<Object> FetchAssets();
	}
}
