using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;

namespace UtinyRipper.Classes
{
	public class MovieTexture : Texture
	{
		public MovieTexture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			IsLoop = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);

			AudioClip.Read(stream);
			m_movieData = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);

			ColorSpace = stream.ReadInt32();
		}

		public override void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(m_movieData, 0, m_movieData.Length);
			}
		}

		public override string ExportExtension => "ogv";

		public bool IsLoop { get; private set; }
		public IReadOnlyList<byte> MovieData => m_movieData;
		public int ColorSpace { get; private set; }

		public PPtr<AudioClip> AudioClip;

		private byte[] m_movieData;
	}
}
