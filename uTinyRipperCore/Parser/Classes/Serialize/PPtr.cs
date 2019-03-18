using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct PPtr<T> : IYAMLExportable, IAssetReadable
		where T: Object
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

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsLongID(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public PPtr<T1> CastTo<T1>()
			where T1 : Object
		{
			return new PPtr<T1>(FileIndex, PathID);
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathID = IsLongID(reader.Version) ? reader.ReadInt64() : reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			if (IsNull)
			{
				return ExportPointer.EmptyPointer.ExportYAML(container);
			}

			T asset = FindAsset(container);
			if (asset == null)
			{
				ClassIDType classType = typeof(T).ToClassIDType();
				AssetType assetType = container.ToExportType(classType);
				ExportPointer pointer = new ExportPointer(classType, assetType);
				return pointer.ExportYAML(container);
			}

			ExportPointer exPointer = container.CreateExportPointer(asset);
			return exPointer.ExportYAML(container);
		}

		public T FindAsset(ISerializedFile file)
		{
			if (IsNull)
			{
				return null;
			}
			Object asset = file.FindAsset(FileIndex, PathID);
			switch (asset)
			{
				case null:
					return null;
				case T t:
					return t;
				default:
					throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
			}
		}

		public T FindAsset(IExportContainer container)
		{
			if (IsNull)
			{
				return null;
			}
			Object asset = container.FindAsset(FileIndex, PathID);
			switch (asset)
			{
				case null:
					return null;
				case T t:
					return t;
				default:
					throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
			}
		}

		public T TryGetAsset(ISerializedFile file)
		{
			if(IsNull)
			{
				return null;
			}
			return GetAsset(file);
		}

		public T GetAsset(ISerializedFile file)
		{
			if (IsNull)
			{
				throw new Exception("Can't get null PPtr");
			}
			Object asset = file.GetAsset(FileIndex, PathID);
			if(asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
		}

		public bool IsAsset(Object asset)
		{
			if(FileIndex == 0)
			{
				return asset.PathID == PathID;
			}
			else
			{
				throw new NotSupportedException("Need to specify file where to find");
			}
		}

		public bool IsAsset(ISerializedFile file, Object asset)
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

		public Object FetchDependency(ISerializedFile file)
		{
			return FetchDependency(file, null, null);
		}

		public Object FetchDependency(ISerializedFile file, Func<string> logger, string name)
		{
			return FetchDependency(file, logger != null, logger, name);
		}

		public Object FetchDependency(ISerializedFile file, bool isLog, Func<string> owner, string name)
		{
			if (IsNull)
			{
				return null;
			}
			
			T obj = FindAsset(file);
			if (obj == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"{owner.Invoke()}'s {name} {ToLogString(file)} hasn't been found");
				}
			}
			else
			{
				return obj;
			}
			return null;
		}

		public override string ToString()
		{
			return $"[{FileIndex}, {PathID}]";
		}

		public string ToLogString(ISerializedFile file)
		{
			if(Config.IsAdvancedLog)
			{
				string depName = FileIndex == 0 ? file.Name : file.Dependencies[FileIndex - 1].FilePathOrigin;
				ClassIDType classID = typeof(T).ToClassIDType();
				return $"[{classID} {PathID}({depName})]";
			}
			return ToString();
		}

		public override bool Equals(object obj)
		{
			if(obj == null)
			{
				return false;
			}
			if(obj is PPtr<T> ptr)
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

		public bool IsVirtual => FileIndex == VirtualSerializedFile.VirtualFileIndex;
		public bool IsNull => PathID == 0;

		/// <summary>
		/// 0 means current file
		/// </summary>
		public int FileIndex { get; private set; }
		/// <summary>
		/// It is acts more like a hash in some cases
		/// </summary>
		public long PathID { get; private set; }
	}
}
