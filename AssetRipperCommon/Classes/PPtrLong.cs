using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes
{
	class PPtrLong<T> : IPPtr<T> where T : UnityObjectBase
	{
		/// <summary>
		/// 0 means current file
		/// </summary>
		private int FileIndex;
		/// <summary>
		/// It is acts more like a hash in some cases
		/// </summary>
		private long PathID;

		public PPtrLong() { }

		public PPtrLong(int fileIndex, long pathID)
		{
			FileIndex = fileIndex;
			PathID = pathID;
		}

		/// <summary>
		/// PathID == 0
		/// </summary>
		public bool IsNull => PathID == 0;

		/// <summary>
		/// FileIndex == -1
		/// </summary>
		public bool IsVirtual => FileIndex == -1;

		public IPPtr<T1> CastTo<T1>() where T1 : UnityObjectBase
		{
			return new PPtrLong<T1>(FileIndex, PathID);
		}

		public YAMLNode ExportYAML()
		{
			throw new NotImplementedException();
		}

		public bool IsAsset(UnityObjectBase asset)
		{
			throw new NotImplementedException();
		}

		public void Read(AssetReader reader)
		{
			FileIndex = reader.ReadInt32();
			PathID = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileIndex);
			writer.Write(PathID);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is PPtrLong<T> ptr)
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

		public static bool operator ==(PPtrLong<T> left, PPtrLong<T> right)
		{
			return left.FileIndex == right.FileIndex && left.PathID == right.PathID;
		}

		public static bool operator !=(PPtrLong<T> left, PPtrLong<T> right)
		{
			return left.FileIndex != right.FileIndex || left.PathID != right.PathID;
		}

	}
}
