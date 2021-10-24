using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public class UnityAssetBase : IAssetNew
	{
		public UnityVersion AssetUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		
		public virtual void ReadEditor(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public virtual void ReadRelease(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public virtual void Read(AssetReader reader)
		{
			AssetUnityVersion = reader.Version;
			EndianType = reader.EndianType;
			if (reader.Flags.IsRelease())
				ReadRelease(reader);
			else
				ReadEditor(reader);
		}

		public virtual void WriteEditor(AssetWriter writer)
		{
			throw new NotSupportedException();
		}

		public virtual void WriteRelease(AssetWriter writer)
		{
			throw new NotSupportedException();
		}

		public virtual void Write(AssetWriter writer)
		{
			if (writer.Flags.IsRelease())
				WriteRelease(writer);
			else
				WriteEditor(writer);
		}

		public virtual YAMLNode ExportYAML(bool release)
		{
			if (release)
				return ExportYAMLRelease();
			else
				return ExportYAMLDebug();
		}

		public virtual YAMLNode ExportYAMLDebug()
		{
			throw new NotSupportedException();
		}

		public virtual YAMLNode ExportYAMLRelease()
		{
			throw new NotSupportedException();
		}
	}
}
