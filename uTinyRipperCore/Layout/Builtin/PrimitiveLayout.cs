using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public static class BoolLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddBool(name);
		}
	}

	public static class SByteLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddSByte(name);
		}
	}

	public static class ByteLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddByte(name);
		}
	}

	public static class Int16Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddInt16(name);
		}
	}

	public static class UInt16Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddUInt16(name);
		}
	}

	public static class Int32Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddInt32(name);
		}
	}

	public static class UInt32Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddUInt32(name);
		}
	}

	public static class Int64Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddInt64(name);
		}
	}

	public static class UInt64Layout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddUInt64(name);
		}
	}

	public static class SingleLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddSingle(name);
		}
	}

	public static class DoubleLayout
	{
		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddDouble(name);
		}
	}
}
