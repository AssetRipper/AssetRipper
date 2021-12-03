using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class RegisterVersionHandlerAttribute : Attribute
	{
		public Type HandlerType { get; }

		public RegisterVersionHandlerAttribute(Type type)
		{
			HandlerType = type;
		}
	}
}
