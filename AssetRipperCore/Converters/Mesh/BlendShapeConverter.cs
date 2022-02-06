using AssetRipper.Core.Classes.Mesh;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class BlendShapeConverter
	{
		public static IMeshBlendShapeChannel[] GenerateBlendChannels(IMeshBlendShape[] shapes)
		{
			IMeshBlendShapeChannel[] channels = new IMeshBlendShapeChannel[shapes.Length];
			for (int i = 0; i < shapes.Length; i++)
			{
				channels[i] = new MeshBlendShapeChannel(shapes[i].Name.String, i, 1);
			}
			return channels;
		}

		public static float[] GenerateFullWeights(IMeshBlendShape[] shapes)
		{
			float[] fullWeights = new float[shapes.Length];
			for (int i = 0; i < shapes.Length; i++)
			{
				fullWeights[i] = 100.0f;
			}
			return fullWeights;
		}
	}
}
