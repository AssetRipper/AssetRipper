using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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

		public static void ForceAddSerializedVersion(this YAMLMappingNode _this, int version)
		{
			if (version > 0)
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

		public const string SerializedVersionName = "serializedVersion";
	}
}
