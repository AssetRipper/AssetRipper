using System.Buffers.Binary;
using System.Text;

namespace AssetRipper.Addressables;

internal static class Serialization
{
	internal static int ReadObjectFromData(byte[] keyData, int offset, out ObjectType type, out object value)
	{
		int dataIndex = offset;
		type = (ObjectType)keyData[dataIndex];
		dataIndex++;
		switch (type)
		{
			case ObjectType.UnicodeString:
				{
					int dataLength = ReadInt32(keyData, dataIndex);
					dataIndex += sizeof(int);
					value = Encoding.Unicode.GetString(keyData, dataIndex, dataLength);
					dataIndex += dataLength;
				}
				break;
			case ObjectType.AsciiString:
				{
					int dataLength = ReadInt32(keyData, dataIndex);
					dataIndex += sizeof(int);
					value = Encoding.ASCII.GetString(keyData, dataIndex, dataLength);
					dataIndex += dataLength;
				}
				break;
			case ObjectType.UInt16:
				{
					value = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(keyData, dataIndex, sizeof(ushort)));
					dataIndex += sizeof(ushort);
				}
				break;
			case ObjectType.UInt32:
				{
					value = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(keyData, dataIndex, sizeof(uint)));
					dataIndex += sizeof(uint);
				}
				break;
			case ObjectType.Int32:
				{
					value = ReadInt32(keyData, dataIndex);
					dataIndex += sizeof(int);
				}
				break;
			case ObjectType.Hash128:
				{
					byte count = keyData[dataIndex];
					dataIndex++;
					value = Encoding.ASCII.GetString(keyData, dataIndex, count);
					dataIndex += count;
					//return Hash128.Parse(value);
				}
				break;
			case ObjectType.Type:
				{
					byte count = keyData[dataIndex];
					dataIndex++;
					value = Encoding.ASCII.GetString(keyData, dataIndex, count);
					dataIndex += count;
					//return Type.GetTypeFromCLSID(new Guid(value));
				}
				break;
			case ObjectType.JsonObject:
				{
					int assemblyNameLength = keyData[dataIndex];
					dataIndex++;
					string assemblyName = Encoding.ASCII.GetString(keyData, dataIndex, assemblyNameLength);
					dataIndex += assemblyNameLength;

					int classNameLength = keyData[dataIndex];
					dataIndex++;
					string className = Encoding.ASCII.GetString(keyData, dataIndex, classNameLength);
					dataIndex += classNameLength;
					int jsonLength = ReadInt32(keyData, dataIndex);
					dataIndex += sizeof(int);
					string jsonText = Encoding.Unicode.GetString(keyData, dataIndex, jsonLength);
					//var assembly = Assembly.Load(assemblyName);
					//var t = assembly.GetType(className);
					//return JsonUtility.FromJson(jsonText, t);
					dataIndex += jsonLength;
					value = new BinaryJsonObject(assemblyName, className, jsonText);
				}
				break;
			default:
				throw new NotSupportedException();
		}

		return dataIndex - offset;

		static int ReadInt32(byte[] data, int offset)
		{
			return BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(data, offset, sizeof(int)));
		}
	}
}
