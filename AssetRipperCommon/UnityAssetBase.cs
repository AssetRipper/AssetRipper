using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public class UnityAssetBase : IAssetNew, IDependent
	{
		public UnityVersion AssetUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		public TransferInstructionFlags TransferInstructionFlags { get; set; }
		
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
			TransferInstructionFlags = reader.Flags;
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

		public virtual YAMLNode ExportYAMLDebug(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public virtual YAMLNode ExportYAMLRelease(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			if (container.ExportFlags.IsRelease())
				return ExportYAMLRelease(container);
			else
				return ExportYAMLDebug(container);
		}

		public virtual IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}
	}
}
