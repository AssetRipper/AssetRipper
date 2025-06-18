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
	/// <param name="asset">The asset being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the asset node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterAsset(IUnityAssetBase asset)
	{
		return true;
	}

	/// <summary>
	/// Called between two fields of an asset node during traversal.
	/// </summary>
	/// <param name="asset">The asset having its fields divided.</param>
	public virtual void DivideAsset(IUnityAssetBase asset)
	{
	}

	/// <summary>
	/// Called when exiting an asset node during traversal.
	/// </summary>
	/// <param name="asset">The asset being exited.</param>
	public virtual void ExitAsset(IUnityAssetBase asset)
	{
	}

	/// <summary>
	/// Called when entering a field node during traversal.
	/// </summary>
	/// <param name="name">The name of the field being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the children of the field node,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterField(IUnityAssetBase asset, string name)
	{
		return true;
	}

	/// <summary>
	/// Called when exiting a field node during traversal.
	/// </summary>
	/// <param name="name">The name of the field being exited.</param>
	public virtual void ExitField(IUnityAssetBase asset, string name)
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
	public virtual bool EnterList<T>(IReadOnlyList<T> list)
		where T : notnull
	{
		return true;
	}

	/// <summary>
	/// Called between two elements of a list node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the list elements being divided.</typeparam>
	/// <param name="list">The list having its elements divided.</param>
	public virtual void DivideList<T>(IReadOnlyList<T> list)
		where T : notnull
	{
	}

	/// <summary>
	/// Called when exiting a list node during traversal.
	/// </summary>
	/// <typeparam name="T">The type of the list elements.</typeparam>
	/// <param name="list">The list being exited.</param>
	public virtual void ExitList<T>(IReadOnlyList<T> list)
		where T : notnull
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
	public virtual bool EnterDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
		where TKey : notnull
		where TValue : notnull
	{
		return true;
	}

	/// <summary>
	/// Called between two pairs of a dictionary node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of dictionary keys.</typeparam>
	/// <typeparam name="TValue">The type of dictionary values.</typeparam>
	/// <param name="dictionary">The dictionary having its pairs divided.</param>
	public virtual void DivideDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
		where TKey : notnull
		where TValue : notnull
	{
	}

	/// <summary>
	/// Called when exiting a dictionary node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of dictionary keys.</typeparam>
	/// <typeparam name="TValue">The type of dictionary values.</typeparam>
	/// <param name="dictionary">The dictionary being exited.</param>
	public virtual void ExitDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
		where TKey : notnull
		where TValue : notnull
	{
	}

	/// <summary>
	/// Called when entering a dictionary pair during traversal.
	/// </summary>
	/// <remarks>
	/// This calls <see cref="EnterPair{TKey, TValue}(KeyValuePair{TKey, TValue})"/> by default.
	/// </remarks>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The dictionary pair being entered.</param>
	/// <returns>
	///   <c>true</c> to continue visiting the key and value of the dictionary pair,
	///   <c>false</c> to skip visiting the children and not call the exit method.
	/// </returns>
	public virtual bool EnterDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
	{
		return EnterPair(pair);
	}

	/// <summary>
	/// Called between the key and value of a dictionary pair during traversal.
	/// </summary>
	/// <remarks>
	/// This calls <see cref="DividePair{TKey, TValue}(KeyValuePair{TKey, TValue})"/> by default.
	/// </remarks>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The dictionary pair having its key and value divided.</param>
	public virtual void DivideDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
	{
		DividePair(pair);
	}

	/// <summary>
	/// Called when exiting a dictionary pair during traversal.
	/// </summary>
	/// <remarks>
	/// This calls <see cref="ExitPair{TKey, TValue}(KeyValuePair{TKey, TValue})"/> by default.
	/// </remarks>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The dictionary pair being exited.</param>
	public virtual void ExitDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
	{
		ExitPair(pair);
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
	public virtual bool EnterPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
	{
		return true;
	}

	/// <summary>
	/// Called between the key and value of a key-value pair node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The key-value pair having its key and value divided.</param>
	public virtual void DividePair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
	{
	}

	/// <summary>
	/// Called when exiting a key-value pair node during traversal.
	/// </summary>
	/// <typeparam name="TKey">The type of the key in the pair.</typeparam>
	/// <typeparam name="TValue">The type of the value in the pair.</typeparam>
	/// <param name="pair">The key-value pair being exited.</param>
	public virtual void ExitPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		where TKey : notnull
		where TValue : notnull
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
		where T : notnull
	{
	}

	/// <summary>
	/// Visits a generic PPtr (Serialized Pointer) node with a specified PPtr type and target Unity object type.
	/// </summary>
	/// <remarks>
	/// This method is called when encountering a generic PPtr node during asset traversal.
	/// A generic PPtr is a serialized pointer to a Unity object within the asset file.
	/// </remarks>
	/// <typeparam name="TAsset">The type of the Unity object pointed to by the PPtr.</typeparam>
	/// <param name="pptr">The generic PPtr instance representing the serialized pointer.</param>
	public void VisitPPtr<TAsset>(IPPtr<TAsset> pptr)
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
