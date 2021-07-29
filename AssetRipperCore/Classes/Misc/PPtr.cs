using AssetRipper.Converters.Game;
using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Meta;
using AssetRipper.Classes.Utils.Extensions;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Classes.Misc
{
	public struct PPtr<T> : IAsset where T : Object.Object
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

		public PPtr<T1> CastTo<T1>() where T1 : Object.Object
		{
			return new PPtr<T1>(FileIndex, PathID);
		}

		public static void GenerateTypeTree(TypeTreeContext context, string type, string name)
		{
			context.AddPPtr(type, name);
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathID = reader.Layout.PPtr.IsLongID ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileIndex);
			if (writer.Layout.PPtr.IsLongID)
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
				return null;
			}
			Object.Object asset = file.FindAsset(FileIndex, PathID);
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

		public T TryGetAsset(IAssetContainer file)
		{
			if (IsNull)
			{
				return null;
			}
			return GetAsset(file);
		}

		public T GetAsset(IAssetContainer file)
		{
			if (IsNull)
			{
				throw new Exception("Can't get null PPtr");
			}
			Object.Object asset = file.GetAsset(FileIndex, PathID);
			if (asset is T t)
			{
				return t;
			}
			throw new Exception($"Object's type {asset.ClassID} isn't assignable from {typeof(T).Name}");
		}

		public bool IsAsset(Object.Object asset)
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

		public bool IsAsset(IAssetContainer file, Object.Object asset)
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

		public bool IsVirtual => FileIndex == VirtualSerializedFile.VirtualFileIndex;
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
