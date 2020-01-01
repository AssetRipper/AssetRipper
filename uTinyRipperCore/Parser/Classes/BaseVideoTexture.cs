using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class BaseVideoTexture : Texture
	{
		public BaseVideoTexture(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			IsLoop = reader.ReadBoolean();
			reader.AlignStream();

			AudioClip.Read(reader);
			MovieData = reader.ReadByteArray();
			reader.AlignStream();

			ColorSpace = (ColorSpace)reader.ReadInt32();
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(MovieData, 0, MovieData.Length);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AudioClip, AudioClipName);
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

		protected void ReadTexture(AssetReader reader)
		{
			base.Read(reader);
		}

		protected IEnumerable<PPtr<Object>> FetchDependenciesTexture(DependencyContext context)
		{
			return base.FetchDependencies(context);
		}

		protected YAMLMappingNode ExportYAMLRootTexture(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}

		public bool IsLoop { get; set; }
		public byte[] MovieData { get; set; }
		public ColorSpace ColorSpace { get; set; }

		public const string LoopName = "m_Loop";
		public const string AudioClipName = "m_AudioClip";
		public const string MovieDataName = "m_MovieData";
		public const string ColorSpaceName = "m_ColorSpace";

		public PPtr<AudioClip> AudioClip;
	}
}
