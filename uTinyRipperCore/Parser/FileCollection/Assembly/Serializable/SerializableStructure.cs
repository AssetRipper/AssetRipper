using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Assembly
{
	public class SerializableStructure : ISerializableStructure
	{
		internal SerializableStructure(SerializableType type, SerializableStructure @base, IReadOnlyList<SerializableField> fields)
		{
			Type = type ?? throw new ArgumentNullException(nameof(type));
			Base = @base;
			Fields = fields ?? throw new ArgumentNullException(nameof(fields));
		}

		protected SerializableStructure(SerializableStructure copy) :
			this(copy.Type, CreateBase(copy), CreateFields(copy))
		{
		}
		
		public static ISerializableStructure EngineTypeToScriptStructure(string name)
		{
			switch (name)
			{
				case SerializableType.Vector2Name:
					return new Vector2f();
				case SerializableType.Vector2IntName:
					return new Vector2i();
				case SerializableType.Vector3Name:
					return new Vector3f();
				case SerializableType.Vector3IntName:
					return new Vector3i();
				case SerializableType.Vector4Name:
					return new Vector4f();
				case SerializableType.RectName:
					return new Rectf();
				case SerializableType.BoundsName:
					return new AABB();
				case SerializableType.BoundsIntName:
					return new AABBi();
				case SerializableType.QuaternionName:
					return new Quaternionf();
				case SerializableType.Matrix4x4Name:
					return new Matrix4x4f();
				case SerializableType.ColorName:
					return new ColorRGBAf();
				case SerializableType.Color32Name:
					return new ColorRGBA32();
				case SerializableType.LayerMaskName:
					return new LayerMask();
				case SerializableType.AnimationCurveName:
					return new AnimationCurveTpl<Float>(true);
				case SerializableType.GradientName:
					return new Gradient();
				case SerializableType.RectOffsetName:
					return new RectOffset();
				case SerializableType.GUIStyleName:
					return new GUIStyle(true);

				case SerializableType.PropertyNameName:
					return new PropertyName();

				default:
					throw new NotImplementedException(name);
			}
		}

		protected static SerializableStructure CreateBase(SerializableStructure copy)
		{
			if (copy.Base == null)
			{
				return null;
			}

			return (SerializableStructure)copy.Base.CreateDuplicate();
		}

		protected static List<SerializableField> CreateFields(SerializableStructure copy)
		{
			List<SerializableField> fields = new List<SerializableField>();
			foreach (SerializableField field in copy.Fields)
			{
				SerializableField fieldCopy = field.CreateCopy();
				fields.Add(fieldCopy);
			}
			return fields;
		}

		public virtual ISerializableStructure CreateDuplicate()
		{
			return new SerializableStructure(this);
		}

		public virtual void Read(AssetReader reader)
		{
			if (Base != null)
			{
				Base.Read(reader);
			}

			foreach (SerializableField field in Fields)
			{
				field.Read(reader);
			}
		}

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = Base == null ? new YAMLMappingNode() : (YAMLMappingNode)Base.ExportYAML(container);
			foreach (SerializableField field in Fields)
			{
				node.Add(field.Name, field.ExportYAML(container));
			}
			return node;
		}

		public virtual IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			if (Base != null)
			{
				foreach (Object asset in Base.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}

			foreach (SerializableField field in Fields)
			{
				foreach (Object asset in field.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		/*public TypeTree ToTypeTree()
		{
			List<TypeTreeNode> nodes = new List<TypeTreeNode>();

			int index = 0;
			int size = CalculateSize(0);
			TypeTreeNode root = new TypeTreeNode(0, false, Name, "Base", size, index++, TransferMetaFlags.NoTransferFlags);
			nodes.Add(root);
			foreach (ScriptField field in Fields)
			{

			}

			return new TypeTree(nodes);
		}

		public int CalculateSize(int depth)
		{
			if (Type.Type == PrimitiveType.Complex)
			{
				int size = Base == null ? 0 : Base.CalculateSize(depth);
				if (size == -1)
				{
					return -1;
				}

				foreach (ScriptField field in Fields)
				{
					int fieldSize = field.CalculateSize(depth + 1);

				}
			}
			else
			{
				return Type.Type.GetSize();
			}
		}*/

		public override string ToString()
		{
			if (Type.Namespace == string.Empty)
			{
				return $"{Type.Name}";
			}
			else
			{
				return $"{Type.Namespace}.{Type.Name}";
			}
		}

		public SerializableType Type { get; }
		public SerializableStructure Base { get; }
		public IReadOnlyList<SerializableField> Fields { get; }
	}
}
