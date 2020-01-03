using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class TypeTreeUtils
	{
		public const string BoolName = nameof(TreeNodeType.@bool);
		public const string CharName = nameof(TreeNodeType.@char);
		public const string SInt8Name = nameof(TreeNodeType.SInt8);
		public const string UInt8Name = nameof(TreeNodeType.UInt8);
		public const string SInt16Name = nameof(TreeNodeType.SInt16);
		public const string UInt16Name = nameof(TreeNodeType.UInt16);
		public const string IntName = nameof(TreeNodeType.@int);
		public const string UnsignedIntName = "unsigned int";
		public const string SInt64Name = nameof(TreeNodeType.SInt64);
		public const string UInt64Name = nameof(TreeNodeType.UInt64);
		public const string FloatName = nameof(TreeNodeType.@float);
		public const string DoubleName = nameof(TreeNodeType.@double);
		public const string StringName = nameof(TreeNodeType.@string);

		public const string Vector2Name = nameof(TreeNodeType.Vector2f);
		public const string Vector2IntName = "Vector2Int";
		public const string Vector3Name = nameof(TreeNodeType.Vector3f);
		public const string Vector3IntName = "Vector3Int";
		public const string Vector4Name = nameof(TreeNodeType.Vector4f);
		public const string RectName = nameof(TreeNodeType.Rectf);
		public const string BoundName = nameof(TreeNodeType.AABB);
		public const string BoundsIntName = nameof(TreeNodeType.BoundsInt);
		public const string QuaternionName = nameof(TreeNodeType.Quaternionf);
		public const string Matrix4x4Name = nameof(TreeNodeType.Matrix4x4f);
		public const string ColorName = nameof(TreeNodeType.ColorRGBA);
		public const string Color32Name = nameof(TreeNodeType.ColorRGBA);
		public const string BitFieldName = nameof(TreeNodeType.BitField);
		public const string AnimationCurveName = nameof(TreeNodeType.AnimationCurve);
		public const string KeyframeName = "Keyframe";
		public const string GradientName = nameof(TreeNodeType.Gradient);
		public const string GradientNEWName = nameof(TreeNodeType.GradientNEW);
		public const string RectOffsetName = nameof(TreeNodeType.RectOffset);
		public const string GUIStyleName = nameof(TreeNodeType.GUIStyle);
		public const string GUIStyleStateName = "GUIStyleState";
		public const string PropertyNameName = "PropertyName";

		public const string BaseName = nameof(TreeNodeType.Base);
		public const string PairName = nameof(TreeNodeType.pair);
		public const string FirstName = nameof(TreeNodeType.first);
		public const string SecondName = nameof(TreeNodeType.second);
		public const string VectorName = nameof(TreeNodeType.vector);
		public const string ArrayName = nameof(TreeNodeType.Array);
		public const string SizeName = nameof(TreeNodeType.size);
		public const string DataName = nameof(TreeNodeType.data);
		public const string TypeStarName = "Type*";
	}
}
