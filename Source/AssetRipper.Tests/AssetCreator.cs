using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated;
using System.Reflection;

namespace AssetRipper.Tests;

internal static class AssetCreator
{
	public static T Create<T>(ClassIDType classID, UnityVersion version, Func<AssetInfo, T> factory) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset<T>((int)classID, factory);
	}

	public static T Create<T>(ClassIDType classID, UnityVersion version) where T : IUnityObjectBase
	{
		return MakeCollection(version).CreateAsset((int)classID, (assetInfo) =>
		{
			return (T)AssetFactory.Create(assetInfo);
		});
	}

	/// <summary>
	/// Create a new asset using reflection.
	/// </summary>
	/// <remarks>
	/// The type must have a constructor which takes an <see cref="AssetInfo"/> as its only parameter.
	/// </remarks>
	/// <typeparam name="T">The type of asset to create.</typeparam>
	/// <returns>A new asset.</returns>
	public static T CreateUnsafe<T>() where T : UnityObjectBase
	{
		return Create(default, default, (assetInfo) =>
		{
			return (T?)Activator.CreateInstance(typeof(T), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [assetInfo], null)
				?? throw new NullReferenceException();
		});
	}

	/// <summary>
	/// Create a new asset using reflection.
	/// </summary>
	/// <remarks>
	/// The type must have a constructor which takes an <see cref="AssetInfo"/> as its only parameter.
	/// </remarks>
	/// <param name="type">The type of asset to create.</param>
	/// <returns>A new asset.</returns>
	public static UnityObjectBase CreateUnsafe(Type type)
	{
		return (UnityObjectBase?)typeof(AssetCreator).GetMethod(nameof(CreateUnsafe), 1, BindingFlags.Public | BindingFlags.Static, null, [], null)
			!.MakeGenericMethod(type)
			.Invoke(null, null)
			?? throw new NullReferenceException();
	}

	private static ProcessedAssetCollection MakeCollection(UnityVersion version)
	{
		GameBundle gameBundle = new();
		ProcessedAssetCollection collection = gameBundle.AddNewProcessedCollection(nameof(AssetCreator), version);
		return collection;
	}
}
