using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_1020;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Subclasses.SampleSettings;

namespace AssetRipper.Export.UnityProjects.Audio;

public class AudioExportCollection : AssetExportCollection
{
	public AudioExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		IAudioClip clip = (IAudioClip)Asset;
		IAudioImporter importer = AudioImporterFactory.CreateAsset(container.File, container.ExportVersion);
		importer.P_3D_C1020 = clip.P_3D_C83;
		importer.Ambisonic_C1020 = clip.Ambisonic_C83;
		if (importer.Has_DefaultSettings_C1020())
		{
			ISampleSettings settings = importer.DefaultSettings_C1020;
			settings.CompressionFormat = clip.CompressionFormat_C83;
			settings.LoadType = clip.LoadType_C83;
			settings.PreloadAudioData = clip.PreloadAudioData_C83;
		}
		importer.Format_C1020 = clip.Format_C83;
		importer.LoadInBackground_C1020 = clip.LoadInBackground_C83;
		importer.PreloadAudioData_C1020 = clip.PreloadAudioData_C83;
		importer.Stream_C1020 = clip.Stream_C83;
		importer.UseHardware_C1020 = clip.UseHardware_C83;
		if (importer.Has_AssetBundleName_C1020() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_C1020 = Asset.AssetBundleName;
		}
		return importer;
	}
}
