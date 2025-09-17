using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.AssemblyDumper;

internal class UniversalNodeComparer
{
	private const TransferMetaFlags MetaMask = TransferMetaFlags.TreatIntegerValueAsBoolean
		| TransferMetaFlags.AlignBytes
		| TransferMetaFlags.TransferUsingFlowMappingStyle;

	public static bool Equals(UniversalNode? left, UniversalNode? right, bool root)
	{
		if (left is null || right is null)
		{
			return left is null && right is null;
		}
		if (!root && left.OriginalName != right.OriginalName) //The root nodes will most likely not have the same name
		{
			//Console.WriteLine($"\tInequal because original name {left.OriginalName} doesn't match {right.OriginalName}");
			return false;
		}
		if (!root && left.Name != right.Name) //The root nodes will most likely not have the same name
		{
			//Console.WriteLine($"\tInequal because name {left.Name} doesn't match {right.Name}");
			return false;
		}
		if (left.OriginalTypeName != right.OriginalTypeName)
		{
			//Console.WriteLine($"\tInequal because original type name {left.OriginalTypeName} doesn't match {right.OriginalTypeName}");
			return false;
		}
		if (left.TypeName != right.TypeName)
		{
			//Console.WriteLine($"\tInequal because type name {left.TypeName} doesn't match {right.TypeName}");
			return false;
		}
		if (left.Version != right.Version)
		{
			//Console.WriteLine($"\tInequal because version {left.Version} doesn't match {right.Version}");
			return false;
		}
		if (!root && (left.MetaFlag & MetaMask) != (right.MetaFlag & MetaMask))
		//if (!root && left.MetaFlag != right.MetaFlag)
		{
			//Console.WriteLine($"\tInequal because meta flag {left.MetaFlag} doesn't match {right.MetaFlag}");
			return false;
		}
		if (left.SubNodes!.Count != right.SubNodes!.Count)
		{
			//Console.WriteLine($"\tInequal because subnode count {left.SubNodes.Count} doesn't match {right.SubNodes.Count}");
			return false;
		}
		for (int i = 0; i < left.SubNodes.Count; i++)
		{
			if (!Equals(left.SubNodes[i], right.SubNodes[i], false))
			{
				return false;
			}
		}
		return true;
	}
}
