using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Utils;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IMeshBlendShapeChannel : IAsset
	{
		Utf8StringBase Name { get; }
		uint NameHash { get; set; }
		int FrameIndex { get; set; }
		int FrameCount { get; set; }
	}

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
