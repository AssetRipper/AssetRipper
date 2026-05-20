namespace AssetRipper.SerializationLogic;

internal static class ManagedReferenceTypes
{
	private static SerializableType? s_managedReferenceType;
	public static SerializableType GetManagedReferenceType()
	{
		if (s_managedReferenceType == null)
		{
			SerializableType longType = SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long);
			s_managedReferenceType = new CustomSerializableType(null, PrimitiveType.Complex, "managedReference", [new SerializableType.Field(longType, 0, "rid", true)]);
		}
		return s_managedReferenceType;
	}

	private static SerializableType? s_managedReferencesRegistryType;
	public static SerializableType GetManagedReferencesRegistryType()
	{
		if (s_managedReferencesRegistryType == null)
		{
			SerializableType stringType = SerializablePrimitiveType.GetOrCreate(PrimitiveType.String);
			SerializableType intType = SerializablePrimitiveType.GetOrCreate(PrimitiveType.Int);
			SerializableType longType = SerializablePrimitiveType.GetOrCreate(PrimitiveType.Long);

			SerializableType typeType = new CustomSerializableType(null, PrimitiveType.Complex, "ReferencedManagedType", [
				new SerializableType.Field(stringType, 0, "class", true),
				new SerializableType.Field(stringType, 0, "ns", true),
				new SerializableType.Field(stringType, 0, "asm", true)
			]);

			SerializableType dataType = new CustomSerializableType(null, PrimitiveType.Complex, "ReferencedObjectData", []);

			SerializableType referencedObjectType = new CustomSerializableType(null, PrimitiveType.Complex, "ReferencedObject", [
				new SerializableType.Field(longType, 0, "rid", true),
				new SerializableType.Field(typeType, 0, "type", true),
				new SerializableType.Field(dataType, 0, "data", true)
			]);

			s_managedReferencesRegistryType = new CustomSerializableType(null, PrimitiveType.Complex, "ManagedReferencesRegistry", [
				new SerializableType.Field(intType, 0, "version", true),
				new SerializableType.Field(referencedObjectType, 1, "RefIds", true)
			]);
		}
		return s_managedReferencesRegistryType;
	}
}
