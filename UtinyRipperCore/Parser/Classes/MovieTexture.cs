using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.SerializedFiles;

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

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(m_movieData, 0, m_movieData.Length);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return AudioClip.FetchDependency(file, isLog, ToLogString, "m_AudioClip");
		}

		public override string ExportExtension => "ogv";

		public bool IsLoop { get; private set; }
		public IReadOnlyList<byte> MovieData => m_movieData;
		public int ColorSpace { get; private set; }

		public PPtr<AudioClip> AudioClip;

		private byte[] m_movieData;
	}
}
