using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportDelegate : ScriptExportType
	{
		public override void Export(TextWriter writer, int intent)
		{
			writer.WriteIntent(intent);
			writer.Write("{0} delegate {1} {2}(", Keyword, Return.Name, Name);
			for (int i = 0; i < Parameters.Count; i++)
			{
				ScriptExportParameter parameter = Parameters[i];
				parameter.Export(writer, intent);
				if(i < Parameters.Count - 1)
				{
					writer.Write(',');
				}
			}
			writer.WriteLine(");");
		}

		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			GetTypeNamespaces(namespaces);
			Return.GetTypeNamespaces(namespaces);
			foreach (ScriptExportParameter parameter in Parameters)
			{
				parameter.GetUsedNamespaces(namespaces);
			}
		}

		protected override ScriptExportType Base => null;
		
		public override IReadOnlyList<ScriptExportType> GenericArguments { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportType> NestedTypes { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportEnum> NestedEnums { get; } = new ScriptExportEnum[0];
		public override IReadOnlyList<ScriptExportDelegate> Delegates { get; } = new ScriptExportDelegate[0];
		public override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected abstract ScriptExportType Return { get; }
		protected abstract IReadOnlyList<ScriptExportParameter> Parameters { get; }

		protected override bool IsStruct => false;
		protected override bool IsSerializable => false;

		protected const string SystemName = "System";

		protected const string MulticastDelegateName = "MulticastDelegate";
		protected const string InvokeMethodName = "Invoke";
	}
}
