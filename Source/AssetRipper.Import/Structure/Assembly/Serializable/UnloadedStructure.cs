using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Endian;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.Yaml;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// This is a placeholder asset that lazily reads the actual structure sometime after all the assets have been loaded.
/// This allows MonoBehaviours to be loaded before their referenced MonoScript.
/// </summary>
internal sealed class UnloadedStructure : UnityAssetBase
{
	/// <summary>
	/// The <see cref="IMonoBehaviour"/> that <see langword="this"/> is the <see cref="IMonoBehaviour.Structure"/> for.
	/// </summary>
	private readonly IMonoBehaviour monoBehaviour;

	private readonly IAssemblyManager assemblyManager;

	/// <summary>
	/// The segment of data for this structure.
	/// </summary>
	private readonly ReadOnlyArraySegment<byte> structureData;

	public UnloadedStructure(IMonoBehaviour monoBehaviour, IAssemblyManager assemblyManager, ReadOnlyArraySegment<byte> structureData)
	{
		this.monoBehaviour = monoBehaviour;
		this.assemblyManager = assemblyManager;
		this.structureData = structureData;
	}

	private void ThrowIfNotStructure()
	{
		if (!ReferenceEquals(monoBehaviour.Structure, this))
		{
			throw new InvalidOperationException("The MonoBehaviour structure has already been loaded.");
		}
	}

	private SerializableStructure? LoadStructure()
	{
		ThrowIfNotStructure();
		SerializableStructure? structure = monoBehaviour.Script_C114P?.GetBehaviourType(assemblyManager)?.CreateSerializableStructure();
		if (structure is not null)
		{
			EndianSpanReader reader = new EndianSpanReader(structureData, monoBehaviour.Collection.EndianType);
			if (structure.TryRead(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags))
			{
				monoBehaviour.Structure = structure;
				return structure;
			}
		}

		monoBehaviour.Structure = null;
		return null;
	}

	#region UnityAssetBase Overrides
	public override YamlMappingNode ExportYamlEditor(IExportContainer container)
	{
		SerializableStructure? structure = LoadStructure();
		return structure?.ExportYamlEditor(container) ?? new();
	}

	public override YamlMappingNode ExportYamlRelease(IExportContainer container)
	{
		SerializableStructure? structure = LoadStructure();
		return structure?.ExportYamlRelease(container) ?? new();
	}

	public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
	{
		IUnityAssetBase? structure = LoadStructure();
		return structure?.FetchDependencies(context) ?? Enumerable.Empty<PPtr<IUnityObjectBase>>();
	}

	public override IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies(FieldName? parent)
	{
		IUnityAssetBase? structure = LoadStructure();
		return structure?.FetchDependencies(parent) ?? Enumerable.Empty<(FieldName, PPtr<IUnityObjectBase>)>();
	}

	public override void WriteEditor(AssetWriter writer)
	{
		IUnityAssetBase? structure = LoadStructure();
		structure?.WriteEditor(writer);
	}

	public override void WriteRelease(AssetWriter writer)
	{
		IUnityAssetBase? structure = LoadStructure();
		structure?.WriteRelease(writer);
	}
	#endregion
}
