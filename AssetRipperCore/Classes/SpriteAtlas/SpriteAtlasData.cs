using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.SpriteAtlas
{
	public sealed class SpriteAtlasData : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasAtlasRectOffset(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasSecondaryTextures(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			AlphaTexture.Read(reader);
			TextureRect.Read(reader);
			TextureRectOffset.Read(reader);
			if (HasAtlasRectOffset(reader.Version))
			{
				AtlasRectOffset.Read(reader);
			}
			UVTransform.Read(reader);
			DownscaleMultiplier = reader.ReadSingle();
			SettingsRaw = reader.ReadUInt32();
			if (HasSecondaryTextures(reader.Version))
			{
				SecondaryTextures = reader.ReadAssetArray<SecondarySpriteTexture>();
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
			yield return context.FetchDependency(AlphaTexture, AlphaTextureName);
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

		public float DownscaleMultiplier { get; set; }
		public uint SettingsRaw { get; set; }
		public SecondarySpriteTexture[] SecondaryTextures { get; set; }

		public const string TextureName = "texture";
		public const string AlphaTextureName = "alphaTexture";
		public const string TextureRectName = "textureRect";
		public const string TextureRectOffsetName = "textureRectOffset";
		public const string AtlasRectOffsetName = "atlasRectOffset";
		public const string UVTransformName = "uvTransform";
		public const string DownscaleMultiplierName = "downscaleMultiplier";
		public const string SettingsRawName = "settingsRaw";
		public const string SecondaryTexturesName = "secondaryTextures";

		public PPtr<Texture2D.Texture2D> Texture = new();
		public PPtr<Texture2D.Texture2D> AlphaTexture = new();
		public Rectf TextureRect = new();
		public Vector2f TextureRectOffset = new();
		public Vector2f AtlasRectOffset = new();
		public Vector4f UVTransform = new();
	}
}
