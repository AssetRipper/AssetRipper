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
		public const string FileIDName = "m_FileID";
		public const string PathIDName = "m_PathID";

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

		public static void SetValues(this IPPtr destination, IPPtr source)
		{
			destination.FileIndex = source.FileIndex;
			destination.PathID = source.PathID;
		}
	}

	public interface IPPtr
	{
		int FileIndex { get; set; }
		long PathID { get; set; }
	}

	public struct PPtr<T> : IAsset, IPPtr where T : IUnityObjectBase
	{
		public PPtr(int fileIndex, long pathID)
		{
			FileIndex = fileIndex;
			PathID = pathID;
		}

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
			if (IsNull)
			{
				return MetaPtr.NullPtr.ExportYAML(container);
			}

			T asset = FindAsset(container);
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

		public T FindAsset(IAssetContainer file)
		{
			if (IsNull)
			{
				return default;
			}
			IUnityObjectBase asset = file.FindAsset(FileIndex, PathID);
			switch (asset)
			{
				case null:
					return default;
				case UnknownObject or UnreadableObject:
					return default;
				case T t:
					return t;
				default:
					throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
			}
		}

		public T TryGetAsset(IAssetContainer file)
		{
			if (IsNull)
			{
				return default;
			}
			return GetAsset(file);
		}

		public T GetAsset(IAssetContainer file)
		{
			if (IsNull)
			{
				throw new Exception("Can't get null PPtr");
			}
			IUnityObjectBase asset = file.GetAsset(FileIndex, PathID);
			if (asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
		}

		public bool IsAsset(IUnityObjectBase asset)
		{
			if (FileIndex == 0)
			{
				return asset.PathID == PathID;
			}
			else
			{
				throw new NotSupportedException("Need to specify file where to find");
			}
		}

		public bool IsAsset(IAssetContainer file, IUnityObjectBase asset)
		{
			if (FileIndex == 0)
			{
				if (file == asset.File)
				{
					return asset.PathID == PathID;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return asset.PathID == PathID && file.Dependencies[FileIndex - 1].IsFile(asset.File);
			}
		}

		public override string ToString()
		{
			return $"[{FileIndex}, {PathID}]";
		}

		public string ToLogString(IAssetContainer container)
		{
			string depName = FileIndex == 0 ? container.Name : container.Dependencies[FileIndex - 1].PathNameOrigin;
			ClassIDType classID = typeof(T).ToClassIDType();
			return $"[{depName}]{classID}_{PathID}";
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

		public bool IsValid(IExportContainer container)
		{
			return FindAsset(container) != null;
		}

		public Type AssetType => typeof(T);

		public bool IsVirtual => FileIndex == VirtualSerializedFile.VirtualFileIndex;
		/// <summary>
		/// PathID == 0
		/// </summary>
		public bool IsNull => PathID == 0;

		/// <summary>
		/// 0 means current file
		/// </summary>
		public int FileIndex { get; set; }
		/// <summary>
		/// It is acts more like a hash in some cases
		/// </summary>
		public long PathID { get; set; }
	}
}
