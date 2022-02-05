using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class Blend1dDataConstant : IBlend1dDataConstant
	{
		public void Read(AssetReader reader)
		{
			ChildThresholdArray = reader.ReadSingleArray();
		}

		public void Write(AssetWriter writer)
		{
			throw new NotSupportedException();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public float[] ChildThresholdArray { get; set; }
	}
}
