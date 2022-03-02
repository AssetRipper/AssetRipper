using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderResource : IAssetReadable, IYAMLExportable
	{
		public static bool HasGeneratedName(UnityVersion version) => version.IsGreaterEqual(5, 1, 0, UnityVersionType.Final, 3);

		public static bool HasSecondaryBindPoint(UnityVersion version) => version.IsGreaterEqual(5, 1, 0, UnityVersionType.Final, 3) && version.IsLess(5, 2, 3, UnityVersionType.Final, 1);

		public static bool HasComputeBufferCounter(UnityVersion version) => version.IsGreaterEqual(5, 2, 3, UnityVersionType.Final, 1) && version.IsLess(2018, 1, 0, UnityVersionType.Beta, 11);

		public static bool HasSamplerBindPoint(UnityVersion version) => version.IsGreaterEqual(2018, 2, 0, UnityVersionType.Beta, 1);

		public static bool HasTexDimension(UnityVersion version) => version.IsGreaterEqual(2018, 2, 0, UnityVersionType.Beta, 9);


		public void Read(AssetReader reader)
		{
			Name = reader.ReadAsset<FastPropertyName>();
			if (HasGeneratedName(reader.Version))
			{
				GeneratedName = reader.ReadAsset<FastPropertyName>();
			}
			BindPoint = reader.ReadInt32();
			if (HasSecondaryBindPoint(reader.Version))
			{
				SecondaryBindPoint = reader.ReadInt32();
			}
			if (HasComputeBufferCounter(reader.Version))
			{
				Counter = reader.ReadAsset<ComputeBufferCounter>();
			}
			if (HasSamplerBindPoint(reader.Version))
			{
				SamplerBindPoint = reader.ReadInt32();
			}
			if (HasTexDimension(reader.Version))
			{
				TexDimension = reader.ReadInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name.ExportYAML(container));
			if (HasGeneratedName(container.Version))
			{
				node.Add("generatedName", GeneratedName.ExportYAML(container));
			}
			node.Add("bindPoint", BindPoint);
			if (HasSecondaryBindPoint(container.Version))
			{
				node.Add("secondaryBindPoint", SecondaryBindPoint);
			}
			if (HasComputeBufferCounter(container.Version))
			{
				node.Add("counter", Counter.ExportYAML(container));
			}
			if (HasSamplerBindPoint(container.Version))
			{
				node.Add("samplerBindPoint", SamplerBindPoint);
			}
			if (HasTexDimension(container.Version))
			{
				node.Add("texDimension", TexDimension);
			}

			return node;
		}

		public FastPropertyName Name { get; set; }
		public FastPropertyName GeneratedName { get; set; }
		public int BindPoint { get; set; }
		public int SecondaryBindPoint { get; set; }
		public ComputeBufferCounter Counter { get; set; }
		public int SamplerBindPoint { get; set; }
		public int TexDimension { get; set; }
	}
}
