using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Meshes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters.Sprites;

namespace uTinyRipper.Classes.Sprites
{
	public struct SpriteRenderData : IAssetReadable, IDependent
	{
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasAlphaTexture(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasSecondaryTextures(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool HasVertices(Version version) => version.IsLess(5, 6);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasBindpose(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.1
		/// </summary>
		public static bool HasSourceSkin(Version version) => version.IsEqual(2018, 1);
		/// <summary>
		/// 5.x.x (mess) and greater
		/// </summary>
		public static bool HasAtlasRectOffset(Version version)
		{
			if (version.IsGreaterEqual(5, 4, 5, VersionType.Patch, 1))
			{
				if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 10))
				{
					return true;
				}
				if (version.IsGreaterEqual(5, 5, 2, VersionType.Patch))
				{
					if (version.IsGreaterEqual(5, 6))
					{
						return false;
					}
					return true;
				}
				if (version.IsGreaterEqual(5, 5, 0, VersionType.Patch, 3))
				{
					if (version.IsGreaterEqual(5, 5, 1))
					{
						return false;
					}
					return true;
				}
				if (version.IsGreaterEqual(5, 5))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasUVTransform(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasDownscaleMultiplier(Version version) => version.IsGreaterEqual(2017);

		public Vector2f[][] GenerateOutline(Version version)
		{
			if (HasVertices(version))
			{
				Vector2f[][] outline = new Vector2f[1][];
				outline[0] = new Vector2f[Vertices.Length];
				for (int i = 0; i < Vertices.Length; i++)
				{
					outline[0][i] = Vertices[i].Position.ToVector2();
				}
				return outline;
			}
			else
			{
				List<Vector2f[]> outlines = new List<Vector2f[]>();
				for (int i = 0; i < SubMeshes.Length; i++)
				{
					Vector3f[] vertices = VertexData.GenerateVertices(version, ref SubMeshes[i]);
					VerticesToOutline(outlines, vertices, ref SubMeshes[i]);
				}
				return outlines.ToArray();
			}
		}

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (HasAlphaTexture(reader.Version))
			{
				AlphaTexture.Read(reader);
			}
			if (HasSecondaryTextures(reader.Version))
			{
				SecondaryTextures = reader.ReadAssetArray<SecondarySpriteTexture>();
			}

			if (HasVertices(reader.Version))
			{
				Vertices = reader.ReadAssetArray<SpriteVertex>();
				Indices = reader.ReadUInt16Array();
				reader.AlignStream();
			}
			else
			{
				SubMeshes = reader.ReadAssetArray<SubMesh>();
				IndexBuffer = reader.ReadByteArray();
				reader.AlignStream();

				VertexData.Read(reader);
			}
			if (HasBindpose(reader.Version))
			{
				Bindpose = reader.ReadAssetArray<Matrix4x4f>();
			}
			if (HasSourceSkin(reader.Version))
			{
				SourceSkin = reader.ReadAssetArray<BoneWeights4>();
			}

			TextureRect.Read(reader);
			TextureRectOffset.Read(reader);
			if (HasAtlasRectOffset(reader.Version))
			{
				AtlasRectOffset.Read(reader);
			}
			SettingsRaw = reader.ReadUInt32();
			if (HasUVTransform(reader.Version))
			{
				UVTransform.Read(reader);
			}
			if (HasDownscaleMultiplier(reader.Version))
			{
				DownscaleMultiplier = reader.ReadSingle();
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
			yield return context.FetchDependency(AlphaTexture, AlphaTextureName);

			if (HasSecondaryTextures(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(SecondaryTextures, SecondaryTexturesName))
				{
					yield return asset;
				}
			}
		}

		private void VerticesToOutline(List<Vector2f[]> outlines, Vector3f[] vertices, ref SubMesh submesh)
		{
			int triangleCount = submesh.IndexCount / 3;
			List<Vector3i> triangles = new List<Vector3i>(triangleCount);
			using (MemoryStream memStream = new MemoryStream(IndexBuffer))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					memStream.Position = submesh.FirstByte;
					for (int i = 0; i < triangleCount; i++)
					{
						int x = reader.ReadInt16();
						int y = reader.ReadInt16();
						int z = reader.ReadInt16();
						triangles.Add(new Vector3i(x, y, z));
					}
				}
			}
			MeshOutlineGenerator outlineGenerator = new MeshOutlineGenerator(vertices, triangles);
			List<Vector2f[]> meshOutlines = outlineGenerator.GenerateOutlines();
			outlines.AddRange(meshOutlines);
		}

		public bool IsPacked => (SettingsRaw & 1) != 0;
		public SpritePackingMode PackingMode => (SpritePackingMode)((SettingsRaw >> 1) & 1);
		public SpritePackingRotation PackingRotation => (SpritePackingRotation)((SettingsRaw >> 2) & 0xF);
		public SpriteMeshType MeshType => (SpriteMeshType)((SettingsRaw >> 6) & 0x1);

		public SecondarySpriteTexture[] SecondaryTextures { get; set; }
		public SpriteVertex[] Vertices { get; set; }
		public ushort[] Indices { get; set; }
		public SubMesh[] SubMeshes { get; set; }
		public byte[] IndexBuffer { get; set; }
		public Matrix4x4f[] Bindpose { get; set; }
		public BoneWeights4[] SourceSkin { get; set; }
		public uint SettingsRaw { get; set; }
		public float DownscaleMultiplier { get; set; }

		public const string TextureName = "texture";
		public const string AlphaTextureName = "alphaTexture";
		public const string SecondaryTexturesName = "secondaryTextures";

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> AlphaTexture;
		public VertexData VertexData;
		public Rectf TextureRect;
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;
	}
}
