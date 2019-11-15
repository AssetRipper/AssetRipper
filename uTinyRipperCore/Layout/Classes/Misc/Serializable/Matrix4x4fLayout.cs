using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class Matrix4x4fLayout
	{
		public Matrix4x4fLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Matrix4x4fLayout layout = context.Layout.Serialized.Matrix4x4f;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddSingle(layout.E00Name);
			context.AddSingle(layout.E01Name);
			context.AddSingle(layout.E02Name);
			context.AddSingle(layout.E03Name);
			context.AddSingle(layout.E10Name);
			context.AddSingle(layout.E11Name);
			context.AddSingle(layout.E12Name);
			context.AddSingle(layout.E13Name);
			context.AddSingle(layout.E20Name);
			context.AddSingle(layout.E21Name);
			context.AddSingle(layout.E22Name);
			context.AddSingle(layout.E23Name);
			context.AddSingle(layout.E30Name);
			context.AddSingle(layout.E31Name);
			context.AddSingle(layout.E32Name);
			context.AddSingle(layout.E33Name);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE00 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE01 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE02 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE03 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE10 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE11 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE12 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE13 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE20 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE21 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE22 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE23 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE30 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE32 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasE33 => true;

		public string Name => TypeTreeUtils.Matrix4x4Name;
		public string E00Name => "e00";
		public string E01Name => "e01";
		public string E02Name => "e02";
		public string E03Name => "e03";
		public string E10Name => "e10";
		public string E11Name => "e11";
		public string E12Name => "e12";
		public string E13Name => "e13";
		public string E20Name => "e20";
		public string E21Name => "e21";
		public string E22Name => "e22";
		public string E23Name => "e23";
		public string E30Name => "e30";
		public string E31Name => "e31";
		public string E32Name => "e32";
		public string E33Name => "e33";
	}
}
