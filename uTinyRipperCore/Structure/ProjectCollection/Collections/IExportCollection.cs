using System.Collections.Generic;
using uTinyRipper.Project.Classes;
using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Project
{
	public interface IExportCollection
	{
		bool Export(ProjectAssetContainer container, string dirPath);
		bool IsContains(Object asset);
		long GetExportID(Object asset);
		ExportPointer CreateExportPointer(Object asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<Object> Assets { get; }
		string Name { get; } 
	}
}
