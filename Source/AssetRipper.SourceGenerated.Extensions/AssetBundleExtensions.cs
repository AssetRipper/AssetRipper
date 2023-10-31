﻿using AssetRipper.SourceGenerated.Classes.ClassID_142;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class AssetBundleExtensions
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(UnityVersion version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(this IAssetBundle bundle) => HasPathExtension(bundle.Collection.Version);

		public static string GetAssetBundleName(this IAssetBundle bundle)
		{
			return bundle.Has_AssetBundleName_R() ? bundle.AssetBundleName_R : bundle.Collection.Bundle.Name;
		}
	}
}
