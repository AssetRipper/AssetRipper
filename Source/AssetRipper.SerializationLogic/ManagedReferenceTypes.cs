namespace AssetRipper.SerializationLogic;

/// <summary>
/// The types that Unity uses to serialize fields with the [SerializeReference] attribute.
/// </summary>
/// <remarks>
/// These mirror the nodes that Unity emits into type trees, which are documented in Notes.md.
/// Type trees are normalized onto these types, so that a structure read from a type tree has the same layout as one read from an assembly.
/// </remarks>
public static class ManagedReferenceTypes
{
	/// <summary>
	/// The type name of a field with the [SerializeReference] attribute.
	/// </summary>
	public const string ManagedReferenceName = "managedReference";

	/// <summary>
	/// The type name of an element of an array or list with the [SerializeReference] attribute.
	/// </summary>
	public const string ManagedReferenceArrayItemName = "managedRefArrayItem";

	public const string RegistryName = "ManagedReferencesRegistry";
	public const string ReferencedObjectName = "ReferencedObject";
	public const string ReferencedManagedTypeName = "ReferencedManagedType";
	public const string ReferencedObjectDataName = "ReferencedObjectData";

	public const string RegistryFieldName = "references";
	public const string VersionFieldName = "version";
	public const string ReferenceIdsFieldName = "RefIds";
	public const string ReferenceIdFieldName = "rid";
	public const string IndexFieldName = "id";
	public const string TypeFieldName = "type";
	public const string DataFieldName = "data";
	public const string ClassFieldName = "class";
	public const string NamespaceFieldName = "ns";
	public const string AssemblyFieldName = "asm";

	/// <summary>
	/// The identifier of an object in the <see cref="Registry"/>. It is stored in place of the object itself.
	/// </summary>
	public static SerializableType ManagedReference { get; } = new CustomSerializableType(ManagedReferenceName, false,
	[
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long), 0, ReferenceIdFieldName, false),
	]);

	/// <summary>
	/// The index of an object in the <see cref="Registry"/>, used before referenced objects got stable identifiers.
	/// </summary>
	/// <remarks>
	/// This is a 32 bit integer named "id", rather than the 64 bit integer named "rid" of <see cref="ManagedReference"/>.
	/// </remarks>
	public static SerializableType IndexedManagedReference { get; } = new CustomSerializableType(ManagedReferenceName, false,
	[
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Int), 0, IndexFieldName, false),
	]);

	/// <summary>
	/// The assembly qualified name of a <see cref="ReferencedObject"/>'s type.
	/// </summary>
	public static SerializableType ReferencedManagedType { get; } = new CustomSerializableType(ReferencedManagedTypeName, true,
	[
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, ClassFieldName, false),
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, NamespaceFieldName, false),
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, AssemblyFieldName, false),
	]);

	/// <summary>
	/// The content of a <see cref="ReferencedObject"/>.
	/// </summary>
	/// <remarks>
	/// This type has no fields because its layout is only known once the <see cref="ReferencedManagedType"/> preceding it has been read.
	/// </remarks>
	public static SerializableType ReferencedObjectData { get; } = new CustomSerializableType(ReferencedObjectDataName, false, []);

	/// <summary>
	/// An object stored in the <see cref="Registry"/>.
	/// </summary>
	public static SerializableType ReferencedObject { get; } = new CustomSerializableType(ReferencedObjectName, false,
	[
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long), 0, ReferenceIdFieldName, false),
		new SerializableType.Field(ReferencedManagedType, 0, TypeFieldName, false),
		new SerializableType.Field(ReferencedObjectData, 0, DataFieldName, false),
	]);

	/// <summary>
	/// The objects referenced by all the [SerializeReference] fields of an asset.
	/// </summary>
	public static SerializableType Registry { get; } = new CustomSerializableType(RegistryName, false,
	[
		new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.Int), 0, VersionFieldName, false),
		new SerializableType.Field(ReferencedObject, 1, ReferenceIdsFieldName, true),
	]);

	/// <summary>
	/// The field holding the <see cref="Registry"/>. It is always the last field of an asset's root type.
	/// </summary>
	public static SerializableType.Field RegistryField { get; } = new(Registry, 0, RegistryFieldName, false);
}
