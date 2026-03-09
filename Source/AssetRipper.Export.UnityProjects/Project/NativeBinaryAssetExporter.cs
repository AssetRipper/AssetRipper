using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_238;

namespace AssetRipper.Export.UnityProjects.Project;

internal sealed class NativeBinaryAssetExporter : BinaryAssetExporter
{
	private readonly DefaultYamlExporter yamlFallback = new();

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (IsSupportedAsset(asset) && SupportsNativeBinaryExport(asset.Collection.Version))
		{
			exportCollection = new AssetExportCollection<IUnityObjectBase>(this, asset);
			return true;
		}

		exportCollection = null;
		return false;
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		if (!IsSupportedAsset(asset) || !SupportsNativeBinaryExport(container.ExportVersion))
		{
			return yamlFallback.Export(container, asset, path, fileSystem);
		}

		try
		{
			SerializedFile file = NativeBinaryExportBuilder.Build(container, asset);
			using Stream stream = fileSystem.File.Create(path);
			file.Write(stream);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Warning(LogCategory.Export, $"Falling back to YAML for native asset '{asset.GetBestName()}' ({asset.ClassName}) because {ex.GetType().Name} was thrown: {ex.Message}");
			return yamlFallback.Export(container, asset, path, fileSystem);
		}
	}

	private static bool IsSupportedAsset(IUnityObjectBase asset) => asset is ILightingDataAsset or INavMeshData;

	// Keep the binary writer on modern serialized-file generations until older versions are verified.
	private static bool SupportsNativeBinaryExport(UnityVersion version) => version.GreaterThanOrEquals(2020);

	private static class NativeBinaryExportBuilder
	{
		public static SerializedFile Build(IExportContainer container, IUnityObjectBase root)
		{
			ProcessedBundle bundle = new("BinaryNativeExport");
			BinaryProxyCollection mainCollection = new("Main", bundle, container.ExportVersion, root.Collection.EndianType, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

			Dictionary<IUnityObjectBase, IUnityObjectBase> proxyMap = new();
			Dictionary<ExternalDependencyKey, BinaryProxyCollection> externalCollections = new();
			Queue<IUnityObjectBase> pendingLocalAssets = new();

			IUnityObjectBase mainClone = mainCollection.CreateProxyAsset(container.GetExportID(root), root.ClassID);
			proxyMap[root] = mainClone;
			pendingLocalAssets.Enqueue(root);

			while (pendingLocalAssets.Count > 0)
			{
				IUnityObjectBase current = pendingLocalAssets.Dequeue();
				foreach (IUnityObjectBase dependency in EnumerateDependencies(current))
				{
					if (proxyMap.ContainsKey(dependency))
					{
						continue;
					}

					MetaPtr pointer = container.CreateExportPointer(dependency);
					if (pointer.GUID == UnityGuid.MissingReference || pointer.FileID == 0)
					{
						throw new InvalidOperationException($"Dependency '{dependency.GetBestName()}' does not have a stable export pointer.");
					}

					if (pointer.GUID.IsZero)
					{
						IUnityObjectBase localClone = mainCollection.CreateProxyAsset(pointer.FileID, dependency.ClassID);
						proxyMap[dependency] = localClone;
						pendingLocalAssets.Enqueue(dependency);
					}
					else
					{
						ExternalDependencyKey key = new(pointer.GUID, pointer.AssetType);
						if (!externalCollections.TryGetValue(key, out BinaryProxyCollection? dependencyCollection))
						{
							dependencyCollection = new(pointer.GUID.ToString(), bundle, container.ExportVersion, root.Collection.EndianType, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags, pointer);
							externalCollections.Add(key, dependencyCollection);
						}

						IUnityObjectBase proxyAsset = dependencyCollection.CreateProxyAsset(pointer.FileID, dependency.ClassID);
						proxyMap[dependency] = proxyAsset;
					}
				}
			}

			DictionaryAssetResolver resolver = new(proxyMap);
			foreach ((IUnityObjectBase sourceAsset, IUnityObjectBase targetAsset) in proxyMap.Where(static pair => pair.Value.Collection is BinaryProxyCollection { ExternalPointer: null }))
			{
				targetAsset.CopyValues(sourceAsset, new PPtrConverter(sourceAsset.Collection, targetAsset.Collection, resolver));
			}

			return BuildSerializedFile(container, mainCollection);
		}

		private static SerializedFile BuildSerializedFile(IExportContainer container, BinaryProxyCollection mainCollection)
		{
			SerializedFileBuilder builder = new()
			{
				Generation = GetFormatVersion(container.ExportVersion),
				Version = container.ExportVersion,
				Platform = BuildTarget.NoTarget,
				EndianType = mainCollection.EndianType,
				HasTypeTree = false,
			};

			Dictionary<int, int> typeIndexByClassId = new();
			foreach (IUnityObjectBase asset in mainCollection.Assets.Values.OrderBy(static asset => asset.PathID))
			{
				int typeIndex = GetOrAddTypeIndex(asset.ClassID, builder.Types, typeIndexByClassId);
				byte[] objectData = WriteObjectData(asset, mainCollection);
				SerializedType type = builder.Types[typeIndex];
				builder.Objects.Add(new ObjectInfo(type)
				{
					FileID = asset.PathID,
					SerializedTypeIndex = typeIndex,
					ObjectData = objectData,
				});
			}

			foreach (AssetCollection dependency in mainCollection.Dependencies.Skip(1))
			{
				if (dependency is not BinaryProxyCollection dependencyCollection || dependencyCollection.ExternalPointer is not MetaPtr pointer)
				{
					throw new InvalidOperationException($"Unexpected dependency collection '{dependency.Name ?? "<unnamed>"}'.");
				}

				builder.Dependencies.Add(new FileIdentifier
				{
					AssetPath = Utf8String.Empty,
					Type = pointer.AssetType,
					PathNameOrigin = pointer.GUID.ToString(),
					PathName = pointer.GUID.ToString(),
					Guid = pointer.GUID,
				});
			}

			return builder.Build();
		}

		private static byte[] WriteObjectData(IUnityObjectBase asset, AssetCollection collection)
		{
			using MemoryStream stream = new();
			using AssetWriter writer = new(stream, collection);
			asset.Write(writer, TransferInstructionFlags.NoTransferInstructionFlags);
			return stream.ToArray();
		}

		private static IEnumerable<IUnityObjectBase> EnumerateDependencies(IUnityObjectBase asset)
		{
			HashSet<IUnityObjectBase> uniqueDependencies = new();
			foreach ((_, PPtr dependencyPointer) in asset.FetchDependencies())
			{
				if (asset.Collection.TryGetAsset(dependencyPointer, out IUnityObjectBase? dependencyAsset) && dependencyAsset is not null && uniqueDependencies.Add(dependencyAsset))
				{
					yield return dependencyAsset;
				}
			}
		}

		private static int GetOrAddTypeIndex(int classId, List<SerializedType> types, Dictionary<int, int> typeIndexByClassId)
		{
			if (typeIndexByClassId.TryGetValue(classId, out int existingIndex))
			{
				return existingIndex;
			}

			SerializedType type = new()
			{
				TypeID = classId,
				ScriptTypeIndex = -1,
				IsStrippedType = false,
			};
			int index = types.Count;
			types.Add(type);
			typeIndexByClassId.Add(classId, index);
			return index;
		}

		private static FormatVersion GetFormatVersion(UnityVersion version)
		{
			return true switch
			{
				_ when version.GreaterThanOrEquals(2020, 1) => FormatVersion.LargeFilesSupport,
				_ when version.GreaterThanOrEquals(2019, 3) => FormatVersion.StoresTypeDependencies,
				_ when version.GreaterThanOrEquals(2019, 2) => FormatVersion.SupportsRefObject,
				_ when version.GreaterThanOrEquals(2019, 1) => FormatVersion.TypeTreeNodeWithTypeFlags,
				_ when version.GreaterThanOrEquals(5, 5) => FormatVersion.RefactorTypeData,
				_ when version.GreaterThanOrEquals(5, 0, 1) => FormatVersion.SupportsStrippedObject,
				_ when version.GreaterThanOrEquals(5, 0) => FormatVersion.Unknown_14,
				_ when version.GreaterThanOrEquals(3, 5) => FormatVersion.Unknown_9,
				_ when version.GreaterThanOrEquals(3, 0) => FormatVersion.Unknown_8,
				_ when version.GreaterThanOrEquals(2, 1) => FormatVersion.Unknown_6,
				_ => FormatVersion.Unknown_5,
			};
		}

		private readonly record struct ExternalDependencyKey(UnityGuid Guid, AssetType AssetType);
	}

	private sealed class DictionaryAssetResolver(Dictionary<IUnityObjectBase, IUnityObjectBase> cache) : IAssetResolver
	{
		public T? Resolve<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
		{
			if (asset is null)
			{
				return default;
			}

			if (cache.TryGetValue(asset, out IUnityObjectBase? resolvedAsset))
			{
				return TryCast<T>(resolvedAsset);
			}

			return TryCast<T>(asset);
		}

		private static T? TryCast<T>(IUnityObjectBase asset) where T : IUnityObjectBase
		{
			return asset is T t ? t : default;
		}
	}

	private sealed class BinaryProxyCollection : VirtualAssetCollection
	{
		public BinaryProxyCollection(string name, Bundle bundle, UnityVersion version, EndianType endianType, BuildTarget platform, TransferInstructionFlags flags, MetaPtr? externalPointer = null) : base(bundle)
		{
			Name = name;
			Version = version;
			OriginalVersion = version;
			Platform = platform;
			Flags = flags;
			EndianType = endianType;
			ExternalPointer = externalPointer;
		}

		public new string Name
		{
			get => base.Name;
			set => base.Name = value;
		}

		public MetaPtr? ExternalPointer { get; }

		public IUnityObjectBase CreateProxyAsset(long pathId, int classId)
		{
			if (TryGetAsset(pathId, out IUnityObjectBase? existingAsset))
			{
				return existingAsset;
			}

			IUnityObjectBase asset = AssetFactory.Create(new AssetInfo(this, pathId, classId));
			AddAsset(asset);
			return asset;
		}
	}
}
