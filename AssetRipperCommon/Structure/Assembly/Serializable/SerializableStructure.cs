using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Structure.Assembly.Serializable
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					foreach (PPtr<IUnityObjectBase> asset in Fields[i].FetchDependencies(context, etalon))
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
			if (field.Type.Type == PrimitiveType.Complex)
			{
				if (MonoUtils.IsEngineStruct(field.Type.Namespace, field.Type.Name))
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
