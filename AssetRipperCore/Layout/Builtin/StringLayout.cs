using AssetRipper.Converters;

namespace AssetRipper.Layout
{
	public static class StringLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddString(name);
		}
	}
}
