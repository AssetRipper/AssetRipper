using AssetRipper.SourceGenerated.Classes.ClassID_1020;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Subclasses.SampleSettings;

namespace AssetRipper.Export.UnityProjects.Audio;

public class AudioExportCollection : AssetExportCollection<IAudioClip>
{
	public AudioExportCollection(IAssetExporter assetExporter, IAudioClip asset) : base(assetExporter, asset)
	{
	}

	protected override IAudioImporter CreateImporter(IExportContainer container)
	{
		IAudioImporter importer = AudioImporter.Create(container.File, container.ExportVersion);
		importer.P_3D = Asset.P_3D;
		importer.Ambisonic = Asset.Ambisonic;
		if (importer.Has_DefaultSettings())
		{
			ISampleSettings settings = importer.DefaultSettings;
			settings.CompressionFormat = Asset.CompressionFormat;
			settings.LoadType = Asset.LoadType;
			settings.PreloadAudioData = Asset.PreloadAudioData;
		}
		importer.Format = Asset.Format;
		importer.LoadInBackground = Asset.LoadInBackground;
		importer.PreloadAudioData = Asset.PreloadAudioData;
		importer.Stream = Asset.Stream;
		importer.UseHardware = Asset.UseHardware;
		if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_R = Asset.AssetBundleName;
		}
		return importer;
	}
}
