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

		protected sealed override bool HasMemberInner(string name)
		{
			throw new NotSupportedException();
		}

		public sealed override bool IsEnum => true;

		public sealed override ScriptExportType Base => null;

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => false;
	}
}
