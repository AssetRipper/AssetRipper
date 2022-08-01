namespace AssetRipper.Library.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RegisterPluginAttribute : Attribute
	{
		public Type PluginType { get; }

		public RegisterPluginAttribute(Type pluginType)
		{
			PluginType = pluginType;
		}
	}
}
