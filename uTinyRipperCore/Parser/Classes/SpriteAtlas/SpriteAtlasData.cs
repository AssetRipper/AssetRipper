using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.SpriteAtlases
{
	public struct SpriteAtlasData : IAssetReadable
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
			yield return Texture.FetchDependency(file, isLog, () => nameof(SpriteAtlasData), "Texture");
			yield return AlphaTexture.FetchDependency(file, isLog, () => nameof(SpriteAtlasData), "AlphaTexture");
		}

		public float DownscaleMultiplier { get; private set; }
		public uint SettingsRaw { get; private set; }

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> AlphaTexture;
		public Rectf TextureRect;
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;
	}
}
