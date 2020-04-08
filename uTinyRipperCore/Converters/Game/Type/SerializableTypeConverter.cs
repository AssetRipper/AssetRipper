using System;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Layout;
using uTinyRipper.Layout.AnimationClips;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class SerializableTypeConverter
	{
		public static TypeTree GenerateTypeTree(AssetLayout layout, SerializableType origin)
		{
			TypeTreeContext context = new TypeTreeContext(layout);
			GenerateTypeTree(context, origin);

			TypeTree instance = new TypeTree();
			instance.Nodes = context.Nodes.ToArray();
			instance.StringBuffer = Array.Empty<byte>();
			return instance;
		}

		private static void GenerateTypeTree(TypeTreeContext context, SerializableType origin)
		{
			context.AddNode(nameof(MonoBehaviour), TypeTreeUtils.BaseName);
			context.BeginChildren();
			MonoBehaviourLayout.GenerateTypeTree(context);
			GenerateFields(context, origin);
			context.EndChildren();
		}

		private static void GenerateNode(TypeTreeContext context, SerializableType origin, string name)
		{
#warning TODO: QuaternionCurve, Vector3Curve, PPtrCurve
			if (origin.IsPrimitive())
			{
				context.AddPrimitive(origin.Name, name);
			}
			else if (origin.IsString())
			{
				context.AddString(name);
			}
			else if (IsEngineCurve(origin))
			{
				GenerateEngineCurve(context, origin, name);
			}
			else if (origin.IsEngineStruct())
			{
				GenerateEngineStruct(context, origin, name);
			}
			else if (origin.IsEnginePointer())
			{
				context.AddPPtr(origin.Name, name);
			}
			else
			{
				GenerateSerializableNode(context, origin, name);
			}
		}

		private static bool IsEngineCurve(SerializableType origin)
		{
			switch (origin.Name)
			{
				case SerializableType.FloatCurveName:
				case SerializableType.Vector3CurveName:
				case SerializableType.QuaternionCurveName:
				case SerializableType.PPtrCurveName:
					return true;

				default:
					return false;
			}
		}

		private static void GenerateEngineCurve(TypeTreeContext context, SerializableType origin, string name)
		{
			switch (origin.Name)
			{
				case SerializableType.FloatCurveName:
					FloatCurveLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector3CurveName:
					Vector3CurveLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.QuaternionCurveName:
					QuaternionCurveLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.PPtrCurveName:
					PPtrCurveLayout.GenerateTypeTree(context, name);
					break;

				default:
					throw new Exception($"Unknown engine curve {origin.Name}");
			}
		}

		private static void GenerateEngineStruct(TypeTreeContext context, SerializableType origin, string name)
		{
			switch (origin.Name)
			{
				case SerializableType.Vector2Name:
					Vector2fLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector2IntName:
					Vector2iLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector3Name:
					Vector3fLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector3IntName:
					Vector3iLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector4Name:
					Vector4fLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.RectName:
					RectfLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.BoundsName:
					AABBLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.BoundsIntName:
					AABBiLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.QuaternionName:
					QuaternionfLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Matrix4x4Name:
					Matrix4x4fLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.ColorName:
					ColorRGBAfLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.Color32Name:
					ColorRGBA32Layout.GenerateTypeTree(context, name);
					break;
				case SerializableType.LayerMaskName:
					LayerMaskLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.AnimationCurveName:
					AnimationCurveTplLayout.GenerateTypeTree(context, name, SingleLayout.GenerateTypeTree);
					break;
				case SerializableType.GradientName:
					GradientLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.RectOffsetName:
					RectOffsetLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.GUIStyleName:
					GUIStyleLayout.GenerateTypeTree(context, name);
					break;
				case SerializableType.PropertyNameName:
					PropertyNameLayout.GenerateTypeTree(context, name);
					break;

				default:
					throw new Exception($"Unknown engine struct {origin.Name}");
			}
		}

		private static void GenerateSerializableNode(TypeTreeContext context, SerializableType origin, string name)
		{
			context.AddNode(origin.Name, name);
			context.BeginChildren();
			GenerateFields(context, origin);
			context.EndChildren();
		}

		private static void GenerateFields(TypeTreeContext context, SerializableType origin)
		{
			if (origin.Base != null)
			{
				GenerateFields(context, origin.Base);
			}

			foreach (SerializableType.Field field in origin.Fields)
			{
				if (field.IsArray)
				{
					GenerateVectorNode(context, field.Type, field.Name);
				}
				else
				{
					GenerateNode(context, field.Type, field.Name);
				}
			}
		}

		private static void GenerateVectorNode(TypeTreeContext context, SerializableType origin, string name)
		{
			context.BeginArray(name);
			GenerateNode(context, origin, TypeTreeUtils.DataName);
			context.EndArray();
		}
	}
}
