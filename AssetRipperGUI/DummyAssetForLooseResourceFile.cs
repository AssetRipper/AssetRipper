using AssetRipper.Core;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetRipper.GUI
{
	public class DummyAssetForLooseResourceFile : UnityObjectBase
	{
		public ResourceFile AssociatedFile;
		public byte[] RawData;

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

		public bool IsProbablyPlainText => DataAsString.Take(32).All(c => char.IsAscii(c) && !char.IsControl(c));
		
		public string ToFormattedHex()
		{
			StringBuilder sb = new StringBuilder();
			int count = 0;
			foreach(byte b in RawData)
			{
				sb.Append(b.ToString("X2"));
				count++;
				if(count >= 16)
				{
					sb.AppendLine();
					count = 0;
				}
				else if(count % 4 == 0)
				{
					sb.Append('\t');
				}
				else
				{
					sb.Append(' ');
				}
			}
			return sb.ToString();
		}
	}
}