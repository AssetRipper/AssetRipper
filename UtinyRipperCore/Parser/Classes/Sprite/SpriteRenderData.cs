using System.Collections.Generic;
using UtinyRipper.Classes.Meshes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.Sprites
{
	public struct SpriteRenderData : IAssetReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAlphaTexture(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadVertices(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool IsReadAtlasRectOffset(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadUVTransform(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadDownscaleMultiplier(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetStream stream)
		{
			Texture.Read(stream);
			if (IsReadAlphaTexture(stream.Version))
			{
				AlphaTexture.Read(stream);
			}

			if (IsReadVertices(stream.Version))
			{
				m_vertices = stream.ReadArray<SpriteVertex>();
				m_indices = stream.ReadUInt16Array();
				stream.AlignStream(AlignType.Align4);
			}
			else
			{
				m_subMeshes = stream.ReadArray<SubMesh>();
				m_indexBuffer = stream.ReadByteArray();
				stream.AlignStream(AlignType.Align4);

				VertexData.Read(stream);
			}

			TextureRect.Read(stream);
			TextureRectOffset.Read(stream);
			if(IsReadAtlasRectOffset(stream.Version))
			{
				AtlasRectOffset.Read(stream);
			}
			SettingsRaw = stream.ReadUInt32();
			if(IsReadUVTransform(stream.Version))
			{
				UVTransform.Read(stream);
			}
			if(IsReadDownscaleMultiplier(stream.Version))
			{
				DownscaleMultiplier = stream.ReadSingle();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			Texture2D texture = Texture.FindObject(file);
			if (texture == null)
			{
				if (isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"SpiteAtlasData's Texture {Texture.ToLogString(file)} wasn't found");
				}
			}
			else
			{
				yield return texture;
			}

			if (!AlphaTexture.IsNull)
			{
				texture = AlphaTexture.FindObject(file);
				if (texture == null)
				{
					if (isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"SpiteAtlasData's AlphaTexture {AlphaTexture.ToLogString(file)} wasn't found");
					}
				}
				else
				{
					yield return texture;
				}
			}
		}

		public IReadOnlyList<SpriteVertex> Vertices => m_vertices;
		public IReadOnlyList<ushort> Indices => m_indices;
		public IReadOnlyList<SubMesh> SubMeshes => m_subMeshes;
		public IReadOnlyList<byte> IndexBuffer => m_indexBuffer;
		public uint SettingsRaw { get; private set; }
		public float DownscaleMultiplier { get; private set; }

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> AlphaTexture;
		public VertexData VertexData;
		public Rectf TextureRect;
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;

		private SpriteVertex[] m_vertices;
		private ushort[] m_indices;
		private SubMesh[] m_subMeshes;
		private byte[] m_indexBuffer;
	}
}
