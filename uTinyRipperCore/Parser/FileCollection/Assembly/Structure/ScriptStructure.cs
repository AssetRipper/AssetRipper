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
	public class ScriptStructure : IScriptStructure
	{
		internal ScriptStructure(ScriptType type, ScriptStructure @base, IReadOnlyList<ScriptField> fields)
		{
			Type = type ?? throw new ArgumentNullException(nameof(type));
			Base = @base;
			Fields = fields ?? throw new ArgumentNullException(nameof(fields));
		}

		protected ScriptStructure(ScriptStructure copy) :
			this(copy.Type, CreateBase(copy), CreateFields(copy))
		{
		}
		
		public static IScriptStructure EngineTypeToScriptStructure(string name)
		{
			switch (name)
			{
				case ScriptType.Vector2Name:
					return new Vector2f();
				case ScriptType.Vector2IntName:
					return new Vector2i();
				case ScriptType.Vector3Name:
					return new Vector3f();
				case ScriptType.Vector3IntName:
					return new Vector3i();
				case ScriptType.Vector4Name:
					return new Vector4f();
				case ScriptType.RectName:
					return new Rectf();
				case ScriptType.BoundsName:
					return new AABB();
				case ScriptType.BoundsIntName:
					return new AABBi();
				case ScriptType.QuaternionName:
					return new Quaternionf();
				case ScriptType.Matrix4x4Name:
					return new Matrix4x4f();
				case ScriptType.ColorName:
					return new ColorRGBAf();
				case ScriptType.Color32Name:
					return new ColorRGBA32();
				case ScriptType.LayerMaskName:
					return new LayerMask();
				case ScriptType.AnimationCurveName:
					return new AnimationCurveTpl<Float>(true);
				case ScriptType.GradientName:
					return new Gradient();
				case ScriptType.RectOffsetName:
					return new RectOffset();
				case ScriptType.GUIStyleName:
					return new GUIStyle(true);

				case ScriptType.PropertyNameName:
					return new PropertyName();

				default:
					throw new NotImplementedException(name);
			}
		}

		protected static ScriptStructure CreateBase(ScriptStructure copy)
		{
			if (copy.Base == null)
			{
				return null;
			}

			return (ScriptStructure)copy.Base.CreateDuplicate();
		}

		protected static List<ScriptField> CreateFields(ScriptStructure copy)
		{
			List<ScriptField> fields = new List<ScriptField>();
			foreach (ScriptField field in copy.Fields)
			{
				ScriptField fieldCopy = field.CreateCopy();
				fields.Add(fieldCopy);
			}
			return fields;
		}

		public virtual IScriptStructure CreateDuplicate()
		{
			return new ScriptStructure(this);
		}

		public virtual void Read(AssetReader reader)
		{
			if (Base != null)
			{
				Base.Read(reader);
			}

			foreach (ScriptField field in Fields)
			{
				field.Read(reader);
			}
		}

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = Base == null ? new YAMLMappingNode() : (YAMLMappingNode)Base.ExportYAML(container);
			foreach (ScriptField field in Fields)
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

			foreach (ScriptField field in Fields)
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

		public ScriptType Type { get; }
		public ScriptStructure Base { get; }
		public IReadOnlyList<ScriptField> Fields { get; }
	}
}
