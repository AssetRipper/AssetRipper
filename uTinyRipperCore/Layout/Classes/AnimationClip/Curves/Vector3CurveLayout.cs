using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout.AnimationClips
{
	public sealed class Vector3CurveLayout
	{
		public Vector3CurveLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			Vector3CurveLayout layout = context.Layout.AnimationClip.Vector3Curve;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			AnimationCurveTplLayout.GenerateTypeTree(context, name, Vector3fLayout.GenerateTypeTree);
			context.AddString(layout.PathName);
			context.EndChildren();
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCurveName => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPathName => true;

		public string Name => nameof(Vector3Curve);
		public string CurveName => "curve";
		public string PathName => "path";
	}
}
