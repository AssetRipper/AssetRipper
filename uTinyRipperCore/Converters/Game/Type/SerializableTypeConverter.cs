using System;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Game.Assembly;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class SerializableTypeConverter
	{
		/// <summary>
		/// NOTE: Only for debug purposes at this moment
		/// </summary>
		public static TypeTree GenerateTypeTree(SerializableType origin)
		{
			Version version = new Version(9999, 1, 0, VersionType.Final, 1);
			TypeTreeContext context = new TypeTreeContext(version, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			GenerateTypeTree(context, origin);

			TypeTree instance = new TypeTree();
			instance.Nodes = context.Nodes.ToArray();
			instance.CustomTypeBuffer = Array.Empty<byte>();
			return instance;
		}

		private static void GenerateTypeTree(TypeTreeContext context, SerializableType origin)
		{
			context.AddNode(nameof(MonoBehaviour), TypeTreeUtils.BaseName);
			context.BeginChildren();
			MonoBehaviour.GenerateTypeTree(context);
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

		private static void GenerateEngineStruct(TypeTreeContext context, SerializableType origin, string name)
		{
			switch (origin.Name)
			{
				case SerializableType.Vector2Name:
					Vector2f.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector2IntName:
					Vector2i.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector3Name:
					Vector3f.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector3IntName:
					Vector3i.GenerateTypeTree(context, name);
					break;
				case SerializableType.Vector4Name:
					Vector4f.GenerateTypeTree(context, name);
					break;
				case SerializableType.RectName:
					Rectf.GenerateTypeTree(context, name);
					break;
				case SerializableType.BoundsName:
					AABB.GenerateTypeTree(context, name);
					break;
				case SerializableType.BoundsIntName:
					AABBi.GenerateTypeTree(context, name);
					break;
				case SerializableType.QuaternionName:
					Quaternionf.GenerateTypeTree(context, name);
					break;
				case SerializableType.Matrix4x4Name:
					Matrix4x4f.GenerateTypeTree(context, name);
					break;
				case SerializableType.ColorName:
					ColorRGBAf.GenerateTypeTree(context, name);
					break;
				case SerializableType.Color32Name:
					ColorRGBA32.GenerateTypeTree(context, name);
					break;
				case SerializableType.LayerMaskName:
					LayerMask.GenerateTypeTree(context, name);
					break;
				case SerializableType.AnimationCurveName:
					AnimationCurveTpl<Float>.GenerateTypeTree(context, name, Float.GenerateTypeTree);
					break;
				case SerializableType.GradientName:
					Gradient.GenerateTypeTree(context, name);
					break;
				case SerializableType.RectOffsetName:
					RectOffset.GenerateTypeTree(context, name);
					break;
				case SerializableType.GUIStyleName:
					GUIStyle.GenerateTypeTree(context, name);
					break;
				case SerializableType.PropertyNameName:
					PropertyName.GenerateTypeTree(context, name);
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
