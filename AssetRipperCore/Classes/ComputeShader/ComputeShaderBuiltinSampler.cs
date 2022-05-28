using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderBuiltinSampler : IAssetReadable, IYamlExportable
	{
		public static bool HaUnsignedSampler(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 1);

		public void Read(AssetReader reader)
		{
			if (HaUnsignedSampler(reader.Version))
			{
				Sampler = reader.ReadUInt32();
			}
			else
			{
				SignedSampler = reader.ReadInt32();
			}

			BindPoint = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("sampler", HaUnsignedSampler(container.Version) ? Sampler : SignedSampler);
			node.Add("bindPoint", BindPoint);
			return node;
		}

		public uint Sampler { get; set; }
		public int SignedSampler { get; set; }
		public int BindPoint { get; set; }
	}
}
