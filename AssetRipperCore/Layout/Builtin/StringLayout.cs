using AssetRipper.Core.Converters.Game;

namespace AssetRipper.Core.Layout.Builtin
{
	public static class StringLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddString(name);
		}
	}
}
