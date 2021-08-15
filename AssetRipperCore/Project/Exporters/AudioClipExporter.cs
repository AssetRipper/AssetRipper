using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Collections;
using System;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project.Exporters
{
	public class AudioClipExporter : BinaryAssetExporter
	{
		private bool _hasFailedToLoadLibPreviously;
		
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new OggFileExportCollection(this, (AudioClip)asset);
		}

		public override bool Export(IExportContainer container, Object asset, string path)
		{
			if (_hasFailedToLoadLibPreviously)
			{
				return false;
			}

			try
			{
				return base.Export(container, asset, path);
			}
			catch (DllNotFoundException e)
			{
				Logger.Error(LogCategory.Export, "Either LibVorbis or LibOgg is missing from your system, so Ogg audio clips cannot be exported. This message will not repeat.");
				_hasFailedToLoadLibPreviously = true;
				return false;
			}
		}
	}
}