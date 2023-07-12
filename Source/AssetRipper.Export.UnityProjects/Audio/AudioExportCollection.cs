using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
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
		importer.P_3D_C1020 = Asset.P_3D_C83;
		importer.Ambisonic_C1020 = Asset.Ambisonic_C83;
		if (importer.Has_DefaultSettings_C1020())
		{
			ISampleSettings settings = importer.DefaultSettings_C1020;
			settings.CompressionFormat = Asset.CompressionFormat_C83;
			settings.LoadType = Asset.LoadType_C83;
			settings.PreloadAudioData = Asset.PreloadAudioData_C83;
		}
		importer.Format_C1020 = Asset.Format_C83;
		importer.LoadInBackground_C1020 = Asset.LoadInBackground_C83;
		importer.PreloadAudioData_C1020 = Asset.PreloadAudioData_C83;
		importer.Stream_C1020 = Asset.Stream_C83;
		importer.UseHardware_C1020 = Asset.UseHardware_C83;
		if (importer.Has_AssetBundleName_C1020() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_C1020 = Asset.AssetBundleName;
		}
		return importer;
	}
}
