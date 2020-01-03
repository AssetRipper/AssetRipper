using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout.Misc
{
	public sealed class KeyframeTplLayout
	{
		public KeyframeTplLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2018))
			{
				// unknown conversion
				Version = 3;
			}
			else if (TangentModeExtensions.TangentMode5Relevant(info.Version))
			{
				// TangentMode enum has been changed
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsGreaterEqual(2, 1) && !info.Flags.IsRelease())
			{
				HasTangentMode = true;
			}
			if (info.Version.IsGreaterEqual(2018))
			{
				HasWeightedMode = true;
				HasInWeight = true;
				HasOutWeight = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name, TypeTreeGenerator generator)
		{
			KeyframeTplLayout layout = context.Layout.Misc.KeyframeTpl;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			context.AddSingle(layout.TimeName);
			generator.Invoke(context, layout.ValueName);
			generator.Invoke(context, layout.InSlopeName);
			generator.Invoke(context, layout.OutSlopeName);
			if (layout.HasTangentMode)
			{
				context.AddInt32(layout.TangentModeName);
			}
			if (layout.HasWeightedMode)
			{
				context.AddInt32(layout.WeightedModeName);
				generator.Invoke(context, layout.InWeightName);
				generator.Invoke(context, layout.OutWeightName);
			}
			context.EndChildren();
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTime => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasValue => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasInSlope => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasOutSlope => true;
		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public bool HasTangentMode { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasWeightedMode { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasInWeight { get; }
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool HasOutWeight { get; }

		public string Name => TypeTreeUtils.KeyframeName;
		public string TimeName => "time";
		public string ValueName => "value";
		public string InSlopeName => "inSlope";
		public string OutSlopeName => "outSlope";
		public string TangentModeName => "tangentMode";
		public string WeightedModeName => "weightedMode";
		public string InWeightName => "inWeight";
		public string OutWeightName => "outWeight";
	}
}
