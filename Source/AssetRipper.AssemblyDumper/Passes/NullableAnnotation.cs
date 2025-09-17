namespace AssetRipper.AssemblyDumper.Passes;

public enum NullableAnnotation : byte
{
	Oblivious,
	NotNull,
	MaybeNull,
}
