using System;
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
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasVertexData(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasBindpose(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.1
		/// </summary>
		public static bool HasSourceSkin(Version version) => version.IsEqual(2018, 1);
		/// <summary>
		/// 5.4.x-5.6.x and greater
		/// </summary>
		public static bool HasAtlasRectOffset(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 10))
			{
				return true;
			}
			if (version.IsGreaterEqual(5, 4, 5, VersionType.Patch, 1))
			{
				if (version.IsGreaterEqual(5, 5, 2, VersionType.Patch))
				{
					return version.IsEqual(5, 5);
				}
				return version.IsEqual(5, 4);
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
			if (HasVertexData(version))
			{
				if (SubMeshes.Length == 0)
				{
					return Array.Empty<Vector2f[]>();
				}
				else
				{
					List<Vector2f[]> outlines = new List<Vector2f[]>();
					for (int i = 0; i < SubMeshes.Length; i++)
					{
						Vector3f[] vertices = VertexData.GenerateVertices(version, ref SubMeshes[i]);
						VertexDataToOutline(outlines, vertices, ref SubMeshes[i]);
					}
					return outlines.ToArray();
				}
			}
			else
			{
				if (Vertices.Length == 0)
				{
					return Array.Empty<Vector2f[]>();
				}
				else
				{
					return VerticesToOutline().ToArray();
				}
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

			if (HasVertexData(reader.Version))
			{
				SubMeshes = reader.ReadAssetArray<SubMesh>();
				IndexBuffer = reader.ReadByteArray();
				reader.AlignStream();

				VertexData.Read(reader);
			}
			else
			{
				Vertices = reader.ReadAssetArray<SpriteVertex>();
				Indices = reader.ReadUInt16Array();
				reader.AlignStream();
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

		private List<Vector2f[]> VerticesToOutline()
		{
			Vector3f[] vertices = new Vector3f[Vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = Vertices[i].Position;
			}

			Vector3i[] triangles = new Vector3i[Indices.Length / 3];
			for (int i = 0, j = 0; i < triangles.Length; i++)
			{
				int x = Indices[j++];
				int y = Indices[j++];
				int z = Indices[j++];
				triangles[i] = new Vector3i(x, y, z);
			}

			MeshOutlineGenerator outlineGenerator = new MeshOutlineGenerator(vertices, triangles);
			return outlineGenerator.GenerateOutlines();
		}

		private void VertexDataToOutline(List<Vector2f[]> outlines, Vector3f[] vertices, ref SubMesh submesh)
		{
			Vector3i[] triangles = new Vector3i[submesh.IndexCount / 3];
			for (int o = submesh.FirstByte, ti = 0; ti < triangles.Length; o += 6, ti++)
			{
				int x = BitConverter.ToUInt16(IndexBuffer, o + 0);
				int y = BitConverter.ToUInt16(IndexBuffer, o + 2);
				int z = BitConverter.ToUInt16(IndexBuffer, o + 4);
				triangles[ti] = new Vector3i(x, y, z);
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
		/// <summary>
		/// Actual sprite rectangle inside atlas texture (or in original texture for non atlas sprite)
		/// It is a retangle of cropped image if tight mode is used. Otherwise, its size matches the original size
		/// </summary>
		public Rectf TextureRect;
		/// <summary>
		/// Offset of actual (cropped) sprite rectangle relative to Sprite.Rect .
		/// Unity crops rectangle to save atlas space if tight mode is used. So final atlas image is a cropped version
		/// of a rectangle, developer specified in original texture.
		/// In other words, this value show how much Unity cropped the Sprite.Rect from bottom-left corner
		/// </summary>
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;
	}
}
