using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Game.Assembly
{
	public sealed class SerializableStructure : IAsset, IDependent
	{
		internal SerializableStructure(SerializableType type, int depth)
		{
			Depth = depth;
			Type = type ?? throw new ArgumentNullException(nameof(type));
			Fields = new SerializableField[type.FieldCount];
		}

		public void Read(AssetReader reader)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					Fields[i].Read(reader, Depth, etalon);
				}
			}
		}

		public void Write(AssetWriter writer)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					Fields[i].Write(writer, etalon);
				}
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					node.Add(etalon.Name, Fields[i].ExportYAML(container, etalon));
				}
			}
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					foreach (PPtr<Object> asset in Fields[i].FetchDependencies(context, etalon))
					{
						yield return asset;
					}
				}
			}
		}

		public override string ToString()
		{
			if (Type.Namespace.Length == 0)
			{
				return $"{Type.Name}";
			}
			else
			{
				return $"{Type.Namespace}.{Type.Name}";
			}
		}

		private bool IsAvailable(in SerializableType.Field field)
		{
			if (Depth < MaxDepthLevel)
			{
				return true;
			}
			if (field.IsArray)
			{
				return false;
			}
			if (field.Type.Type	== PrimitiveType.Complex)
			{
				if (SerializableType.IsEngineStruct(field.Type.Namespace, field.Type.Name))
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public int Depth { get; }
		public SerializableType Type { get; }
		public SerializableField[] Fields { get; }

		public const int MaxDepthLevel = 8;
	}
}
