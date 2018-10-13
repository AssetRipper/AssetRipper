using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class MovieTexture : Texture
	{
		public MovieTexture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			IsLoop = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			AudioClip.Read(reader);
			m_movieData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);

			ColorSpace = reader.ReadInt32();
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
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
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
