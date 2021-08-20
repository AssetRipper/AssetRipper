using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class BlendShapeData
    {
        public BlendShapeVertex[] vertices;
        public MeshBlendShape[] shapes;
        public MeshBlendShapeChannel[] channels;
        public float[] fullWeights;

        public BlendShapeData(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numVerts = reader.ReadInt32();
                vertices = new BlendShapeVertex[numVerts];
                for (int i = 0; i < numVerts; i++)
                {
                    vertices[i] = new BlendShapeVertex(reader);
                }

                int numShapes = reader.ReadInt32();
                shapes = new MeshBlendShape[numShapes];
                for (int i = 0; i < numShapes; i++)
                {
                    shapes[i] = new MeshBlendShape(reader);
                }

                int numChannels = reader.ReadInt32();
                channels = new MeshBlendShapeChannel[numChannels];
                for (int i = 0; i < numChannels; i++)
                {
                    channels[i] = new MeshBlendShapeChannel(reader);
                }

                fullWeights = reader.ReadSingleArray();
            }
            else
            {
                var m_ShapesSize = reader.ReadInt32();
                var m_Shapes = new MeshBlendShape[m_ShapesSize];
                for (int i = 0; i < m_ShapesSize; i++)
                {
                    m_Shapes[i] = new MeshBlendShape(reader);
                }
                reader.AlignStream();
                var m_ShapeVerticesSize = reader.ReadInt32();
                var m_ShapeVertices = new BlendShapeVertex[m_ShapeVerticesSize]; //MeshBlendShapeVertex
                for (int i = 0; i < m_ShapeVerticesSize; i++)
                {
                    m_ShapeVertices[i] = new BlendShapeVertex(reader);
                }
            }
        }
    }
}
