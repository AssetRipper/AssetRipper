using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core.Classes.Misc
{
	public static class PPtr
	{
		/// <summary>
		/// At least version 5.0.0
		/// </summary>
		public static bool IsLongID(UnityVersion version)
		{
			// NOTE: unknown version SerializedFiles.FormatVersion.Unknown_14
			return version.IsGreaterEqual(5);
		}

		public static YamlNode ExportYaml<T>(IExportContainer container, int fileIndex, long pathID) where T : IUnityObjectBase
		{
			return new PPtr<T>(fileIndex, pathID).ExportYaml(container);
		}

		public static YamlNode ExportYaml<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				return MetaPtr.NullPtr.ExportYaml(container);
			}

			T? asset = pptr.TryGetAsset(container);
			if (asset is null)
			{
				AssetType assetType = container.ToExportType(typeof(T));
				MetaPtr pointer = MetaPtr.CreateMissingReference(VersionHandling.VersionManager.AssetFactory.GetClassIdForType(typeof(T)), assetType);
				return pointer.ExportYaml(container);
			}
			else
			{
				MetaPtr exPointer = container.CreateExportPointer(asset);
				return exPointer.ExportYaml(container);
			}
		}

		public static void CopyValues(this IPPtr destination, IPPtr source)
		{
			destination.FileIndex = source.FileIndex;
			destination.PathIndex = source.PathIndex;
		}

		//prevents boxing
		public static void CopyValues<T>(this IPPtr destination, PPtr<T> source) where T : IUnityObjectBase
		{
			destination.FileIndex = source.FileIndex;
			destination.PathIndex = source.PathIndex;
		}

		public static void SetNull(this IPPtr destination)
		{
			destination.FileIndex = 0;
			destination.PathIndex = 0;
		}

		public static PPtr<T>[] CastArray<T>(IPPtr[] array) where T : IUnityObjectBase
		{
			PPtr<T>[] result = new PPtr<T>[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				result[i] = new PPtr<T>(array[i]);
			}
			return result;
		}

		public static bool TryGetAsset<T>(this IPPtr<T> pptr, IAssetContainer file, [NotNullWhen(true)] out T? asset) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				asset = default;
				return false;
			}
			IUnityObjectBase? @object = file.TryGetAsset(pptr.FileIndex, pptr.PathIndex);
			switch (@object)
			{
				case null:
					asset = default;
					return false;
				case T t:
					asset = t;
					return true;
				case UnknownObject or UnreadableObject:
					asset = default;
					return false;
				default:
					throw new Exception($"Object's type {@object.GetType().Name} isn't assignable from {typeof(T).Name}");
			}
		}

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
			IUnityObjectBase asset = file.GetAsset(pptr.FileIndex, pptr.PathIndex);
			if (asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.GetType().Name} isn't assignable from {typeof(T).Name}");
		}

		public static bool IsAsset<T>(this IPPtr<T> pptr, IUnityObjectBase asset) where T : IUnityObjectBase
		{
			if (pptr.FileIndex == 0)
			{
				return asset.PathID == pptr.PathIndex;
			}
			else
			{
				throw new NotSupportedException("Need to specify file where to find");
			}
		}

		public static bool IsAsset<T>(this IPPtr<T> pptr, IAssetContainer file, IUnityObjectBase asset) where T : IUnityObjectBase
		{
			if (asset.PathID != pptr.PathIndex)
			{
				return false;
			}
			else if (pptr.FileIndex == 0)
			{
				return file == asset.SerializedFile;
			}
			else
			{
				return file.Dependencies[pptr.FileIndex - 1].IsFile(asset.SerializedFile);
			}
		}

		public static bool IsValid<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
		{
			return pptr.TryGetAsset(container) != null;
		}

		public static string ToLogString<T>(this IPPtr<T> pptr, IAssetContainer container) where T : IUnityObjectBase
		{
			string depName = pptr.FileIndex == 0 ? container.Name : container.Dependencies[pptr.FileIndex - 1].PathNameOrigin;
			return $"[{depName}]{typeof(T).Name}_{pptr.PathIndex}";
		}

		public static PPtr<T1> CastTo<T1>(this IPPtr pptr) where T1 : IUnityObjectBase
		{
			return new PPtr<T1>(pptr.FileIndex, pptr.PathIndex);
		}

		public static bool IsVirtual(this IPPtr pptr) => pptr.FileIndex == VirtualSerializedFile.VirtualFileIndex;
		/// <summary>
		/// PathID == 0
		/// </summary>
		public static bool IsNull(this IPPtr pptr) => pptr.PathIndex == 0;
	}

	public interface IPPtr : IAsset
	{
		/// <summary>
		/// 0 means current file
		/// </summary>
		int FileIndex { get; set; }
		/// <summary>
		/// It is acts more like a hash in some cases
		/// </summary>
		long PathIndex { get; set; }
	}

	public interface IPPtr<T> : IPPtr where T : IUnityObjectBase
	{
	}

	public sealed class PPtr<T> : IPPtr<T>, IEquatable<PPtr<T>> where T : IUnityObjectBase
	{
		public PPtr() { }

		public PPtr(int fileIndex, long pathID)
		{
			FileIndex = fileIndex;
			PathIndex = pathID;
		}

		public PPtr(IPPtr other) : this(other.FileIndex, other.PathIndex) { }

		public static bool operator ==(PPtr<T> left, PPtr<T> right)
		{
			return left.FileIndex == right.FileIndex && left.PathIndex == right.PathIndex;
		}

		public static bool operator !=(PPtr<T> left, PPtr<T> right)
		{
			return left.FileIndex != right.FileIndex || left.PathIndex != right.PathIndex;
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathIndex = PPtr.IsLongID(reader.Version) ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileIndex);
			if (PPtr.IsLongID(writer.Version))
			{
				writer.Write(PathIndex);
			}
			else
			{
				writer.Write((int)PathIndex);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			return PPtr.ExportYaml(this, container);
		}

		public override string ToString()
		{
			return $"[{FileIndex}, {PathIndex}]";
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as PPtr<T>);
		}

		public override int GetHashCode()
		{
			int hash = 149;
			unchecked
			{
				hash = hash + (181 * FileIndex.GetHashCode());
				hash = (hash * 173) + PathIndex.GetHashCode();
			}
			return hash;
		}

		public bool Equals(PPtr<T>? other)
		{
			return other is not null && this == other;
		}

		public bool IsVirtual => this.IsVirtual();
		/// <summary>
		/// PathID == 0
		/// </summary>
		public bool IsNull => this.IsNull();
		/// <inheritdoc/>
		public int FileIndex { get; set; }
		/// <inheritdoc/>
		public long PathIndex { get; set; }
	}
}
