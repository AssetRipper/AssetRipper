using AssetRipper.AssemblyDumper.Passes;

namespace AssetRipper.AssemblyDumper.Groups;

internal sealed class SubclassGroup : ClassGroupBase
{
	public override string Name { get; }

	public override int ID => -1;

	public override bool IsSealed => true;

	public override string Namespace => SharedState.GetSubclassNamespace(Name);

	public override bool UniformlyNamed => true;

	public override bool IsPPtr => Name.StartsWith("PPtr_", StringComparison.Ordinal);

	public override bool IsString => Name == Pass002_RenameSubnodes.Utf8StringName;

	public SubclassGroup(string name, TypeDefinition @interface) : base(@interface)
	{
		Name = name;
	}
}
