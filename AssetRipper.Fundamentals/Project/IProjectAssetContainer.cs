using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core.Project
{
	public interface IProjectAssetContainer : IExportContainer
	{
		ISerializedFile File { get; }
		VirtualSerializedFile VirtualFile { get; }
		UnityGUID SceneNameToGUID(string name);
		bool TryGetAssetPathFromAssets(IEnumerable<IUnityObjectBase> assets, [NotNullWhen(true)] out IUnityObjectBase? selectedAsset, out string assetPath);
	}
}
