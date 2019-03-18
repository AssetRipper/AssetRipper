using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Textures;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

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

			ColorSpace = (ColorSpace)reader.ReadInt32();
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
			
			yield return AudioClip.FetchDependency(file, isLog, ToLogString, AudioClipName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(LoopName, IsLoop);
			node.Add(AudioClipName, AudioClip.ExportYAML(container));
			node.Add(MovieDataName, MovieData.ExportYAML());
			node.Add(ColorSpaceName, (int)ColorSpace);
			return node;
		}

		public bool IsLoop { get; private set; }
		public IReadOnlyList<byte> MovieData => m_movieData;
		public ColorSpace ColorSpace { get; private set; }

		public const string LoopName = "m_Loop";
		public const string AudioClipName = "m_AudioClip";
		public const string MovieDataName = "m_MovieData";
		public const string ColorSpaceName = "m_ColorSpace";

		public PPtr<AudioClip> AudioClip;

		private byte[] m_movieData;
	}
}
