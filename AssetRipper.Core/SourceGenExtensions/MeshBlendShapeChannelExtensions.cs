using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShapeChannel;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MeshBlendShapeChannelExtensions
	{
		public static void SetValues(this IMeshBlendShapeChannel channel, string name, int frameIndex, int frameCount)
		{
			channel.Name.String = name;
			channel.NameHash = CrcUtils.CalculateDigestUTF8(name);
			channel.FrameIndex = frameIndex;
			channel.FrameCount = frameCount;
		}
	}
}
