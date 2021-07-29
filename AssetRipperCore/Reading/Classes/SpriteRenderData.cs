using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class SpriteRenderData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public SecondarySpriteTexture[] secondaryTextures;
        public SubMesh[] m_SubMeshes;
        public byte[] m_IndexBuffer;
        public VertexData m_VertexData;
        public SpriteVertex[] vertices;
        public ushort[] indices;
        public Matrix4x4f[] m_Bindpose;
        public BoneWeights4[] m_SourceSkin;
        public Rectf textureRect;
        public Vector2f textureRectOffset;
        public Vector2f atlasRectOffset;
        public SpriteSettings settingsRaw;
        public Vector4f uvTransform;
        public float downscaleMultiplier;

        public SpriteRenderData(ObjectReader reader)
        {
            var version = reader.version;

            texture = new PPtr<Texture2D>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                alphaTexture = new PPtr<Texture2D>(reader);
            }

            if (version[0] >= 2019) //2019 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures[i] = new SecondarySpriteTexture(reader);
                }
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var m_SubMeshesSize = reader.ReadInt32();
                m_SubMeshes = new SubMesh[m_SubMeshesSize];
                for (int i = 0; i < m_SubMeshesSize; i++)
                {
                    m_SubMeshes[i] = new SubMesh(reader);
                }

                m_IndexBuffer = reader.ReadUInt8Array();
                reader.AlignStream();

                m_VertexData = new VertexData(reader);
            }
            else
            {
                var verticesSize = reader.ReadInt32();
                vertices = new SpriteVertex[verticesSize];
                for (int i = 0; i < verticesSize; i++)
                {
                    vertices[i] = new SpriteVertex(reader);
                }

                indices = reader.ReadUInt16Array();
                reader.AlignStream();
            }

            if (version[0] >= 2018) //2018 and up
            {
                m_Bindpose = reader.ReadMatrixArray();

                if (version[0] == 2018 && version[1] < 2) //2018.2 down
                {
                    var m_SourceSkinSize = reader.ReadInt32();
                    for (int i = 0; i < m_SourceSkinSize; i++)
                    {
                        m_SourceSkin[i] = new BoneWeights4(reader);
                    }
                }
            }

            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2f();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                atlasRectOffset = reader.ReadVector2f();
            }

            settingsRaw = new SpriteSettings(reader);
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                uvTransform = reader.ReadVector4f();
            }

            if (version[0] >= 2017) //2017 and up
            {
                downscaleMultiplier = reader.ReadSingle();
            }
        }
    }
}
