using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public interface ITypeTree : IReadOnlyList<ITypeTreeNode>, IEndianWritable
{
}
