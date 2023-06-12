using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.IO.Files;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.Metadata
{
	public interface IPPtr
	{
		/// <summary>
		/// Zero means the asset is located within the current file.
		/// </summary>
		int FileID { get; set; }
		/// <summary>
		/// It is sometimes sequential and sometimes more like a hash. Zero signifies a null reference.
		/// </summary>
		long PathID { get; set; }
	}

	public interface IPPtr<T> : IPPtr where T : IUnityObjectBase
	{
	}

	public static class PPtrExtensions
	{
		//To do: move to source generation as an injected helper.
		public static YamlNode ExportYaml<T>(this IPPtr<T> pptr, IExportContainer container, int classID) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				return MetaPtr.NullPtr.ExportYaml();
			}

			T? asset = pptr.TryGetAsset(container);
			if (asset is null)
			{
				AssetType assetType = container.ToExportType(typeof(T));
				MetaPtr pointer = MetaPtr.CreateMissingReference(classID, assetType);
				return pointer.ExportYaml();
			}
			else
			{
				MetaPtr exPointer = container.CreateExportPointer(asset);
				return exPointer.ExportYaml();
			}
		}

		public static void CopyValues(this IPPtr destination, PPtr source)
		{
			destination.FileID = source.FileID;
			destination.PathID = source.PathID;
		}

		public static void SetNull(this IPPtr destination)
		{
			destination.FileID = 0;
			destination.PathID = 0;
		}

		public static bool TryGetAsset<T>(this IPPtr<T> pptr, IAssetContainer file, [NotNullWhen(true)] out T? asset) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				asset = default;
				return false;
			}
			IUnityObjectBase? @object = file.TryGetAsset(pptr.FileID, pptr.PathID);
			switch (@object)
			{
				case null:
					asset = default;
					return false;
				case T t:
					asset = t;
					return true;
				case NullObject:
					asset = default;
					return false;
				default:
					throw new Exception($"Object's type {@object.GetType().Name} isn't assignable from {typeof(T).Name}");
			}
		}

		//Called from source gen
		public static T? TryGetAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
		{
			pptr.TryGetAsset(file, out T? asset);
			return asset;
		}

		public static T GetAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				throw new Exception("Can't get null PPtr");
			}
			IUnityObjectBase asset = file.GetAsset(pptr.FileID, pptr.PathID);
			if (asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.GetType().Name} isn't assignable from {typeof(T).Name}");
		}

		//Called from source gen
		public static void SetAsset<T>(this IPPtr<T> pptr, AssetCollection collection, T? asset) where T : IUnityObjectBase
		{
			pptr.CopyValues(collection.ForceCreatePPtr(asset));
		}

		public static bool IsAsset<T>(this IPPtr<T> pptr, IUnityObjectBase asset) where T : IUnityObjectBase
		{
			if (pptr.FileID == 0)
			{
				return asset.PathID == pptr.PathID;
			}
			else
			{
				throw new NotSupportedException("Need to specify file where to find");
			}
		}

		public static bool IsAsset<T>(this IPPtr<T> pptr, IAssetContainer file, IUnityObjectBase asset) where T : IUnityObjectBase
		{
			if (asset.PathID != pptr.PathID)
			{
				return false;
			}
			else if (pptr.FileID == 0)
			{
				return file == asset.Collection;
			}
			else
			{
				return file.Dependencies[pptr.FileID - 1] == asset.Collection;
			}
		}

		public static bool IsValid<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
		{
			return pptr.TryGetAsset(container) != null;
		}

		public static string ToLogString<T>(this IPPtr<T> pptr, IAssetContainer container) where T : IUnityObjectBase
		{
			string depName = pptr.FileID == 0 ? container.Name : container.Dependencies[pptr.FileID - 1]?.Name ?? "Null";
			return $"[{depName}]{typeof(T).Name}_{pptr.PathID}";
		}

		public static PPtr<T> CastTo<T>(this IPPtr pptr) where T : IUnityObjectBase
		{
			return new PPtr<T>(pptr.FileID, pptr.PathID);
		}

		//Called from source gen
		public static PPtr ToStruct(this IPPtr pptr)
		{
			return new PPtr(pptr.FileID, pptr.PathID);
		}

		public static PPtr<T> ToStruct<T>(this IPPtr<T> pptr) where T : IUnityObjectBase
		{
			return new PPtr<T>(pptr.FileID, pptr.PathID);
		}

		/// <summary>
		/// PathID == 0
		/// </summary>
		public static bool IsNull(this IPPtr pptr) => pptr.PathID == 0;
	}
}
