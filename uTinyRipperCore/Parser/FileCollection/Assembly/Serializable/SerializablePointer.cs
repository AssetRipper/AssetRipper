using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Assembly
{
	public sealed class SerializablePointer : SerializableStructure
	{
		public SerializablePointer(SerializableType type):
			base(type, null, EmptyFields)
		{
		}

		public override ISerializableStructure CreateDuplicate()
		{
			return new SerializablePointer(Type);
		}

		public override void Read(AssetReader reader)
		{
			Pointer.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Pointer.FetchDependency(file, () => nameof(MonoBehaviour), ToString());
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			return Pointer.ExportYAML(container);
		}

		public PPtr<Object> Pointer;

		private static readonly SerializableField[] EmptyFields = new SerializableField[0];
	}
}
