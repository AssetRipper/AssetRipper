using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class ChildTrack : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Track.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Track, TrackName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PathName, Path);
			node.Add(ClassIDName, (int)ClassID);
			node.Add(TrackName, Track.ExportYAML(container));
			return node;
		}

		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }

		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string TrackName = "track";

		public PPtr<BaseAnimationTrack> Track = new();
	}
}
