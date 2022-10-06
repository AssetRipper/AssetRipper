namespace AssetRipper.Assets.Metadata;

public readonly record struct PPtr(int FileID, long PathID)
{
	public PPtr(long PathID) : this(0, PathID) { }

	/// <summary>
	/// PathID == 0
	/// </summary>
	public bool IsNull => PathID == 0;

	public static implicit operator PPtr<IUnityObjectBase>(PPtr pptr) => new PPtr<IUnityObjectBase>(pptr.FileID, pptr.PathID);
}

public readonly record struct PPtr<T>(int FileID, long PathID) where T : IUnityObjectBase
{
	public PPtr(long PathID) : this(0, PathID) { }

	/// <summary>
	/// PathID == 0
	/// </summary>
	public bool IsNull => PathID == 0;

	public static implicit operator PPtr(PPtr<T> pptr) => new PPtr(pptr.FileID, pptr.PathID);
	public static explicit operator PPtr<T>(PPtr pptr) => new PPtr<T>(pptr.FileID, pptr.PathID);
	public static implicit operator PPtr<IUnityObjectBase>(PPtr<T> pptr) => new PPtr<IUnityObjectBase>(pptr.FileID, pptr.PathID);
	public static explicit operator PPtr<T>(PPtr<IUnityObjectBase> pptr) => new PPtr<T>(pptr.FileID, pptr.PathID);
}
