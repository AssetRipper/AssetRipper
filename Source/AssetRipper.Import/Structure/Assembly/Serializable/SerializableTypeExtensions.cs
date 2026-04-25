using AssetRipper.Assets;
using AssetRipper.Import.AssetCreation;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Object;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

public static class SerializableTypeExtensions
{
	public static SerializableStructure CreateSerializableStructure(this SerializableType type)
	{
		return new SerializableStructure(type, 0);
	}

	public static IUnityAssetBase CreateInstance(this SerializableType type, int depth, UnityVersion version)
	{
		return CreateInstance(type, depth, version, null);
	}

	internal static IUnityAssetBase CreateInstance(this SerializableType type, int depth, UnityVersion version, ManagedReferenceResolver? managedReferenceResolver)
	{
		if (type.Name == "ManagedReferencesRegistry")
		{
			return new ManagedReferencesRegistryAsset(managedReferenceResolver);
		}
		if (type.IsEngineStruct())
		{
			return GameAssetFactory.CreateEngineAsset(type.Name, version);
		}
		if (type.IsEnginePointer())
		{
			return PPtr_Object.Create(version);
		}
		SerializableStructure structure = new(type, depth);
		structure.InitializeFields(version);
		return structure;
	}
}
