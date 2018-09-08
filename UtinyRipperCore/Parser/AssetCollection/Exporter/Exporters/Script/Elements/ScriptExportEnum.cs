using System;
using System.IO;

namespace UtinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportEnum : ScriptExportType
	{
		public sealed override void Export(TextWriter writer, int intent)
		{
			writer.WriteIntent(intent);
			writer.WriteLine("{0} enum {1}", Keyword, TypeName);

			writer.WriteIntent(intent++);
			writer.WriteLine('{');
			
			foreach (ScriptExportField field in Fields)
			{
				field.ExportEnum(writer, intent);
			}

			writer.WriteIntent(--intent);
			writer.WriteLine('}');
		}

		public sealed override bool HasMember(string name)
		{
			throw new NotSupportedException();
		}

		public sealed override string ClearName => Name;
		public sealed override bool IsEnum => true;

		public sealed override ScriptExportType Base => null;

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => false;
	}
}
