using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Yaml;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Project.Exporters
{
	public abstract class YamlExporterBase : IAssetExporter
	{
		public virtual bool IsHandle(IUnityObjectBase asset)
		{
			return true;
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using Stream fileStream = File.Create(path);
			using InvariantStreamWriter streamWriter = new InvariantStreamWriter(fileStream, UTF8);
			YamlWriter writer = new YamlWriter();
			YamlDocument doc = asset.ExportYamlDocument(container);
			writer.AddDocument(doc);
			writer.Write(streamWriter);
			return true;
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			using Stream fileStream = File.Create(path);
			using InvariantStreamWriter streamWriter = new InvariantStreamWriter(fileStream, UTF8);
			YamlWriter writer = new YamlWriter();
			writer.WriteHead(streamWriter);
			foreach (IUnityObjectBase asset in assets)
			{
				YamlDocument doc = asset.ExportYamlDocument(container);
				writer.WriteDocument(doc);
			}
			writer.WriteTail(streamWriter);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Yaml supports only single file export");
		}

		public abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset);

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			return AssetType.Serialized;
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Serialized;
			return true;
		}

		private static readonly Encoding UTF8 = new UTF8Encoding(false);
	}
}
