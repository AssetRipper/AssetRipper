using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Text;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class SerializablePrimitive : SerializableEntry
{
	public SerializablePrimitive(PrimitiveType type)
	{
		Type = type;
	}

	public PrimitiveType Type { get; }
	public override JsonNode? Read(ref EndianSpanReader reader)
	{
		JsonValue? result = Type switch
		{
			PrimitiveType.U1 => JsonValue.Create(reader.ReadByte()),
			PrimitiveType.U2 => JsonValue.Create(reader.ReadUInt16()),
			PrimitiveType.U4 => JsonValue.Create(reader.ReadUInt32()),
			PrimitiveType.U8 => JsonValue.Create(reader.ReadUInt64()),
			PrimitiveType.I1 => JsonValue.Create(reader.ReadSByte()),
			PrimitiveType.I2 => JsonValue.Create(reader.ReadInt16()),
			PrimitiveType.I4 => JsonValue.Create(reader.ReadInt32()),
			PrimitiveType.I8 => JsonValue.Create(reader.ReadInt64()),
			PrimitiveType.Float => JsonValue.Create(Clamp(reader.ReadSingle())),
			PrimitiveType.Double => JsonValue.Create(Clamp(reader.ReadDouble())),
			PrimitiveType.Boolean => JsonValue.Create(reader.ReadBoolean()),
			PrimitiveType.Character => JsonValue.Create(reader.ReadChar()),
			PrimitiveType.String => JsonValue.Create(ReadString(ref reader)),
			_ => throw new NotSupportedException()
		};
		MaybeAlign(ref reader);
		return result;
	}

	private static string ReadString(ref EndianSpanReader reader)
	{
		int size = reader.ReadInt32();
		byte[] data = reader.ReadBytes(size);
		if (data.Length != size)
		{
			throw new EndOfStreamException();
		}
		return Encoding.UTF8.GetString(data);
	}

	public static bool TryMakeFromTypeTreeNodes(List<TypeTreeNode> list, ref int index, [NotNullWhen(true)] out SerializablePrimitive? primitive)
	{
		TypeTreeNode node = list[index];
		string typeName = node.Type;
		if (typeName == "string")
		{
			TransferMetaFlags metaFlags = list[index].MetaFlag;
			index++;
			ThrowIfIncorrectName(list[index], "Array");
			index++;
			ThrowIfIncorrectName(list[index], "size");
			index++;
			ThrowIfIncorrectName(list[index], "data");
			index++;
			primitive = new SerializablePrimitive(PrimitiveType.String);
			primitive.Align = metaFlags.IsAlignBytes() || metaFlags.IsAnyChildUsesAlignBytes();
			return true;
		}

		primitive = typeName switch
		{
			"bool" => new SerializablePrimitive(PrimitiveType.Boolean),
			"char" => node.ByteSize == 2
				? new SerializablePrimitive(PrimitiveType.Character) //I don't think this can happen, but just to be safe.
				: new SerializablePrimitive(PrimitiveType.U1),
			"SInt8" => new SerializablePrimitive(PrimitiveType.I1),
			"UInt8" => new SerializablePrimitive(PrimitiveType.U1),
			"short" or "SInt16" => new SerializablePrimitive(PrimitiveType.I2),
			"ushort" or "UInt16" or "unsigned short" => node.MetaFlag.IsCharPropertyMask()
				? new SerializablePrimitive(PrimitiveType.Character)
				: new SerializablePrimitive(PrimitiveType.U2),
			"int" or "SInt32" or "Type*" => new SerializablePrimitive(PrimitiveType.I4),
			"uint" or "UInt32" or "unsigned int" => new SerializablePrimitive(PrimitiveType.U4),
			"SInt64" or "long long" => new SerializablePrimitive(PrimitiveType.I8),
			"UInt64" or "FileSize" or "unsigned long long" => new SerializablePrimitive(PrimitiveType.U8),
			"float" => new SerializablePrimitive(PrimitiveType.Float),
			"double" => new SerializablePrimitive(PrimitiveType.Double),
			_ => null,
		};
		if (primitive is not null)
		{
			index++;
			return true;
		}
		else
		{
			return false;
		}
	}

	private static float Clamp(float value)
	{
		if (float.IsNaN(value))
		{
			return 0;
		}
		else if (float.IsPositiveInfinity(value))
		{
			return float.MaxValue;
		}
		else if (float.IsNegativeInfinity(value))
		{
			return float.MinValue;
		}
		else
		{
			return value;
		}
	}

	private static double Clamp(double value)
	{
		if (double.IsNaN(value))
		{
			return 0;
		}
		else if (double.IsPositiveInfinity(value))
		{
			return double.MaxValue;
		}
		else if (double.IsNegativeInfinity(value))
		{
			return double.MinValue;
		}
		else
		{
			return value;
		}
	}
}
