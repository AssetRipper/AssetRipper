using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class RectfLayout
	{
		public RectfLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2))
			{
				// absolute Min/Max has been replaced by relative values
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsLess(2))
			{
				HasXMin = true;
				HasYMin = true;
				HasXMax = true;
				HasYMax = true;
			}
			else
			{
				HasX = true;
				HasY = true;
				HasWidth = true;
				HasHeight = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			RectfLayout layout = context.Layout.Serialized.Rectf;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			if (layout.Version == 1)
			{
				context.AddSingle(layout.XMinName);
				context.AddSingle(layout.YMinName);
				context.AddSingle(layout.XMaxName);
				context.AddSingle(layout.YMaxName);
			}
			else
			{
				context.AddSingle(layout.XName);
				context.AddSingle(layout.YName);
				context.AddSingle(layout.WidthName);
				context.AddSingle(layout.HeightName);
			}
			context.EndChildren();
		}

		public int Version { get; }

		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public bool HasXMin { get; }
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public bool HasYMin { get; }
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public bool HasXMax { get; }
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public bool HasYMax { get; }
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasX { get; }
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasY { get; }
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasWidth { get; }
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasHeight { get; }

		public string Name => TypeTreeUtils.RectName;
		public string XMinName => "xmin";
		public string YMinName => "ymin";
		public string XMaxName => "xmax";
		public string YMaxName => "ymax";
		public string XName => "x";
		public string YName => "y";
		public string WidthName => "width";
		public string HeightName => "height";
	}
}
