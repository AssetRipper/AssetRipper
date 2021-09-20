using AssetRipper.Core.Classes;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Builtin;
using AssetRipper.Core.Layout.Classes;
using AssetRipper.Core.Layout.Classes.AnimationClip.Curves;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;
using AssetRipper.Core.Layout.Classes.Misc.Serializable.GUIStyle;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Structure.Assembly.Serializable;
using System;
using System.Linq;

namespace AssetRipper.Core.Converters.Game
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
				case MonoUtils.FloatCurveName:
				case MonoUtils.Vector3CurveName:
				case MonoUtils.QuaternionCurveName:
				case MonoUtils.PPtrCurveName:
					return true;

				default:
					return false;
			}
		}

		private static void GenerateEngineCurve(TypeTreeContext context, SerializableType origin, string name)
		{
			switch (origin.Name)
			{
				case MonoUtils.FloatCurveName:
					FloatCurveLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Vector3CurveName:
					Vector3CurveLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.QuaternionCurveName:
					QuaternionCurveLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.PPtrCurveName:
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
				case MonoUtils.Vector2Name:
					Vector2fLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Vector2IntName:
					Vector2iLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Vector3Name:
					Vector3fLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Vector3IntName:
					Vector3iLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Vector4Name:
					Vector4fLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.RectName:
					RectfLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.BoundsName:
					AABBLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.BoundsIntName:
					AABBiLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.QuaternionName:
					QuaternionfLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Matrix4x4Name:
					Matrix4x4fLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.ColorName:
					ColorRGBAfLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.Color32Name:
					ColorRGBA32Layout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.LayerMaskName:
					LayerMaskLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.AnimationCurveName:
					AnimationCurveTplLayout.GenerateTypeTree(context, name, SingleLayout.GenerateTypeTree);
					break;
				case MonoUtils.GradientName:
					GradientLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.RectOffsetName:
					RectOffsetLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.GUIStyleName:
					GUIStyleLayout.GenerateTypeTree(context, name);
					break;
				case MonoUtils.PropertyNameName:
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
