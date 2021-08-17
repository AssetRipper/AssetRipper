using AssetRipper.Core;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioClipExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new OggFileExportCollection(this, (AudioClip)asset);
		}

		public bool Export(IExportContainer container, Core.Classes.Object.Object asset, string path)
		{
			AudioClip audioClip = (AudioClip)asset;
			byte[] decodedData = AudioClipDecoder.GetDecodedAudioClipData(audioClip);
			if (decodedData == null)
				return false;

			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(decodedData);
					}
				}
			}
			return true;
		}

		public void Export(IExportContainer container, Core.Classes.Object.Object asset, string path, Action<IExportContainer, Core.Classes.Object.Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Core.Classes.Object.Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Core.Classes.Object.Object> assets, string path, Action<IExportContainer, Core.Classes.Object.Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool IsHandle(Core.Classes.Object.Object asset, CoreConfiguration options)
		{
			return true;
		}

		public AssetType ToExportType(Core.Classes.Object.Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
