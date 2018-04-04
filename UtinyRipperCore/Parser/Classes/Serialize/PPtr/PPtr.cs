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
		public PPtr(Object @object)
		{
			FileIndex = 0;
			PathID = @object.PathID;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsLongID(Version version)
		{
			return version.IsGreaterEqual(5);
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

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			if (IsNull)
			{
				return ExportPointer.EmptyPointer.ExportYAML(exporter);
			}

			T @object = FindObject(exporter.File);
			if (@object == null)
			{
				ClassIDType classType = typeof(T).ToClassIDType();
				AssetType assetType = exporter.ToExportType(classType);
				ExportPointer pointer = new ExportPointer(classType, assetType);
				return pointer.ExportYAML(exporter);
			}

			ExportPointer exPointer = exporter.CreateExportPointer(@object);
			return exPointer.ExportYAML(exporter);
		}

		public T FindObject(ISerializedFile file)
		{
			if (IsNull)
			{
				return null;
			}
			Object @object = file.FindObject(FileIndex, PathID);
			switch (@object)
			{
				case null:
					return null;
				case T t:
					return t;
				default:
					throw new Exception($"Object's type {@object.ClassID} doesn't assignable from {typeof(T).Name}");
			}
		}

		public T TryGetObject(ISerializedFile file)
		{
			if(IsNull)
			{
				return null;
			}
			return GetObject(file);
		}

		public T GetObject(ISerializedFile file)
		{
			if (IsNull)
			{
				throw new Exception("Can't get null PPtr");
			}
			Object @object = file.GetObject(FileIndex, PathID);
			if(@object is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {@object.ClassID} doesn't assignable from {typeof(T).Name}");
		}

		public bool IsObject(ISerializedFile file, Object @object)
		{
			if (FileIndex == 0)
			{
				return @object.PathID == PathID;
			}
			else
			{
				return @object.PathID == PathID && file.Dependencies[FileIndex - 1].IsFile(@object.File);
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
			
			T obj = FindObject(file);
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
				string depName = FileIndex == 0 ? file.Name : file.Dependencies[FileIndex - 1].FilePath;
				ClassIDType classID = typeof(T).ToClassIDType();
				return $"[{classID} {PathID}({depName})]";
			}
			return ToString();
		}

		public bool IsValid(ISerializedFile file)
		{
			return FindObject(file) != null;
		}

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
