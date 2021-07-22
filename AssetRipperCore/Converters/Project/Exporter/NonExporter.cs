using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Parser.Classes.Object.Object;

namespace AssetRipper.Converters.Project.Exporter
{
	public class NonExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			throw new NotImplementedException();
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			throw new NotImplementedException();
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotImplementedException();
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotImplementedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotImplementedException();
		}

		public bool IsHandle(Object asset, ExportOptions options)
		{
			throw new NotImplementedException();
		}

		public AssetType ToExportType(Object asset)
		{
			throw new NotImplementedException();
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			throw new NotImplementedException();
		}
	}
}
