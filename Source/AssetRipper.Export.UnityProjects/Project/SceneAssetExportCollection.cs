using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class SceneAssetExportCollection : IExportCollection
{
	public ISceneAsset Asset { get; }
	public SceneDefinition TargetScene => Asset.TargetScene ?? throw new NullReferenceException();

	public SceneAssetExportCollection(ISceneAsset asset)
	{
		Asset = asset;
	}

	public AssetCollection File => Asset.Collection;
	public TransferInstructionFlags Flags => File.Flags;
	public IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			yield return Asset;
		}
	}

	public string Name => $"{TargetScene.Name} (SceneAsset)";

	public MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		return new MetaPtr(GetExportID(), TargetScene.GUID, AssetType.Meta);
	}

	private static long GetExportID()
	{
		return ExportIdHandler.GetMainExportID((int)ClassIDType.DefaultAsset);
	}

	public bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		return true;
	}

	public long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		return GetExportID();
	}

	public bool Contains(IUnityObjectBase asset)
	{
		return asset == Asset;
	}
}
