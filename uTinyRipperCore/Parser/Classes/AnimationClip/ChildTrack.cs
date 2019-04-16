using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct ChildTrack : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Track.Read(reader);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Track.FetchDependency(file, isLog, () => nameof(ChildTrack), TrackName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PathName, Path);
			node.Add(ClassIDName, (int)ClassID);
			node.Add(TrackName, Track.ExportYAML(container));
			return node;
		}

		public string Path { get; private set; }
		public ClassIDType ClassID { get; private set; }

		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string TrackName = "track";

		public PPtr<BaseAnimationTrack> Track;
	}
}
