using AssetRipper.IO.Endian;
using System.Collections.Generic;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public interface ITypeTree : IReadOnlyList<ITypeTreeNode>, IEndianReadable, IEndianWritable
{
}
