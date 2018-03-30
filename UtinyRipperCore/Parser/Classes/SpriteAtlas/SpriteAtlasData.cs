using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.SpriteAtlases
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

		public void Read(AssetStream stream)
		{
			Texture.Read(stream);
			AlphaTexture.Read(stream);
			Texture.Read(stream);
			TextureRectOffset.Read(stream);
			if(IsReadAtlasRectOffset(stream.Version))
			{
				AtlasRectOffset.Read(stream);
			}
			UVTransform.Read(stream);
			DownscaleMultiplier = stream.ReadSingle();
			SettingsRaw = stream.ReadUInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			Texture2D texture = Texture.FindObject(file);
			if(texture == null)
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

			if(!AlphaTexture.IsNull)
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
