using AssetRipper.Core.Extensions;
using System.Text;

namespace AssetRipper.Core.YAML.Extensions
{
	public static class ArrayYAMLExtensions
	{
		public static YAMLNode ExportYAML(this byte[] _this)
		{
			StringBuilder sb = new StringBuilder(_this.Length * 2);
			for (int i = 0; i < _this.Length; i++)
			{
				sb.AppendHex(_this[i]);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static void AddTypelessData(this YAMLMappingNode mappingNode, string name, byte[] data)
		{
			mappingNode.Add(name, data.Length);
			mappingNode.Add(Layout.LayoutInfo.TypelessdataName, data.ExportYAML());
		}
	}
}
