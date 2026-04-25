using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SerializationLogic;
using System.Text;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

internal sealed class ManagedReferencesRegistryAsset : UnityAssetBase, IDeepCloneable
{
	private readonly ManagedReferenceResolver? resolver;

	public ManagedReferencesRegistryAsset(ManagedReferenceResolver? resolver)
	{
		this.resolver = resolver;
	}

	public int Version { get; private set; }
	public bool UsedLegacyShortTerminusTail { get; private set; }
	public List<ManagedReferenceEntry> References { get; } = [];

	public void InitializeFallback(int version)
	{
		Version = version;
		UsedLegacyShortTerminusTail = false;
		References.Clear();
	}

	public void EnsureNullReference(long rid)
	{
		if (!References.Any(entry => entry.Rid == rid))
		{
			References.Add(new ManagedReferenceEntry
			{
				Rid = rid,
				Type = new ManagedReferenceTypeDescriptor("", "", ""),
			});
		}
	}

	public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
	{
		if (resolver is null)
		{
			throw new InvalidOperationException("Managed reference resolver is required.");
		}

		Version = reader.ReadInt32();
		UsedLegacyShortTerminusTail = false;
		References.Clear();
		Trace($"registry version={Version} position={reader.Position}");

		if (TryReadLegacyShortTerminusTail(ref reader))
		{
			return;
		}

		if (Version == 1)
		{
			long rid = 0;
			while (true)
			{
				ManagedReferenceEntry entry = ReadEntry(ref reader, version, flags, rid, hasRidPrefix: false);
				if (entry.Type.IsTerminus)
				{
					break;
				}
				References.Add(entry);
				rid++;
			}
		}
		else
		{
			int count = reader.ReadInt32();
			Trace($"registry count={count} position={reader.Position}");
			for (int i = 0; i < count; i++)
			{
				References.Add(ReadEntry(ref reader, version, flags, 0, hasRidPrefix: true));
			}
		}
	}

	private bool TryReadLegacyShortTerminusTail(ref EndianSpanReader reader)
	{
		if (Version != ManagedReferenceResolver.TerminusKey.Namespace.Length)
		{
			return false;
		}

		int startPosition = reader.Position;
		try
		{
			string namespaceName = ReadRawUtf8String(ref reader, Version);
			reader.Align();
			string assemblyName = reader.ReadUtf8StringAligned().String;
			if (namespaceName == ManagedReferenceResolver.TerminusKey.Namespace
				&& assemblyName == ManagedReferenceResolver.TerminusKey.AssemblyName
				&& reader.Position == reader.Length)
			{
				Version = 1;
				UsedLegacyShortTerminusTail = true;
				Trace("registry legacy short terminus tail detected");
				return true;
			}
		}
		catch
		{
		}

		reader.Position = startPosition;
		return false;
	}

	private static string ReadRawUtf8String(ref EndianSpanReader reader, int length)
	{
		byte[] bytes = new byte[length];
		for (int i = 0; i < length; i++)
		{
			bytes[i] = reader.ReadByte();
		}
		return Encoding.UTF8.GetString(bytes);
	}

	private ManagedReferenceEntry ReadEntry(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, long rid, bool hasRidPrefix)
	{
		ManagedReferenceEntry entry = new();
		entry.Rid = hasRidPrefix ? reader.ReadInt64() : rid;
		entry.Type = ReadDescriptor(ref reader);
		Trace($"entry rid={entry.Rid} type={entry.Type.AssemblyName}|{entry.Type.Namespace}|{entry.Type.ClassName} position={reader.Position}");
		if (entry.Type.Key.IsEmpty || entry.Type.IsTerminus)
		{
			return entry;
		}
		SerializableType? type = resolver!.Resolve(entry.Type);
		if (type is null)
		{
			throw new InvalidOperationException($"Unable to resolve managed reference type {entry.Type.Namespace}.{entry.Type.ClassName} from assembly {entry.Type.AssemblyName}.");
		}
		else
		{
			IUnityAssetBase data = type.CreateInstance(0, version, resolver);
			if (data is SerializableStructure structure)
			{
				structure.Read(ref reader, version, flags, resolver);
			}
			else
			{
				data.Read(ref reader, flags);
			}
			entry.Data = data;
		}
		return entry;
	}

	private static ManagedReferenceTypeDescriptor ReadDescriptor(ref EndianSpanReader reader)
	{
		string className = reader.ReadUtf8StringAligned();
		string namespaceName = reader.ReadUtf8StringAligned();
		string assemblyName = reader.ReadUtf8StringAligned();
		return new ManagedReferenceTypeDescriptor(assemblyName, namespaceName, className);
	}

	private static void Trace(string message)
	{
		if (Environment.GetEnvironmentVariable("AR_DEBUG_MANAGED_REFERENCE") is "1")
		{
			Logger.Info(LogCategory.Import, $"ManagedRefTrace {message}");
		}
	}

