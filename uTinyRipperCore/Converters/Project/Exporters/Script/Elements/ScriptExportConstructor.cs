using System.Collections.Generic;
using System.IO;

namespace uTinyRipper.Converters.Script
{
	public abstract class ScriptExportConstructor : ScriptExportMethod
	{
		public override void Export(TextWriter writer, int intent)
		{
			writer.WriteIndent(intent);
			writer.Write("{0} {1}(", Keyword, DeclaringType.Name);
			for (int i = 0; i < Parameters.Count; i++)
			{
				ScriptExportParameter parameter = Parameters[i];
				parameter.Export(writer, intent);
				if (i < Parameters.Count - 1)
				{
					writer.Write(", ");
				}
			}

			if (BaseParameters != null && BaseParameters.Count != 0)
			{
				writer.Write(") : base(");
				for (int i = 0; i < BaseParameters.Count; i++)
				{
					ScriptExportParameter parameter = BaseParameters[i];
					writer.Write("default({0})", parameter.Type.GetTypeNestedName(DeclaringType));
					if (i < BaseParameters.Count - 1)
					{
						writer.Write(", ");
					}
				}
			} 
			else if (DeclaringType.IsValueType)
			{
				// all field of a value type must be initialized
				// they always inherit System.ValueType which only has a parameterless constructor
				// so this is always written
				writer.Write(") : this(");
			}

			writer.WriteLine(")");
			writer.WriteIndent(intent);
			writer.WriteLine("{");
			writer.WriteIndent(intent);
			writer.WriteLine("}");
		}

		public override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			base.GetUsedNamespaces(namespaces);
			foreach (ScriptExportParameter parameter in BaseParameters)
			{
				parameter.GetUsedNamespaces(namespaces);
			}
		}
		
		public abstract IReadOnlyList<ScriptExportParameter> BaseParameters { get; }
	}
}
