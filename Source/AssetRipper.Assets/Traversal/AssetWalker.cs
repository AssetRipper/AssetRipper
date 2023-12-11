﻿using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets.Traversal;

/// <summary>
/// Abstract base class for traversing objects that implement <see cref="IUnityAssetBase"/>.
/// </summary>
public abstract class AssetWalker
{
	/// <summary>
	/// Called when entering an asset node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the asset being entered.</typeparam>
	/// <param name="asset">The asset being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the asset node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterAsset<T>(T asset)
		where T : IUnityAssetBase
	{
		return true;
	}

	/// <summary>
	/// Called when exiting an asset node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the asset being exited.</typeparam>
	/// <param name="asset">The asset being exited.</param>
	public virtual void ExitAsset<T>(T asset)
		where T : IUnityAssetBase
	{
	}

	/// <summary>
	/// Called when entering a field node during traversal.
	/// </summary>
	/// <typeparam name="T">The declaring type of the field being entered.</typeparam>
	/// <param name="name">The name of the field being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the field node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterField<T>(string name)
	{
		return true;
	}

	/// <summary>
	/// Called when exiting a field node during traversal.
	/// </summary>
	/// <typeparam name="T">The declaring type of the field being exited.</typeparam>
	/// <param name="name">The name of the field being exited.</param>
	public virtual void ExitField<T>(string name)
	{
	}

	/// <summary>
	/// Called when entering a list node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the list elements.</typeparam>
	/// <param name="list">The list being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the list node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterList<T>(AssetList<T> list)
		where T : notnull, new()
	{
		return true;
	}

	/// <summary>
	/// Called when exiting a list node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the list elements.</typeparam>
	/// <param name="list">The list being exited.</param>
	public virtual void ExitList<T>(AssetList<T> list)
		where T : notnull, new()
	{
	}

	/// <summary>
	/// Called when entering a dictionary node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of dictionary keys.</typeparam>
	/// <typeparam name="TValue">The type of dictionary values.</typeparam>
	/// <param name="dictionary">The dictionary being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the dictionary node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
		return true;
	}

	/// <summary>
	/// Called when exiting a dictionary node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of dictionary keys.</typeparam>
	/// <typeparam name="TValue">The type of dictionary values.</typeparam>
	/// <param name="dictionary">The dictionary being exited.</param>
	public virtual void ExitDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
	}

	/// <summary>
	/// Called when entering a key-value pair node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The key-value pair being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the key-value pair node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
		return true;
	}

	/// <summary>
	/// Called when exiting a key-value pair node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The key-value pair being exited.</param>
	public virtual void ExitPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
	}

	/// <summary>
	/// Visit a primitive leaf node.
	/// </summary>
	/// <remarks>
	/// byte[] is treated as a primitive.
	/// </remarks>
	/// <typeparam name="T">The type of the primitive.</typeparam>
	/// <param name="value">The primitive value.</param>
	public virtual void VisitPrimitive<T>(T value)
	{
	}

	/// <summary>
	/// Visits a generic PPtr (Serialized Pointer) node with a specified PPtr type and target Unity object type.
	/// </summary>
	/// <remarks>
	/// This method is called when encountering a generic PPtr node during asset traversal.
	/// A generic PPtr is a serialized pointer to a Unity object within the asset file.
	/// </remarks>
	/// <typeparam name="TPPtr">The type of the generic PPtr.</typeparam>
	/// <typeparam name="TAsset">The type of the Unity object pointed to by the PPtr.</typeparam>
	/// <param name="pptr">The generic PPtr instance representing the serialized pointer.</param>
	public void VisitPPtr<TPPtr, TAsset>(TPPtr pptr)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		VisitPPtr(new PPtr<TAsset>(pptr.FileID, pptr.PathID));
	}

	/// <summary>
	/// Visits a Unity PPtr (Serialized Pointer) node.
	/// </summary>
	/// <remarks>
	/// This method is called when encountering a Unity PPtr node during asset traversal.
	/// A PPtr is a serialized pointer to a Unity object within the asset file.
	/// </remarks>
	/// <typeparam name="TAsset">The type of the Unity object pointed to by the PPtr.</typeparam>
	/// <param name="pptr">The PPtr instance representing the serialized pointer.</param>
	public virtual void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
		where TAsset : IUnityObjectBase
	{
	}
}