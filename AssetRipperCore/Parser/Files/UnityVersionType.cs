using System;

namespace AssetRipper.Core.Parser.Files
{
	public enum UnityVersionType
	{
		Alpha = 0,
		Beta,
		Final,
		Patch,

		MaxValue = Patch,
	}

	public static class UnityVersionTypeExtentions
	{
		public static string ToLiteral(this UnityVersionType _this)
		{
			switch (_this)
			{
				case UnityVersionType.Alpha:
					return "a";

				case UnityVersionType.Beta:
					return "b";

				case UnityVersionType.Final:
					return "f";

				case UnityVersionType.Patch:
					return "p";

				default:
					throw new Exception($"Unsupported vertion type {_this}");
			}
		}
	}
}
