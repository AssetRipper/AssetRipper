using AssetRipper.Extensions;
using AssetRipper.Structure.Assembly.Mono;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Project.Exporters.Script.Elements
{
	public abstract class ScriptExportEnum : ScriptExportType
	{
		public sealed override void Export(TextWriter writer, int indent)
		{
			writer.WriteIndent(indent);
			writer.Write("{0} enum {1}", Keyword, TypeName);
			if (Base != null && Base.TypeName != MonoUtils.IntName)
			{
				writer.Write(" : {0}", Base.TypeName);
			}
			writer.WriteLine();

			writer.WriteIndent(indent++);
			writer.WriteLine('{');

			foreach (ScriptExportField field in Fields)
			{
				field.ExportEnum(writer, indent);
			}

			writer.WriteIndent(--indent);
			writer.WriteLine('}');
		}

		public sealed override bool HasMember(string name)
		{
			throw new NotSupportedException();
		}

		public sealed override bool IsEnum => true;

		public sealed override ScriptExportConstructor Constructor => null;
		public sealed override IReadOnlyList<ScriptExportProperty> Properties { get; } = Array.Empty<ScriptExportProperty>();
		public sealed override IReadOnlyList<ScriptExportMethod> Methods { get; } = Array.Empty<ScriptExportMethod>();

		public sealed override bool IsStruct => throw new NotSupportedException();
		public sealed override bool IsSerializable => false;
	}
}
