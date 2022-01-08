using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public interface IAssetBundle : IUnityObjectBase
	{
		bool HasAssetBundleName { get; }
		string AssetBundleName { get; set; }
		NullableKeyValuePair<string, IAssetInfo>[] GetAssets();
	}

	public static class AssetBundleExtensions
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(UnityVersion version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(this IAssetBundle bundle) => HasPathExtension(bundle.File.Version);

		public static string GetAssetBundleName(this IAssetBundle bundle)
		{
			return bundle.HasAssetBundleName ? bundle.AssetBundleName : bundle.File.Name;
		}
	}
}