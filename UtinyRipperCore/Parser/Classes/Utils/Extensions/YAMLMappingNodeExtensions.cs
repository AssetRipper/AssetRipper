using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public static class YAMLMappingNodeExtensions
	{
		public static void AddSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if(version > 1)
			{
				_this.Add(SerializedVersionName, version);
			}
		}

		public static void InsertSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if(version > 1)
			{
				_this.InsertBegin(SerializedVersionName, version);
			}
		}

		private const string SerializedVersionName = "serializedVersion";
	}
}
