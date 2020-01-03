using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public static class TupleLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name, TypeTreeGenerator first, TypeTreeGenerator second)
		{
			context.AddNode(TypeTreeUtils.PairName, name);
			context.BeginChildren();
			first(context, TypeTreeUtils.FirstName);
			second(context, TypeTreeUtils.SecondName);
			context.EndChildren();
		}
	}
}
