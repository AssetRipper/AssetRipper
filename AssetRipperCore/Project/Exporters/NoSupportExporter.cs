using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Project.Exporters
{
	/// <summary>Designed to cause stack traces</summary>
	public class NoSupportExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool IsHandle(UnityObject asset, ExportOptions options)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(UnityObject asset)
		{
			throw new NotSupportedException();
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			throw new NotSupportedException();
		}
	}
}
