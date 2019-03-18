using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;
using uTinyRipper.AssetExporters;

namespace uTinyRipper.Assembly
{
	public abstract class ScriptStructure : IScriptStructure
	{
		protected ScriptStructure(string @namespace, string name, IScriptStructure @base, IEnumerable<IScriptField> fields) :
			this(@namespace, name)
		{
			if (fields == null)
			{
				throw new ArgumentNullException(nameof(fields));
			}

			Base = @base;
			m_fields.AddRange(fields);
		}

		protected ScriptStructure(string @namespace, string name)
		{
			if (@namespace == null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			Namespace = @namespace;
			Name = name;
		}

		protected ScriptStructure(ScriptStructure copy) :
			this(copy.Namespace, copy.Name, CreateBase(copy), CreateFields(copy))
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

		protected static IScriptStructure CreateBase(ScriptStructure copy)
		{
			if (copy.Base == null)
			{
				return null;
			}

			return copy.Base.CreateCopy();
		}

		protected static IEnumerable<IScriptField> CreateFields(ScriptStructure copy)
		{
			List<IScriptField> fields = new List<IScriptField>();
			foreach (ScriptField cfield in copy.Fields)
			{
				IScriptField field = cfield.CreateCopy();
				fields.Add(field);
			}
			return fields;
		}

		public abstract IScriptStructure CreateCopy();

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

			foreach (ScriptField field in m_fields)
			{
				foreach (Object asset in field.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public override string ToString()
		{
			if (Namespace == string.Empty)
			{
				return $"{Name}";
			}
			else
			{
				return $"{Namespace}.{Name}";
			}
		}

		public IScriptStructure Base { get; }
		public IReadOnlyList<IScriptField> Fields => m_fields;

		public string Namespace { get; }
		public string Name { get; }

		private readonly List<IScriptField> m_fields = new List<IScriptField>();
	}
}
