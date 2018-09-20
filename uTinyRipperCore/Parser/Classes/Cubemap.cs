using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// CubemapTexture previously
	/// </summary>
	public sealed class Cubemap : Texture2D
	{
		public Cubemap(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadSourceTextures(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadSourceTextures(reader.Version))
			{
				m_sourceTextures = reader.ReadArray<PPtr<Texture2D>>();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if (IsReadSourceTextures(file.Version))
			{
				foreach(PPtr<Texture2D> texture in m_sourceTextures)
				{
					yield return texture.FetchDependency(file, isLog, ToLogString, "sourceTextures");
				}
			}
		}

		public IReadOnlyList<PPtr<Texture2D>> SourceTextures => m_sourceTextures;

		private PPtr<Texture2D>[] m_sourceTextures;
	}
}
