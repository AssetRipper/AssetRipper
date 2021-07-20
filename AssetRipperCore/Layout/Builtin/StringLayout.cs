using AssetRipper.Converters.Game;

namespace AssetRipper.Layout.Builtin
{
	public static class StringLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddString(name);
		}
	}
}
