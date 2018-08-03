using System;
using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Classes.ParticleSystems;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
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

		public static bool IsSerializableType(string @namespace, string name)
		{
			if (ScriptType.IsString(@namespace, name))
			{
				return true;
			}
			if (ScriptType.IsList(@namespace, name))
			{
				return true;
			}

			if (ScriptType.IsEngineStruct(@namespace, name))
			{
				return true;
			}

			return false;
		}
		
		public static IScriptStructure EngineTypeToScriptStructure(string name)
		{
			switch (name)
			{
				case ScriptType.Vector2Name:
					return new Vector2f();
				case ScriptType.Vector3Name:
					return new Vector3f();
				case ScriptType.Vector4Name:
					return new Vector4f();
				case ScriptType.RectName:
					return new Rectf();
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

		public virtual void Read(AssetStream stream)
		{
			if(Base != null)
			{
				Base.Read(stream);
			}

			foreach (ScriptField field in Fields)
			{
				field.Read(stream);
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
