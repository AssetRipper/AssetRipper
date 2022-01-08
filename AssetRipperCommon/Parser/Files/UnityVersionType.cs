using System;

namespace AssetRipper.Core.Parser.Files
{
	public enum UnityVersionType
	{
		Alpha = 0,
		Beta,
		China,
		Final,
		Patch,
		Experimental,

		MaxValue = Experimental,
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

				case UnityVersionType.China:
					return "c";

				case UnityVersionType.Final:
					return "f";

				case UnityVersionType.Patch:
					return "p";

				case UnityVersionType.Experimental:
					return "x";

				default:
					throw new Exception($"Unsupported vertion type {_this}");
			}
		}
	}
}
