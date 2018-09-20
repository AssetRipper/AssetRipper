using System;

namespace uTinyRipper
{
	public enum VersionType
	{
		Alpha = 0,
		Beta,
		Base,
		Final,
		Patch,

		MaxValue = Patch,
	}

	public static class VersionTypeExtentions
	{
		public static string ToLiteral(this VersionType _this)
		{
			switch(_this)
			{
				case VersionType.Alpha:
					return "a";

				case VersionType.Beta:
					return "b";

				case VersionType.Base:
					return string.Empty;

				case VersionType.Final:
					return "f";

				case VersionType.Patch:
					return "p";

				default:
					throw new Exception($"Unsupported vertion type {_this}");
			}
		}
	}
}
