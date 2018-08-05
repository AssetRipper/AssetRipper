using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportEnum : ScriptExportType
	{
		public override void Export(TextWriter writer, int intent)
		{
			writer.WriteIntent(intent);
			writer.WriteLine("{0} enum {1}", Keyword, Name);

			writer.WriteIntent(intent++);
			writer.WriteLine('{');
			
			foreach (ScriptExportField field in Fields)
			{
				field.ExportEnum(writer, intent);
			}

			writer.WriteIntent(--intent);
			writer.WriteLine('}');
		}

		public sealed override bool IsEnum => true;

		protected sealed override ScriptExportType Base => null;
		
		public override IReadOnlyList<ScriptExportType> GenericArguments { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportType> NestedTypes { get; } = new ScriptExportType[0];
		public override IReadOnlyList<ScriptExportEnum> NestedEnums { get; } = new ScriptExportEnum[0];
		public override IReadOnlyList<ScriptExportDelegate> Delegates { get; } = new ScriptExportDelegate[0];

		protected override bool IsStruct => throw new NotSupportedException();
		protected override bool IsSerializable => false;
	}
}
