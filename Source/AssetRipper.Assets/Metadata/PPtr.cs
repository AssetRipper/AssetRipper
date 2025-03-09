namespace AssetRipper.Assets.Metadata;

/// <summary>
/// A Unity pointer to any type of object.
/// </summary>
/// <param name="FileID">Zero means the asset is located within the current file.</param>
/// <param name="PathID">It is sometimes sequential and sometimes more like a hash. Zero signifies a null reference.</param>
public readonly record struct PPtr(int FileID, long PathID)
{
	public PPtr(long PathID) : this(0, PathID) { }

	public PPtr(IPPtr pptr) : this(pptr.FileID, pptr.PathID) { }

	/// <summary>
	/// PathID == 0
	/// </summary>
	public bool IsNull => PathID == 0;

	public static implicit operator PPtr<IUnityObjectBase>(PPtr pptr) => new PPtr<IUnityObjectBase>(pptr.FileID, pptr.PathID);
}

/// <summary>
/// A Unity pointer to a specific type of object.
/// </summary>
/// <typeparam name="T">The type of object this references.</typeparam>
/// <param name="FileID">Zero means the asset is located within the current file.</param>
/// <param name="PathID">It is sometimes sequential and sometimes more like a hash. Zero signifies a null reference.</param>
public readonly record struct PPtr<T>(int FileID, long PathID) where T : IUnityObjectBase
{
	public PPtr(long PathID) : this(0, PathID) { }

	public PPtr<TCast> Cast<TCast>() where TCast : IUnityObjectBase
	{
		return new PPtr<TCast>(FileID, PathID);
	}

	/// <summary>
	/// PathID == 0
	/// </summary>
	public bool IsNull => PathID == 0;

	public static implicit operator PPtr(PPtr<T> pptr) => new PPtr(pptr.FileID, pptr.PathID);
	public static explicit operator PPtr<T>(PPtr pptr) => new PPtr<T>(pptr.FileID, pptr.PathID);
	public static implicit operator PPtr<IUnityObjectBase>(PPtr<T> pptr) => new PPtr<IUnityObjectBase>(pptr.FileID, pptr.PathID);
	public static explicit operator PPtr<T>(PPtr<IUnityObjectBase> pptr) => new PPtr<T>(pptr.FileID, pptr.PathID);
}