	public override void WalkEditor(AssetWalker walker)
	{
		if (!walker.EnterAsset(this))
		{
			return;
		}

		if (walker.EnterField(this, "version"))
		{
			walker.VisitPrimitive(Version);
			walker.ExitField(this, "version");
		}
		walker.DivideAsset(this);
		string refsFieldName = Version >= 2 ? "RefIds" : "00000000";
		if (walker.EnterField(this, refsFieldName))
		{
			if (walker.EnterList(References))
			{
				for (int i = 0; i < References.Count; i++)
				{
					WalkEntry(walker, References[i]);
					if (i + 1 < References.Count)
					{
						walker.DivideList(References);
					}
				}
				walker.ExitList(References);
			}
			walker.ExitField(this, refsFieldName);
		}
		walker.ExitAsset(this);
	}

	private static void WalkEntry(AssetWalker walker, ManagedReferenceEntry entry)
	{
		ManagedReferenceEntryAsset asset = new(entry);
		asset.WalkEditor(walker);
	}

	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		for (int i = 0; i < References.Count; i++)
		{
			if (References[i].Data is IUnityAssetBase data)
			{
				foreach ((string path, PPtr pptr) in data.FetchDependencies())
				{
					yield return ($"references[{i}].{path}", pptr);
				}
			}
		}
	}

	public override void WriteEditor(AssetWriter writer) => throw new NotSupportedException();
	public override void WriteRelease(AssetWriter writer) => throw new NotSupportedException();

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		References.Clear();
		if (source is not ManagedReferencesRegistryAsset other)
		{
			return;
		}
		Version = other.Version;
		foreach (ManagedReferenceEntry entry in other.References)
		{
			References.Add(new ManagedReferenceEntry
			{
				Rid = entry.Rid,
				Type = entry.Type,
				Data = entry.Data,
			});
		}
	}

	public override void Reset()
	{
		References.Clear();
		Version = 0;
		UsedLegacyShortTerminusTail = false;
	}

	public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer) => null;

	public IUnityAssetBase DeepClone(PPtrConverter converter)
	{
		ManagedReferencesRegistryAsset clone = new(resolver)
		{
			Version = Version,
			UsedLegacyShortTerminusTail = UsedLegacyShortTerminusTail,
		};
		clone.CopyValues(this, converter);
		return clone;
	}

	private sealed class ManagedReferenceEntryAsset(ManagedReferenceEntry entry) : UnityAssetBase
	{
		public override void WalkEditor(AssetWalker walker)
		{
			if (!walker.EnterAsset(this))
			{
				return;
			}
			if (walker.EnterField(this, "rid"))
			{
				walker.VisitPrimitive(entry.Rid);
				walker.ExitField(this, "rid");
			}
			walker.DivideAsset(this);
			if (walker.EnterField(this, "type"))
			{
				new ManagedReferenceTypeAsset(entry.Type).WalkEditor(walker);
				walker.ExitField(this, "type");
			}
			walker.DivideAsset(this);
			if (walker.EnterField(this, "data"))
			{
				(entry.Data ?? EmptyDataAsset.Instance).WalkEditor(walker);
				walker.ExitField(this, "data");
			}
			walker.ExitAsset(this);
		}

		public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
		public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);
		public override IEnumerable<(string, PPtr)> FetchDependencies() => [];
		public override void WriteEditor(AssetWriter writer) => throw new NotSupportedException();
		public override void WriteRelease(AssetWriter writer) => throw new NotSupportedException();
		public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter) => throw new NotSupportedException();
		public override void Reset() { }
		public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer) => null;
	}

	private sealed class ManagedReferenceTypeAsset(ManagedReferenceTypeDescriptor descriptor) : UnityAssetBase
	{
		public override void WalkEditor(AssetWalker walker)
		{
			if (!walker.EnterAsset(this))
			{
				return;
			}
			if (walker.EnterField(this, "class"))
			{
				walker.VisitPrimitive(descriptor.ClassName);
				walker.ExitField(this, "class");
			}
			walker.DivideAsset(this);
			if (walker.EnterField(this, "ns"))
			{
				walker.VisitPrimitive(descriptor.Namespace);
				walker.ExitField(this, "ns");
			}
			walker.DivideAsset(this);
			if (walker.EnterField(this, "asm"))
			{
				walker.VisitPrimitive(descriptor.AssemblyName);
				walker.ExitField(this, "asm");
			}
			walker.ExitAsset(this);
		}

		public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
		public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);
		public override IEnumerable<(string, PPtr)> FetchDependencies() => [];
		public override void WriteEditor(AssetWriter writer) => throw new NotSupportedException();
		public override void WriteRelease(AssetWriter writer) => throw new NotSupportedException();
		public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter) => throw new NotSupportedException();
		public override void Reset() { }
		public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer) => null;
	}

	private sealed class EmptyDataAsset : UnityAssetBase
	{
		public static EmptyDataAsset Instance { get; } = new();
		public override void WalkEditor(AssetWalker walker)
		{
			if (walker.EnterAsset(this))
			{
				walker.ExitAsset(this);
			}
		}
		public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
		public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);
		public override IEnumerable<(string, PPtr)> FetchDependencies() => [];
		public override void WriteEditor(AssetWriter writer) => throw new NotSupportedException();
		public override void WriteRelease(AssetWriter writer) => throw new NotSupportedException();
		public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter) => throw new NotSupportedException();
		public override void Reset() { }
		public override bool? AddToEqualityComparer(IUnityAssetBase other, AssetEqualityComparer comparer) => null;
	}
}
