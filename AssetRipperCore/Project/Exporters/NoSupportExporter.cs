using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Classes.Object.Object;

namespace AssetRipper.Project.Exporters
{
	/// <summary>Designed to cause stack traces</summary>
	public class NoSupportExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool IsHandle(Object asset, ExportOptions options)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(Object asset)
		{
			throw new NotSupportedException();
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			throw new NotSupportedException();
		}
	}
}
