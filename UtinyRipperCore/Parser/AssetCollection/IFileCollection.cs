using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper
{
	public interface IFileCollection
	{
		ISerializedFile GetSerializedFile(FileIdentifier fileRef);
		ISerializedFile FindSerializedFile(FileIdentifier fileRef);
		ResourcesFile FindResourcesFile(ISerializedFile file, string fileName);

		IReadOnlyList<ISerializedFile> Files { get; }
	}
}
