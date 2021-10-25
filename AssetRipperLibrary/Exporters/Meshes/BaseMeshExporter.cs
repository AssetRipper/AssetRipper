using AssetRipper.Core;
using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Library.Exporters.Meshes
{
	public abstract class BaseMeshExporter : IAssetExporter
	{
		protected MeshExportFormat ExportFormat { get; set; }
		/// <summary>
		/// Should the exporter use the text format or the binary format when exporting?
		/// </summary>
		protected bool BinaryExport { get; set; }
		public BaseMeshExporter(LibraryConfiguration configuration) => ExportFormat = configuration.MeshExportFormat;

		public bool IsHandle(UnityObjectBase asset)
		{
			if (asset is Mesh mesh)
				return IsHandle(mesh);
			else
				return false;
		}

		public abstract bool IsHandle(Mesh mesh);

		public abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset);

		public virtual byte[] ExportBinary(Mesh mesh) => null;

		public virtual string ExportText(Mesh mesh) => null;

		public bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			if (BinaryExport)
			{
				byte[] data = ExportBinary((Mesh)asset);
				if (data == null || data.Length == 0)
					return false;

				using (Stream fileStream = FileUtils.CreateVirtualFile(path))
				{
					fileStream.Write(data);
				}
				return true;
			}
			else
			{
				string text = ExportText((Mesh)asset);
				if (string.IsNullOrEmpty(text))
					return false;

				using (Stream fileStream = FileUtils.CreateVirtualFile(path))
				{
					using (StreamWriter sw = new StreamWriter(fileStream))
					{
						sw.Write(text);
					}
				}
				return true;
			}
		}

		public void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(UnityObjectBase asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
