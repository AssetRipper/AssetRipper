using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IBlendShapeData : IAsset
	{
		IBlendShapeVertex[] Vertices { get; }
		IMeshBlendShape[] Shapes { get; }
		IMeshBlendShapeChannel[] Channels { get; }
		float[] FullWeights { get; set; }
	}

	public static class BlendShapeDataExtensions
	{
		public static string FindShapeNameByCRC(this IBlendShapeData blendShapeData, uint crc)
		{
			foreach (IMeshBlendShapeChannel blendChannel in blendShapeData.Channels)
			{
				if (blendChannel.NameHash == crc)
				{
					return blendChannel.Name.String;
				}
			}
			return null;
		}
	}
}
