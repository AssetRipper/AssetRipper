using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using System.Reflection;

namespace AssetRipper.SourceGenerated.Extensions;

/// <summary>
/// A helper class for creating assets, generally for unit testing.
/// </summary>
public static partial class AssetCreator
{
	public static T Create<T>(ClassIDType classID, UnityVersion version, Func<AssetInfo, T> factory) where T : IUnityObjectBase
	{
		return CreateCollection(version).CreateAsset((int)classID, factory);
	}

	/// <summary>
	/// Create a new asset using reflection.
	/// </summary>
	/// <remarks>
	/// The type must have a constructor which takes an <see cref="AssetInfo"/> as its only parameter.
	/// </remarks>
	/// <typeparam name="T">The type of asset to create.</typeparam>
	/// <returns>A new asset.</returns>
	public static T CreateUnsafe<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>() where T : UnityObjectBase
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
	[RequiresDynamicCode("")]
	public static UnityObjectBase CreateUnsafe([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
	{
#pragma warning disable IL2111 // Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can't guarantee availability of the requirements of the method.
		return (UnityObjectBase?)typeof(AssetCreator).GetMethod(nameof(CreateUnsafe), 1, BindingFlags.Public | BindingFlags.Static, null, [], null)
			!.MakeGenericMethod(type)
			.Invoke(null, null) ?? throw new NullReferenceException();
#pragma warning restore IL2111 // Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can't guarantee availability of the requirements of the method.
	}

	public static ProcessedAssetCollection CreateCollection(UnityVersion version)
	{
		GameBundle gameBundle = new();
		ProcessedAssetCollection collection = gameBundle.AddNewProcessedCollection(nameof(AssetCreator), version);
		return collection;
	}
}
