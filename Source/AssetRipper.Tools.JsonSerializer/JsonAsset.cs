using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class JsonAsset : UnityObjectBase
{
	public JsonNode? Contents { get; private set; }

	public JsonAsset(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public void Read(ref EndianSpanReader reader, SerializableEntry serializableType)
	{
		Contents = serializableType.Read(ref reader);
	}
}
