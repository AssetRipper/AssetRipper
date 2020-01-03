using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
	public static class BehaviourConverter
	{
		public static void Convert(IExportContainer container, Behaviour origin, Behaviour instance)
		{
			ComponentConverter.Convert(container, origin, instance);
			instance.Enabled = origin.Enabled;
		}
	}
}
