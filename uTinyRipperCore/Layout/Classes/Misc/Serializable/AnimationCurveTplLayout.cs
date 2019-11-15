using uTinyRipper.Converters;
using uTinyRipper.Layout.Misc;

namespace uTinyRipper.Layout
{
	public sealed class AnimationCurveTplLayout
	{
		public AnimationCurveTplLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2, 1))
			{
				// unknown conversion
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsGreaterEqual(5, 3))
			{
				HasRotationOrder = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name, TypeTreeGenerator generator)
		{
			AnimationCurveTplLayout layout = context.Layout.Serialized.AnimationCurveTpl;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			context.AddArray(layout.CurveName, (c, n) => KeyframeTplLayout.GenerateTypeTree(c, n, generator));
			if (context.Layout.IsAlign)
			{
				context.Align();
			}
			context.AddInt32(layout.PreInfinityName);
			context.AddInt32(layout.PostInfinityName);
			if (layout.HasRotationOrder)
			{
				context.AddInt32(layout.RotationOrderName);
			}
			context.EndChildren();
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCurve => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPreInfinity => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPostInfinity => true;
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public bool HasRotationOrder { get; }

		public string Name => TypeTreeUtils.AnimationCurveName;
		public string CurveName => "m_Curve";
		public string PreInfinityName => "m_PreInfinity";
		public string PostInfinityName => "m_PostInfinity";
		public string RotationOrderName => "m_RotationOrder";
	}
}
