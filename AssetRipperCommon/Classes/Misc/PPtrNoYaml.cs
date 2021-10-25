using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Classes.Misc
{
	public struct PPtrNoYaml<T> : IAssetReadable, IAssetWritable where T : UnityObjectBase
	{
		public PPtrNoYaml(int fileIndex, long pathID)
		{
			FileIndex = fileIndex;
			PathID = pathID;
		}

		public static bool operator ==(PPtrNoYaml<T> left, PPtrNoYaml<T> right)
		{
			return left.FileIndex == right.FileIndex && left.PathID == right.PathID;
		}

		public static bool operator !=(PPtrNoYaml<T> left, PPtrNoYaml<T> right)
		{
			return left.FileIndex != right.FileIndex || left.PathID != right.PathID;
		}

		public PPtrNoYaml<T1> CastTo<T1>() where T1 : UnityObjectBase
		{
			return new PPtrNoYaml<T1>(FileIndex, PathID);
		}

		/// <summary>
		/// At least version 5.0.0
		/// </summary>
		private static bool IsLongID(UnityVersion version)
		{
			// NOTE: unknown version SerializedFiles.FormatVersion.Unknown_14
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathID = IsLongID(reader.Version) ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileIndex);
			if (IsLongID(writer.Version))
			{
				writer.Write(PathID);
			}
			else
			{
				writer.Write((int)PathID);
			}
		}

		public bool IsAsset(UnityObjectBase asset)
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

		public bool IsAsset(IAssetContainer file, UnityObjectBase asset)
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
			if (obj is PPtrNoYaml<T> ptr)
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

		public bool IsVirtual => FileIndex == -1;
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