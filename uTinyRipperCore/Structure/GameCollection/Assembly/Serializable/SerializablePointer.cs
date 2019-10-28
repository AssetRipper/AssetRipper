using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Game.Assembly
{
	public sealed class SerializablePointer : SerializableStructure
	{
		public SerializablePointer(SerializableType type) :
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

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Pointer, string.Empty);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			return Pointer.ExportYAML(container);
		}

		public PPtr<Object> Pointer;

		private static readonly SerializableField[] EmptyFields = System.Array.Empty<SerializableField>();
	}
}
