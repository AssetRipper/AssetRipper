namespace AssetRipper.Yaml.Extensions;

public static class YamlSerializedVersionExtensions
{
	public static void AddSerializedVersion(this YamlMappingNode _this, int version)
	{
		if (version > 1)
		{
			_this.Add(SerializedVersionName, version);
		}
	}

	public static void ForceAddSerializedVersion(this YamlMappingNode _this, int version)
	{
		if (version > 0)
		{
			_this.Add(SerializedVersionName, version);
		}
	}

	public static void InsertSerializedVersion(this YamlMappingNode _this, int version)
	{
		if (version > 1)
		{
			_this.InsertBegin(SerializedVersionName, version);
		}
	}

	public const string SerializedVersionName = "serializedVersion";
}
