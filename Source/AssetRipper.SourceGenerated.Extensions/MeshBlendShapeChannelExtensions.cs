using AssetRipper.Assets.Utils;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShapeChannel;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MeshBlendShapeChannelExtensions
	{
		public static void SetValues(this IMeshBlendShapeChannel channel, string name, int frameIndex, int frameCount)
		{
			channel.Name = name;
			channel.NameHash = CrcUtils.CalculateDigestUTF8(name);
			channel.FrameIndex = frameIndex;
			channel.FrameCount = frameCount;
		}
	}
}
