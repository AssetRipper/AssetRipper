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
			return _this switch
			{
				UnityVersionType.Alpha => "a",
				UnityVersionType.Beta => "b",
				UnityVersionType.China => "c",
				UnityVersionType.Final => "f",
				UnityVersionType.Patch => "p",
				UnityVersionType.Experimental => "x",
				_ => throw new Exception($"Unsupported vertion type {_this}"),
			};
		}
	}
}
