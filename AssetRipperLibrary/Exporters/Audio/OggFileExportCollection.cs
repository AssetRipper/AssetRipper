using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;

namespace AssetRipper.Library.Exporters.Audio
{
	internal class OggFileExportCollection : AssetExportCollection
	{
		public OggFileExportCollection(IAssetExporter assetExporter, AudioClip asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(Core.Classes.Object.Object asset)
		{
			return "ogg";
		}
	}
}