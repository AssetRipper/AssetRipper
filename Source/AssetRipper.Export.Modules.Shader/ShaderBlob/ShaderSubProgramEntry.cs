using AssetRipper.Assets.IO.Writing;
using AssetRipper.Primitives;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob;

public sealed record class ShaderSubProgramEntry
{
	/// <summary>
	/// 2019.3 and greater
	/// </summary>
	public static bool HasSegment(UnityVersion version) => version.GreaterThanOrEquals(2019, 3);

	public void Read(AssetReader reader)
	{
		Offset = reader.ReadInt32();
		Length = reader.ReadInt32();
		if (HasSegment(reader.AssetCollection.Version))
		{
			Segment = reader.ReadInt32();
		}
	}

	public void Write(AssetWriter writer)
	{
		writer.Write(Offset);
		writer.Write(Length);
		if (HasSegment(writer.AssetCollection.Version))
		{
			writer.Write(Segment);
		}
	}

	public int Offset { get; set; }
	public int Length { get; set; }
	public int Segment { get; set; }
}
