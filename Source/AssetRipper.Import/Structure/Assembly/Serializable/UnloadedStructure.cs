using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Endian;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// This is a placeholder asset that lazily reads the actual structure sometime after all the assets have been loaded.
/// This allows MonoBehaviours to be loaded before their referenced MonoScript.
/// </summary>
public sealed class UnloadedStructure : UnityAssetBase, IDeepCloneable
{
	private sealed class StatelessAsset : UnityAssetBase, IDeepCloneable
	{
		public static StatelessAsset Instance { get; } = new();
		private StatelessAsset()
		{
		}

		public override void Reset()
		{
		}

		public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer)
		{
			return other is StatelessAsset;
		}

		IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => this;
	}

	/// <summary>
	/// The <see cref="IMonoBehaviour"/> that <see langword="this"/> is the <see cref="IMonoBehaviour.Structure"/> for.
	/// </summary>
	public IMonoBehaviour MonoBehaviour { get; }

	public IAssemblyManager AssemblyManager { get; }

	/// <summary>
	/// The segment of data for this structure.
	/// </summary>
	public ReadOnlyArraySegment<byte> StructureData { get; }

	public UnloadedStructure(IMonoBehaviour monoBehaviour, IAssemblyManager assemblyManager, ReadOnlyArraySegment<byte> structureData)
	{
		MonoBehaviour = monoBehaviour;
		AssemblyManager = assemblyManager;
		StructureData = structureData;
	}

	private void ThrowIfNotStructure()
	{
		if (!ReferenceEquals(MonoBehaviour.Structure, this))
		{
			throw new InvalidOperationException("The MonoBehaviour structure has already been loaded.");
		}
	}

	public SerializableStructure? LoadStructure()
	{
		ThrowIfNotStructure();
		string? failureReason = null;
		SerializableStructure? structure = MonoBehaviour.ScriptP?.GetBehaviourType(AssemblyManager, out failureReason)?.CreateSerializableStructure();
		if (structure is not null)
		{
			EndianSpanReader reader = new EndianSpanReader(StructureData, MonoBehaviour.Collection.EndianType);
			if (structure.TryRead(ref reader, MonoBehaviour))
			{
				MonoBehaviour.Structure = structure;
				return structure;
			}
		}
		else if (failureReason is not null)
		{
			Logger.Warning(LogCategory.Import, $"Could not read MonoBehaviour structure for `{MonoBehaviour.ScriptP?.GetFullName()}`. Reason: {failureReason}");
		}

		MonoBehaviour.Structure = null;
		return null;
	}

	private UnityAssetBase LoadStructureOrStatelessAsset()
	{
		return (UnityAssetBase?)LoadStructure() ?? StatelessAsset.Instance;
	}

	public IUnityAssetBase DeepClone(PPtrConverter converter)
	{
		return LoadStructure()?.DeepClone(converter) ?? (IUnityAssetBase)StatelessAsset.Instance;
	}

	#region UnityAssetBase Overrides
	public override bool FlowMappedInYaml => LoadStructure()?.FlowMappedInYaml ?? base.FlowMappedInYaml;

	public override int SerializedVersion => LoadStructure()?.SerializedVersion ?? base.SerializedVersion;

	public override void WalkEditor(AssetWalker walker)
	{
		LoadStructureOrStatelessAsset().WalkEditor(walker);
	}

	public override void WalkRelease(AssetWalker walker)
	{
		LoadStructureOrStatelessAsset().WalkRelease(walker);
	}

	public override void WalkStandard(AssetWalker walker)
	{
		LoadStructureOrStatelessAsset().WalkStandard(walker);
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return LoadStructure()?.FetchDependencies() ?? [];
	}

	public override void WriteEditor(AssetWriter writer) => LoadStructure()?.WriteEditor(writer);

	public override void WriteRelease(AssetWriter writer) => LoadStructure()?.WriteRelease(writer);

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter) => LoadStructure()?.CopyValues(source, converter);

	public override void Reset() => LoadStructure()?.Reset();

	public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer)
	{
		return LoadStructureOrStatelessAsset().AddToEqualityComparer(other, comparer);
	}
	#endregion
}
