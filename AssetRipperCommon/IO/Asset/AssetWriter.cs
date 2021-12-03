using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssetRipper.Core.IO.Asset
{
	public sealed class AssetWriter : EndianWriter
	{
		private static MethodInfo WritePrimitiveInfo = typeof(AssetWriter).GetMethod(nameof(WritePrimitive));
		private static MethodInfo WriteAssetInfo = typeof(AssetWriter).GetMethod(nameof(WriteAsset));
		private static MethodInfo WriteKeyValuePairInfo = typeof(AssetWriter).GetMethod(nameof(WriteKeyValuePair));
		private static MethodInfo WriteDictionaryInfo = typeof(AssetWriter).GetMethod(nameof(WriteDictionary));
		private static MethodInfo WriteGenericArrayInfo = typeof(AssetWriter).GetMethods().Single(m => m.Name == nameof(WriteArray) && m.ContainsGenericParameters);

		public AssetWriter(Stream stream, EndianType endian, LayoutInfo info) : base(stream, endian, info.IsAlignArrays)
		{
			Info = info;
			IsAlignString = info.IsAlign;
		}

		public override void Write(char value)
		{
			FillInnerBuffer(value);
			Write(m_buffer, 0, sizeof(char));
		}

		public override void Write(string value)
		{
			char[] valueArray = value.ToCharArray();
			int count = Encoding.UTF8.GetByteCount(valueArray, 0, valueArray.Length);
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			byte[] buffer = count <= m_buffer.Length ? m_buffer : new byte[count];
			int written = Encoding.UTF8.GetBytes(valueArray, 0, valueArray.Length, buffer, 0);
			if (written != count)
			{
				throw new Exception($"Written {written} but expected {count}");
			}

			Write(buffer, 0, written);
			if (IsAlignString)
			{
				AlignStream();
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(char);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(char), index++)
				{
					FillInnerBuffer(buffer[index], i);
				}

				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WritePrimitive<T>(T value) where T : IConvertible
		{
			//Due to generic optimisations, this method should be reduced down to be very simple.
			//All the Convert.ToBlah operations should be trivial as T will be the actual type being converted to.
			if (value is bool bo)
				Write(bo);
			else if (value is byte b)
				Write(b);
			else if (value is sbyte sb)
				Write(sb);
			else if (value is char c)
				Write(c);
			else if (value is short sh)
				Write(sh);
			else if (value is int i)
				Write(i);
			else if (value is long lo)
				Write(lo);
			else if (value is ushort ush)
				Write(ush);
			else if (value is uint ui)
				Write(ui);
			else if (value is ulong ulo)
				Write(ulo);
			else if (value is float f)
				Write(f);
			else if (value is double d)
				Write(d);
			else if (value is string s)
				Write(s);
			else
				throw new ArgumentException($"Cannot write a primitive of type {typeof(T)}", nameof(value));
		}

		public void WriteAsset<T>(T value) where T : IAssetWritable
		{
			value.Write(this);
		}

		public void WriteAssetArray<T>(T[] buffer) where T : IAssetWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(this);
			}

			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteAssetArray<T>(T[][] buffer) where T : IAssetWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.GetLength(0); i++)
			{
				WriteAssetArray(buffer[i]);
			}

			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray<T>(T[] value)
		{
			var writer = GetWriter(typeof(T));

			FillInnerBuffer(value.Length);
			Write(m_buffer, 0, sizeof(int));

			foreach (T nestedValue in value)
			{
				writer.Invoke(this, new object[] { nestedValue });
			}
		}

		private static (MethodInfo keyWriter, MethodInfo valueWriter) GetKeyAndValueWriter<TKey, TValue>()
		{
			MethodInfo valueWriter = GetWriter<TValue>();
			MethodInfo keyWriter = GetWriter<TKey>();

			return (keyWriter, valueWriter);
		}

		private static MethodInfo GetWriter<T>() => GetWriter(typeof(T));

		private static MethodInfo GetWriter(Type type)
		{
			if (type.IsAssignableTo(typeof(IConvertible)))
				return WritePrimitiveInfo.MakeGenericMethod(type);
			if (type.IsAssignableTo(typeof(IAssetWritable)))
				return WriteAssetInfo.MakeGenericMethod(type);
			if (type.IsArray)
				return WriteGenericArrayInfo.MakeGenericMethod(type.GetElementType());
			if (type.FullName!.StartsWith("System.Collections.Generic.KeyValuePair"))
				return WriteKeyValuePairInfo.MakeGenericMethod(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
			if (type.FullName!.StartsWith("System.Collections.Generic.Dictionary"))
				return WriteDictionaryInfo.MakeGenericMethod(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

			throw new ArgumentException($"Generic Parameter must implement either IConvertible or IAssetWritable, or be an Array, KeyValuePair, or Dictionary for which the parameters also follow this rule. {type} does not.", nameof(type));
		}

		public void WriteKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		{
			(MethodInfo keyWriter, MethodInfo valueWriter) = GetKeyAndValueWriter<TKey, TValue>();

			keyWriter.Invoke(this, new object[] { pair.Key });
			valueWriter.Invoke(this, new object[] { pair.Value });
		}

		public void WriteDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
		{
			(MethodInfo keyWriter, MethodInfo valueWriter) = GetKeyAndValueWriter<TKey, TValue>();

			FillInnerBuffer(dict.Count);
			Write(m_buffer, 0, sizeof(int));

			foreach (var (key, value) in dict)
			{
				keyWriter.Invoke(this, new object[] { key });
				valueWriter.Invoke(this, new object[] { value });
			}
		}

		public void WriteGeneric<T>(T value)
		{
			var writer = GetWriter<T>();
			writer.Invoke(this, new object[] { value });
		}

		public LayoutInfo Info { get; }
		public UnityVersion Version => Info.Version;
		public Platform Platform => Info.Platform;
		public TransferInstructionFlags Flags => Info.Flags;

		private bool IsAlignString { get; }
	}
}