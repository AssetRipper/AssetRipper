using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.SpriteAtlases
{
	public struct SpriteAtlasData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadAtlasRectOffset(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			AlphaTexture.Read(reader);
			TextureRect.Read(reader);
			TextureRectOffset.Read(reader);
			if(IsReadAtlasRectOffset(reader.Version))
			{
				AtlasRectOffset.Read(reader);
			}
			UVTransform.Read(reader);
			DownscaleMultiplier = reader.ReadSingle();
			SettingsRaw = reader.ReadUInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Texture.FetchDependency(file, isLog, () => nameof(SpriteAtlasData), TextureName);
			yield return AlphaTexture.FetchDependency(file, isLog, () => nameof(SpriteAtlasData), AlphaTextureName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(AlphaTextureName, AlphaTexture.ExportYAML(container));
			node.Add(TextureRectName, TextureRect.ExportYAML(container));
			node.Add(TextureRectOffsetName, TextureRectOffset.ExportYAML(container));
			node.Add(AtlasRectOffsetName, AtlasRectOffset.ExportYAML(container));
			node.Add(UVTransformName, UVTransform.ExportYAML(container));
			node.Add(DownscaleMultiplierName, DownscaleMultiplier);
			node.Add(SettingsRawName, SettingsRaw);
			return node;
		}

		public bool IsPacked => (SettingsRaw & 1) != 0;
		public SpritePackingMode PackingMode => (SpritePackingMode)((SettingsRaw >> 1) & 1);
		public SpritePackingRotation PackingRotation => (SpritePackingRotation)((SettingsRaw >> 2) & 0xF);
		public SpriteMeshType MeshType => (SpriteMeshType)((SettingsRaw >> 6) & 0x1);

		public float DownscaleMultiplier { get; private set; }
		public uint SettingsRaw { get; private set; }

		public const string TextureName = "texture";
		public const string AlphaTextureName = "alphaTexture";
		public const string TextureRectName = "textureRect";
		public const string TextureRectOffsetName = "textureRectOffset";
		public const string AtlasRectOffsetName = "atlasRectOffset";
		public const string UVTransformName = "uvTransform";
		public const string DownscaleMultiplierName = "downscaleMultiplier";
		public const string SettingsRawName = "settingsRaw";

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> AlphaTexture;
		public Rectf TextureRect;
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;
	}
}
