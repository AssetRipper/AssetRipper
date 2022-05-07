﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class BaseVideoTexture : Texture
	{
		public BaseVideoTexture(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			IsLoop = reader.ReadBoolean();
			reader.AlignStream();

			AudioClip.Read(reader);
			m_MovieData = reader.ReadByteArray();
			reader.AlignStream();

			ColorSpace = (ColorSpace)reader.ReadInt32();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AudioClip, AudioClipName);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(LoopName, IsLoop);
			node.Add(AudioClipName, AudioClip.ExportYaml(container));
			node.Add(MovieDataName, m_MovieData.ExportYaml());
			node.Add(ColorSpaceName, (int)ColorSpace);
			return node;
		}

		protected void ReadTexture(AssetReader reader)
		{
			base.Read(reader);
		}

		protected IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesTexture(DependencyContext context)
		{
			return base.FetchDependencies(context);
		}

		protected YamlMappingNode ExportYamlRootTexture(IExportContainer container)
		{
			return base.ExportYamlRoot(container);
		}

		public bool IsLoop { get; set; }
		public byte[] m_MovieData { get; set; }
		public ColorSpace ColorSpace { get; set; }

		public const string LoopName = "m_Loop";
		public const string AudioClipName = "m_AudioClip";
		public const string MovieDataName = "m_MovieData";
		public const string ColorSpaceName = "m_ColorSpace";

		public PPtr<AudioClip.AudioClip> AudioClip = new();
	}
}
