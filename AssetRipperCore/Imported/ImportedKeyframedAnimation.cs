using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedKeyframedAnimation
	{
		public string Name { get; set; }
		public float SampleRate { get; set; }
		public List<ImportedAnimationKeyframedTrack> TrackList { get; set; }

		public ImportedAnimationKeyframedTrack FindTrack(string path)
		{
			var track = TrackList.Find(x => x.Path == path);
			if (track == null)
			{
				track = new ImportedAnimationKeyframedTrack { Path = path };
				TrackList.Add(track);
			}

			return track;
		}
	}
}
