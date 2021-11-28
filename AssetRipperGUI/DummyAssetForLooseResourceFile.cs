using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.GUI
{
	public class DummyAssetForLooseResourceFile : UnityObjectBase, IHasRawData
	{
		public ResourceFile AssociatedFile;
		public byte[] RawData { get; set; }

		public DummyAssetForLooseResourceFile(ResourceFile associatedFile) {
			AssociatedFile = associatedFile;
			
			using MemoryStream memStream = new();
			AssociatedFile.Stream.CopyTo(memStream);
			RawData = memStream.ToArray();
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public string DataAsString => Encoding.UTF8.GetString(RawData);

		public bool IsProbablyPlainText => DataAsString.Take(32).All(c => char.IsWhiteSpace(c) || !char.IsControl(c));
	}
}