using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct PPtr<T> : IPPtr<T>
		where T: Object
	{
		public PPtr(Object asset)
		{
			FileIndex = 0;
			PathID = asset.PathID;
		}

		public PPtr(PPtr<T> copy)
		{
			FileIndex = copy.FileIndex;
			PathID = copy.PathID;
		}

		public static PPtr<T> CreateVirtualPointer(Object asset)
		{
			PPtr<T> ptr = new PPtr<T>()
			{
				FileIndex = VirtualFileIndex,
				PathID = asset.PathID,
			};
			return ptr;
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
			return new PPtr<T1>()
			{
				FileIndex = FileIndex,
				PathID = PathID,
			};
		}

		public void Read(AssetStream stream)
		{
			FileIndex = stream.ReadInt32();
			if (IsLongID(stream.Version))
			{
				PathID = stream.ReadInt64();
			}
			else
			{
				PathID = stream.ReadInt32();
			}
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
			Object asset = container.FindObject(FileIndex, PathID);
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
					Logger.Log(LogType.Warning, LogCategory.Export, $"{owner}'s {name} {ToLogString(file)} wasn't found ");
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

		public bool IsValid(IExportContainer container)
		{
			return FindAsset(container) != null;
		}

		public bool IsVirtual => FileIndex == VirtualFileIndex;
		public bool IsNull => PathID == 0;

		/// <summary>
		/// 0 means current file
		/// </summary>
		public int FileIndex { get; private set; }
		/// <summary>
		/// It is acts more like a hash in some cases
		/// </summary>
		public long PathID { get; private set; }

		public const int VirtualFileIndex = -1;
	}
}
