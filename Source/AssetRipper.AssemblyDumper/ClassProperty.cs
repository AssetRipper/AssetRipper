namespace AssetRipper.AssemblyDumper;

internal class ClassProperty : PropertyBase
{
	public ClassProperty(PropertyDefinition definition, FieldDefinition? backingField, InterfaceProperty @base, GeneratedClassInstance @class) : base(definition)
	{
		BackingField = backingField;
		if (!IsInjected && backingField?.Name is not null)
		{
			ReleaseNode = @class.Class.ReleaseRootNode?.TryGetSubNodeByName(backingField.Name);
			EditorNode = @class.Class.EditorRootNode?.TryGetSubNodeByName(backingField.Name);
			if (ReleaseNode is null && EditorNode is null)
			{
				throw new Exception($"Failed to find node: {@class.Name}.{backingField.Name} on {@class.VersionRange}");
			}
		}
		Class = @class;
		Base = @base;
		Base.AddImplementation(this);
	}

	public UniversalNode? ReleaseNode { get; }

	public UniversalNode? EditorNode { get; }

	public string? OriginalFieldName => ReleaseNode?.OriginalName ?? EditorNode?.OriginalName;

	public FieldDefinition? BackingField { get; }

	public InterfaceProperty Base { get; }

	public GeneratedClassInstance Class { get; }

	[MemberNotNullWhen(false, nameof(BackingField))]
	public bool IsAbsent => BackingField is null;

	[MemberNotNullWhen(true, nameof(BackingField))]
	public bool IsPresent => !IsAbsent;

	[MemberNotNullWhen(true, nameof(ReleaseNode))]
	[MemberNotNullWhen(false, nameof(EditorNode))]
	public bool IsReleaseOnly => ReleaseNode is not null && EditorNode is null;

	[MemberNotNullWhen(false, nameof(ReleaseNode))]
	[MemberNotNullWhen(true, nameof(EditorNode))]
	public bool IsEditorOnly => ReleaseNode is null && EditorNode is not null;

	[MemberNotNullWhen(true, nameof(BackingField))]
	public bool HasBackingFieldInDeclaringType
	{
		get
		{
			return BackingField is not null && BackingField.DeclaringType == Class.Type;
		}
	}
}
