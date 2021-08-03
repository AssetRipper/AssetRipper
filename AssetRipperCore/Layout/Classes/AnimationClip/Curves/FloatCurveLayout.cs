using AssetRipper.Core.Converters.Game;
using AssetRipper.Core.Layout.Builtin;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.AnimationClip.Curves;

namespace AssetRipper.Core.Layout.Classes.AnimationClip.Curves
{
	public sealed class FloatCurveLayout
	{
		public FloatCurveLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2))
			{
				HasScript = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			FloatCurveLayout layout = context.Layout.AnimationClip.FloatCurve;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			AnimationCurveTplLayout.GenerateTypeTree(context, name, SingleLayout.GenerateTypeTree);
			context.AddString(layout.AttributeName);
			context.AddString(layout.PathName);
			context.AddNode(TypeTreeUtils.TypeStarName, layout.ClassIDName, 1, sizeof(int));
			context.AddPPtr(context.Layout.MonoScript.Name, layout.ScriptName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCurve => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAttribute => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPath => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasClassID => true;
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasScript { get; }

		public string Name => nameof(FloatCurve);
		public string CurveName => "curve";
		public string AttributeName => "attribute";
		public string PathName => "path";
		public string ClassIDName => "classID";
		public string ScriptName => "script";
	}
}
