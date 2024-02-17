using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public sealed class AudioClipExportCollection : AudioExportCollection
	{
		private byte[]? data;
		private readonly string fileExtension;
		public AudioClipExportCollection(AudioClipExporter assetExporter, IAudioClip asset, byte[] data, string fileExtension) : base(assetExporter, asset)
		{
			this.data = data;
			this.fileExtension = fileExtension;
		}

		protected override bool ExportInner(IExportContainer container, string filePath, string dirPath)
		{
			if (data is null or { Length: 0 })
			{
				return false;
			}
			else
			{
				System.IO.File.WriteAllBytes(filePath, data);
				data = null;
				return true;
			}
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return fileExtension;
		}
	}
}
