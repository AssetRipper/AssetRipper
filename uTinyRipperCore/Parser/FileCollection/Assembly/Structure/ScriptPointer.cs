using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Assembly
{
	public sealed class ScriptPointer : ScriptStructure
	{
		public ScriptPointer(ScriptType type):
			base(type, null, EmptyFields)
		{
		}

		public override IScriptStructure CreateDuplicate()
		{
			return new ScriptPointer(Type);
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

		private static readonly ScriptField[] EmptyFields = new ScriptField[0];
	}
}
