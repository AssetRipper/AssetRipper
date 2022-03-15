using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

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

		public static YAMLNode ExportYAML<T>(IExportContainer container, int fileIndex, long pathID) where T : IUnityObjectBase
		{
			return new PPtr<T>(fileIndex, pathID).ExportYAML(container);
		}

		public static YAMLNode ExportYAML<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				return MetaPtr.NullPtr.ExportYAML(container);
			}

			T asset = pptr.FindAsset(container);
			if (asset == null)
			{
				ClassIDType classType = typeof(T).ToClassIDType();
				AssetType assetType = container.ToExportType(classType);
				MetaPtr pointer = new MetaPtr(classType, assetType);
				return pointer.ExportYAML(container);
			}

			MetaPtr exPointer = container.CreateExportPointer(asset);
			return exPointer.ExportYAML(container);
		}

		public static void CopyValues(this IPPtr destination, IPPtr source)
		{
			destination.FileIndex = source.FileIndex;
			destination.PathID = source.PathID;
		}

		public static PPtr<T>[] CastArray<T>(IPPtr[] array) where T : IUnityObjectBase
		{
			var result = new PPtr<T>[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				result[i] = new PPtr<T>(array[i]);
			}
			return result;
		}

		public static T FindAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				return default;
			}
			IUnityObjectBase asset = file.FindAsset(pptr.FileIndex, pptr.PathID);
			return asset switch
			{
				null => default,
				UnknownObject or UnreadableObject => default,
				T t => t,
				_ => throw new Exception($"Object's type {asset.GetType().Name} isn't assignable from {typeof(T).Name}"),
			};
		}

		public static T TryGetAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				return default;
			}
			return pptr.GetAsset(file);
		}

		public static T GetAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
		{
			if (pptr.IsNull())
			{
				throw new Exception("Can't get null PPtr");
			}
			IUnityObjectBase asset = file.GetAsset(pptr.FileIndex, pptr.PathID);
			if (asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
		}

		public static bool IsAsset<T>(this IPPtr<T> pptr, IUnityObjectBase asset) where T : IUnityObjectBase
		{
			if (pptr.FileIndex == 0)
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
			if (pptr.FileIndex == 0)
			{
				if (file == asset.SerializedFile)
				{
					return asset.PathID == pptr.PathID;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return asset.PathID == pptr.PathID && file.Dependencies[pptr.FileIndex - 1].IsFile(asset.SerializedFile);
			}
		}

		public static bool IsValid<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
		{
			return pptr.FindAsset(container) != null;
		}

		public static string ToLogString<T>(this IPPtr<T> pptr, IAssetContainer container) where T : IUnityObjectBase
		{
			string depName = pptr.FileIndex == 0 ? container.Name : container.Dependencies[pptr.FileIndex - 1].PathNameOrigin;
			return $"[{depName}]{typeof(T).Name}_{pptr.PathID}";
		}

		public static bool IsVirtual(this IPPtr pptr) => pptr.FileIndex == VirtualSerializedFile.VirtualFileIndex;
		/// <summary>
		/// PathID == 0
		/// </summary>
		public static bool IsNull(this IPPtr pptr) => pptr.PathID == 0;
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
		long PathID { get; set; }
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
			PathID = pathID;
		}

		public PPtr(IPPtr other) : this(other.FileIndex, other.PathID) { }

		public static bool operator ==(PPtr<T> left, PPtr<T> right)
		{
			return left.FileIndex == right.FileIndex && left.PathID == right.PathID;
		}

		public static bool operator !=(PPtr<T> left, PPtr<T> right)
		{
			return left.FileIndex != right.FileIndex || left.PathID != right.PathID;
		}

		public PPtr<T1> CastTo<T1>() where T1 : IUnityObjectBase
		{
			return new PPtr<T1>(FileIndex, PathID);
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathID = PPtr.IsLongID(reader.Version) ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileIndex);
			if (PPtr.IsLongID(writer.Version))
			{
				writer.Write(PathID);
			}
			else
			{
				writer.Write((int)PathID);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return PPtr.ExportYAML(this, container);
		}

		public override string ToString()
		{
			return $"[{FileIndex}, {PathID}]";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is PPtr<T> ptr)
			{
				return ptr == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 149;
			unchecked
			{
				hash = hash + 181 * FileIndex.GetHashCode();
				hash = hash * 173 + PathID.GetHashCode();
			}
			return hash;
		}

		public bool Equals(PPtr<T> other)
		{
			return this == other;
		}

		public bool IsVirtual => this.IsVirtual();
		/// <summary>
		/// PathID == 0
		/// </summary>
		public bool IsNull => this.IsNull();
		/// <inheritdoc/>
		public int FileIndex { get; set; }
		/// <inheritdoc/>
		public long PathID { get; set; }
	}
}
