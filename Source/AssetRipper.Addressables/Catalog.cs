using System.Buffers.Binary;
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
	/// <summary>
	/// Stores the id of the data provider.
	/// </summary>
	[JsonPropertyName("m_LocatorId")]
	public string? LocatorId { get; set; }

	/// <summary>
	/// Data for the Addressables.ResourceManager.InstanceProvider initialization;
	/// </summary>
	[JsonPropertyName("m_InstanceProviderData")]
	public DataObject? InstanceProviderData { get; set; }

	/// <summary>
	/// Data for the Addressables.ResourceManager.InstanceProvider initialization;
	/// </summary>
	[JsonPropertyName("m_SceneProviderData")]
	public DataObject? SceneProviderData { get; set; }

	/// <summary>
	/// The list of resource provider data.  Each entry will add an IResourceProvider to the Addressables.ResourceManager.ResourceProviders list.
	/// </summary>
	[JsonPropertyName("m_ResourceProviderData")]
	public List<DataObject>? ResourceProviderData { get; set; }

	/// <summary>
	/// The IDs for the Resource Providers.
	/// </summary>
	[JsonPropertyName("m_ProviderIds")]
	[FormerlySerializedAs("m_providerIds")]
	public List<string>? ProviderIds { get; set; }

	/// <summary>
	/// Internal Content Catalog Entry IDs for Addressable Assets.
	/// </summary>
	[JsonPropertyName("m_InternalIds")]
	[FormerlySerializedAs("m_internalIds")]
	public List<string>? InternalIds { get; set; }

	/// <summary>
	/// m_KeyDataString is all of your keys, Address, Label, GUIDs
	/// </summary>
	[JsonPropertyName("m_KeyDataString")]
	[FormerlySerializedAs("m_keyDataString")]
	public byte[]? KeyDataString { get; set; }

	/// <summary>
	/// m_BucketDataString The mappings between the keys and the entries
	/// </summary>
	[JsonPropertyName("m_BucketDataString")]
	[FormerlySerializedAs("m_bucketDataString")]
	public byte[]? BucketDataString { get; set; }

	/// <summary>
	/// m_EntryDataString is the Addressable entries. This contains indexes into the InternalID's for its load path and dependencies etc
	/// </summary>
	[JsonPropertyName("m_EntryDataString")]
	[FormerlySerializedAs("m_entryDataString")]
	public byte[]? EntryDataString { get; set; }

	/// <summary>
	/// m_ExtraDataString is extra data for the entries. This will likely be exclusively information for how to load the AssetBundle. CRC, Cache Hash etc
	/// </summary>
	[JsonPropertyName("m_ExtraDataString")]
	[FormerlySerializedAs("m_extraDataString")]
	public byte[]? ExtraDataString { get; set; }

	[JsonPropertyName("m_resourceTypes")]
	public List<FullyQualifiedName>? ResourceTypes { get; set; }

	[JsonPropertyName("m_InternalIdPrefixes")]
	public List<string>? InternalIdPrefixes { get; set; }

	public static Catalog? FromJsonFile(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return JsonSerializer.Deserialize(stream, CatalogSerializerContext.Default.Catalog);
	}

	[JsonIgnore]
	public IEnumerable<KeyValuePair<ObjectType, object>> Keys
	{
		get
		{
			if (KeyDataString is not null)
			{
				return DeserializeKeyData(KeyDataString);
			}
			else
			{
				return Enumerable.Empty<KeyValuePair<ObjectType, object>>();
			}

			static IEnumerable<KeyValuePair<ObjectType, object>> DeserializeKeyData(byte[] keyData)
			{
				int count = BinaryPrimitives.ReadInt32LittleEndian(keyData);
				int dataIndex = sizeof(int);
				for (int i = 0; i < count; i++)
				{
					dataIndex += Serialization.ReadObjectFromData(keyData, dataIndex, out ObjectType type, out object value);
					yield return new KeyValuePair<ObjectType, object>(type, value);
				}
			}
		}
	}

	[JsonIgnore]
	public IEnumerable<Bucket> Buckets
	{
		get
		{
			if (BucketDataString is not null)
			{
				return DeserializeBuckets(BucketDataString);
			}
			else
			{
				return Enumerable.Empty<Bucket>();
			}

			static IEnumerable<Bucket> DeserializeBuckets(byte[] data)
			{
				int bucketCount = BinaryPrimitives.ReadInt32LittleEndian(data);
				int offset = sizeof(int);
				for (int i = 0; i < bucketCount; i++)
				{
					offset += Bucket.Read(data.AsSpan(offset), out Bucket bucket);
					yield return bucket;
				}
			}
		}
	}

	[JsonIgnore]
	public IEnumerable<Entry> Entries
	{
		get
		{
			if (EntryDataString is not null && ExtraDataString is not null)
			{
				return DeserializeEntries(EntryDataString, ExtraDataString);
			}
			else
			{
				return Enumerable.Empty<Entry>();
			}

			static IEnumerable<Entry> DeserializeEntries(byte[] entryData, byte[] extraData)
			{
				int count = BinaryPrimitives.ReadInt32LittleEndian(entryData);
				int dataIndex = sizeof(int);
				for (int i = 0; i < count; i++)
				{
					Entry entry = Entry.Read(entryData.AsSpan(dataIndex, Entry.Size), extraData);
					dataIndex += Entry.Size;
					yield return entry;
				}
			}
		}
	}
}
