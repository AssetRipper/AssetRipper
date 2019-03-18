using Mono.Cecil;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Assembly
{
	public class ScriptPointer : ScriptStructure
	{
		public ScriptPointer(TypeReference type):
			base(type.Namespace, type.Name)
		{
		}

		protected ScriptPointer(ScriptPointer copy):
			base(copy.Namespace, copy.Name)
		{
			Pointer = copy.Pointer;
		}

		public override IScriptStructure CreateCopy()
		{
			return new ScriptPointer(this);
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
	}
}
