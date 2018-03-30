using System;

namespace UtinyRipper
{
	internal class EnumNameAttribute : Attribute
	{
		public EnumNameAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
