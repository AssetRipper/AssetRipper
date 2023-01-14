using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

/// <summary>
/// An Addressable catalog from Json
/// </summary>
/// <remarks>
/// The readable parts can be changed freely, as long as the order is not changed.
/// The binary parts will be able to index into the internalID's correctly.
/// The binary parts are little-endian.
/// </remarks>
public sealed record class Catalog
{
	[JsonPropertyName("m_LocatorId")]
	public string? LocatorId { get; set; }

	[JsonPropertyName("m_InstanceProviderData")]
	public DataObject? InstanceProviderData { get; set; }

	[JsonPropertyName("m_SceneProviderData")]
	public DataObject? SceneProviderData { get; set; }

	[JsonPropertyName("m_ResourceProviderData")]
	public List<DataObject>? ResourceProviderData { get; set; }

	[JsonPropertyName("m_ProviderIds")]
	public List<string>? ProviderIds { get; set; }

	[JsonPropertyName("m_InternalIds")]
	public List<string>? InternalIds { get; set; }

	/// <summary>
	/// m_KeyDataString is all of your keys, Address, Label, GUIDs
	/// </summary>
	[JsonPropertyName("m_KeyDataString")]
	public byte[]? KeyDataString { get; set; }

	/// <summary>
	/// m_BucketDataString The mappings between the keys and the entries
	/// </summary>
	[JsonPropertyName("m_BucketDataString")]
	public byte[]? BucketDataString { get; set; }

	/// <summary>
	/// m_EntryDataString is the Addressable entries. This contains indexes into the InternalID's for its load path and dependencies etc
	/// </summary>
	[JsonPropertyName("m_EntryDataString")]
	public byte[]? EntryDataString { get; set; }

	/// <summary>
	/// m_ExtraDataString is extra data for the entries. This will likely be exclusively information for how to load the AssetBundle. CRC, Cache Hash etc
	/// </summary>
	[JsonPropertyName("m_ExtraDataString")]
	public byte[]? ExtraDataString { get; set; }

	[JsonPropertyName("m_Keys")]
	public List<string>? Keys { get; set; }

	[JsonPropertyName("m_resourceTypes")]
	public List<FullyQualifiedName>? ResourceTypes { get; set; }

	public static Catalog? FromJsonFile(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return JsonSerializer.Deserialize(stream, CatalogSerializerContext.Default.Catalog);
	}

	public IEnumerable<KeyValuePair<ObjectType, object>> DeserializeKeyDataString()
	{
		if (KeyDataString is not null)
		{
			return DeserializeKeyData(KeyDataString);
		}
		else
		{
			return Enumerable.Empty<KeyValuePair<ObjectType, object>>();
		}
	}

	private static IEnumerable<KeyValuePair<ObjectType, object>> DeserializeKeyData(byte[] keyData)
	{
		int dataIndex = 4;
		while (dataIndex < keyData.Length)
		{
			ObjectType keyType = (ObjectType)keyData[dataIndex];
			dataIndex++;
			switch (keyType)
			{
				case ObjectType.UnicodeString:
					{
						int dataLength = ReadInt32(keyData, dataIndex);
						dataIndex += sizeof(int);
						string str = Encoding.Unicode.GetString(keyData, dataIndex, dataLength);
						dataIndex += dataLength;
						yield return new KeyValuePair<ObjectType, object>(keyType, str);
					}
					break;
				case ObjectType.AsciiString:
					{
						int dataLength = ReadInt32(keyData, dataIndex);
						dataIndex += sizeof(int);
						string str = Encoding.ASCII.GetString(keyData, dataIndex, dataLength);
						dataIndex += dataLength;
						yield return new KeyValuePair<ObjectType, object>(keyType, str);
					}
					break;
				case ObjectType.UInt16:
					{
						ushort value = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(keyData, dataIndex, sizeof(ushort)));
						dataIndex += sizeof(ushort);
						yield return new KeyValuePair<ObjectType, object>(keyType, value);
					}
					break;
				case ObjectType.UInt32:
					{
						uint value = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(keyData, dataIndex, sizeof(uint)));
						dataIndex += sizeof(uint);
						yield return new KeyValuePair<ObjectType, object>(keyType, value);
					}
					break;
				case ObjectType.Int32:
					{
						int value = ReadInt32(keyData, dataIndex);
						dataIndex += sizeof(int);
						yield return new KeyValuePair<ObjectType, object>(keyType, value);
					}
					break;
				case ObjectType.Hash128:
					{
						byte count = keyData[dataIndex];
						dataIndex++;
						string str = Encoding.ASCII.GetString(keyData, dataIndex, count);
						dataIndex += count;
						//return Hash128.Parse(str);
						yield return new KeyValuePair<ObjectType, object>(keyType, str);
					}
					break;
				case ObjectType.Type:
					{
						byte count = keyData[dataIndex];
						dataIndex++;
						string str = Encoding.ASCII.GetString(keyData, dataIndex, count);
						dataIndex += count;
						//return Type.GetTypeFromCLSID(new Guid(str));
						yield return new KeyValuePair<ObjectType, object>(keyType, str);
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
						yield return new KeyValuePair<ObjectType, object>(keyType, new BinaryJsonObject(assemblyName, className, jsonText));
					}
					break;
				default:
					throw new NotSupportedException();
			}
		}

		static int ReadInt32(byte[] data, int offset)
		{
			return BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(data, offset, sizeof(int)));
		}
	}
}
