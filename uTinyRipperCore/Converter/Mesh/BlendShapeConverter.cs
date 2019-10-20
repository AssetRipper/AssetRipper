using uTinyRipper.Classes.Meshes;

namespace uTinyRipper.Converters.Meshes
{
	public static class BlendShapeConverter
	{
		public static BlendShapeChannel[] GenerateBlendChannels(BlendShape[] shapes)
		{
			BlendShapeChannel[] channels = new BlendShapeChannel[shapes.Length];
			for (int i = 0; i < shapes.Length; i++)
			{
				channels[i] = new BlendShapeChannel(shapes[i].Name, i, 1);
			}
			return channels;
		}

		public static float[] GenerateFullWeights(BlendShape[] shapes)
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
