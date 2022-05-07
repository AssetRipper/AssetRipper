﻿using AssetRipper.Core.Layout;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System;
using System.Text;

namespace AssetRipper.Core.Classes
{
	public abstract class Utf8StringBase : UnityAssetBase, IEquatable<Utf8StringBase>, IEquatable<string>
	{
		public Utf8StringBase() : base() { }
		public Utf8StringBase(LayoutInfo layout) : base(layout) { }

		public abstract byte[]? Data { get; set; }

		public string String
		{
			get => Data == null ? "" : Encoding.UTF8.GetString(Data);
			set => Data = value == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value);
		}

		public static bool operator ==(Utf8StringBase? utf8String, string? str) => utf8String?.String == str;
		public static bool operator !=(Utf8StringBase? utf8String, string? str) => utf8String?.String != str;
		public static bool operator ==(string? str, Utf8StringBase? utf8String) => utf8String?.String == str;
		public static bool operator !=(string? str, Utf8StringBase? utf8String) => utf8String?.String != str;
		public static bool operator ==(Utf8StringBase? str1, Utf8StringBase? str2)
		{
			if (str1 is null || str2 is null)
			{
				if (str1 is null && str2 is null)
					return true;
				else
					return false;
			}
			
			if (str1.Data is null || str2.Data is null)
			{
				if (str1.Data is null && str2.Data is null)
					return false;
				else
					return false;
			}

			if(str1.Data.Length != str2.Data.Length)
				return false;

			for(int i = 0; i < str1.Data.Length; i++)
			{
				if (str1.Data[i] != str2.Data[i])
				{
					return false;
				}
			}

			return true;
		}
		public static bool operator !=(Utf8StringBase? str1, Utf8StringBase? str2) => !(str1 == str2);

		public bool Equals(Utf8StringBase? other) => this == other;

		public bool Equals(string? other) => String.Equals(other);

		public override YamlNode ExportYamlEditor(IExportContainer container)
		{
			return new YamlScalarNode(String);
		}

		public override YamlNode ExportYamlRelease(IExportContainer container)
		{
			return new YamlScalarNode(String);
		}

		public bool CopyIfNullOrEmpty(Utf8StringBase? other)
		{
			if(Data is null || Data.Length == 0)
			{
				Data = CopyData(other?.Data);
				return true;
			}
			return false;
		}

		private static byte[] CopyData(byte[]? source)
		{
			int length = source?.Length ?? 0;
			if (length == 0)
			{
				return Array.Empty<byte>();
			}
			else
			{
				byte[] destination = new byte[length];
				Array.Copy(source!, destination, length);
				return destination;
			}
		}

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;
			else if (obj is Utf8StringBase utf8String)
				return Equals(utf8String);
			else if (obj is string str)
				return Equals(str);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Data);
		}

		public override string ToString()
		{
			return String;
		}
	}

	public static class Utf8StringBaseExtensions
	{
		public static string[] ToStringArray(this Utf8StringBase[] utf8Strings)
		{
			string[] result = new string[utf8Strings.Length];
			for(int i = 0; i < utf8Strings.Length; i++)
			{
				result[i] = utf8Strings[i].String;
			}
			return result;
		}
	}
}
