using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Core.Project
{
	public readonly record struct ProjectAssetPath(string Root, string AssetPath)
	{
		public string SubstituteExportPath(IUnityObjectBase asset)
		{
			string projectPath = SubstitutePath(asset.GetOriginalName());
			projectPath = DirectoryUtils.FixInvalidPathCharacters(projectPath);
			return Path.Combine(Root, projectPath);
		}

		private string SubstitutePath(string assetName)
		{
			if (assetName.Length > 0 && assetName != AssetPath && AssetPath.EndsWith(assetName, StringComparison.OrdinalIgnoreCase))
			{
				if (assetName.Length == AssetPath.Length)
				{
					return assetName;
				}
				if (AssetPath[AssetPath.Length - assetName.Length - 1] == ObjectUtils.DirectorySeparatorChar)
				{
					string directoryPath = AssetPath.Substring(0, AssetPath.Length - assetName.Length);
					return directoryPath + assetName;
				}
			}
			return AssetPath;
		}
	}
}
