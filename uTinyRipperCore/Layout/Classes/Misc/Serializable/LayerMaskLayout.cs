using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class LayerMaskLayout
	{
		public LayerMaskLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2))
			{
				// Bits size has been changed to 32
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			Is32Bits = info.Version.IsGreaterEqual(2);
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			LayerMaskLayout layout = context.Layout.Serialized.LayerMask;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			if (layout.Is32Bits)
			{
				context.AddUInt32(layout.BitsName);
			}
			else
			{
				context.AddUInt16(layout.BitsName);
			}
			context.EndChildren();
		}

		public int Version { get; }

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool Is32Bits { get; }

		public string Name => TypeTreeUtils.BitFieldName;
		public string BitsName => "m_Bits";
	}
}
