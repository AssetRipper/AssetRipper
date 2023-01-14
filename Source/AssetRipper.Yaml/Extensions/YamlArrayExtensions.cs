using System.Text;

namespace AssetRipper.Yaml.Extensions
{
	public static class YamlArrayExtensions
	{
		public static YamlNode ExportYaml(this byte[] _this)
		{
			StringBuilder sb = new StringBuilder(_this.Length * 2);
			for (int i = 0; i < _this.Length; i++)
			{
				sb.AppendHex(_this[i]);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static void AddTypelessData(this YamlMappingNode mappingNode, string name, byte[] data)
		{
			mappingNode.Add(name, data.Length);
			mappingNode.Add(TypelessdataName, data.ExportYaml());
		}

		public const string TypelessdataName = "_typelessdata";
	}
}
