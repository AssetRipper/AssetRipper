using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct PropertyName : IScriptStructure
	{
		public PropertyName(PropertyName copy)
		{
			ID = copy.ID;
		}

		public static bool operator ==(PropertyName lhs, PropertyName rhs)
		{
			return lhs.ID == rhs.ID;
		}

		public static bool operator !=(PropertyName lhs, PropertyName rhs)
		{
			return lhs.ID != rhs.ID;
		}

		public IScriptStructure CreateCopy()
		{
			return new PropertyName(this);
		}

		public void Read(AssetReader reader)
		{
			ID = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(ID == 0 ? string.Empty : $"Unknown_{unchecked((uint)ID)}");
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return ID;
		}

		public override bool Equals(object other)
		{
			if (other is PropertyName propertyName)
			{
				return propertyName == this;
			}
			return false;
		}

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.PropertyNameName;

		public int ID { get; private set; }
	}
}
