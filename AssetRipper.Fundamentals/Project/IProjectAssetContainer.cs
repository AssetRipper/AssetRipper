using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.Project
{
	public interface IProjectAssetContainer : IExportContainer
	{
		ISerializedFile File { get; }
		VirtualSerializedFile VirtualFile { get; }
		UnityGUID SceneNameToGUID(string name);
		bool TryGetAssetPathFromAssets(IEnumerable<IUnityObjectBase> assets, out IUnityObjectBase selectedAsset, out string assetPath);
	}
}
