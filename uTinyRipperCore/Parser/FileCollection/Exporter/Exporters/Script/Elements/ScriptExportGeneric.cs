using System;
using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.Exporters.Scripts
{
	public abstract class ScriptExportGeneric : ScriptExportType
	{
		public sealed override void GetTypeNamespaces(ICollection<string> namespaces)
		{
			base.GetTypeNamespaces(namespaces);
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
		}

		public sealed override void GetUsedNamespaces(ICollection<string> namespaces)
		{
			foreach (ScriptExportType argument in Arguments)
			{
				argument.GetTypeNamespaces(namespaces);
			}
			Template.GetUsedNamespaces(namespaces);
		}
		private bool IsConcreteArgument(ScriptExportType argument)
		{
			return m_manager.TypeNames.Contains($"{argument.Namespace}.{argument.CleanNestedName}");
		}
		public override string GetTypeQualifiedName(ScriptExportType relativeType)
		{
			/*
			 * TODO: Generic Arguments won't use the nested typename in the following case
			 *	public class Example : Base<Nested>
			 *	{
			 *		class Nested
			 *		{
			 *
			 *		}
			 *	}
			 *
			 *	Should be Example : Base<Example.Nested>
			 *	Affected Games: BattleTech
			 */
			if (Arguments.Count == 0)
			{
				return base.GetTypeQualifiedName(relativeType);
			}
			string templateName = Template.GetTypeQualifiedName(relativeType);
			int index = templateName.IndexOf('<');
			//Note that Template.TypeName contains ` while NestedName contains <
			if (index == -1) index = templateName.IndexOf('`');
			if (index == -1)
			{
				//Generic arguments are part of declaring class name
				return templateName;
			}
			StringBuilder sb = new StringBuilder(templateName, 0, index, 50 + index);
			sb.Append("<");
			for (int i = 0; i < Arguments.Count; i++)
			{
				ScriptExportType argument = Arguments[i];
				string argumentName = IsConcreteArgument(argument) ? argument.GetTypeQualifiedName(relativeType) : argument.TypeName;
				sb.Append(argumentName);
				if (i < Arguments.Count - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append(">");
			return sb.ToString();
		}

		public sealed override string CleanNestedName => Template.CleanNestedName;
		public sealed override bool IsEnum => Template.IsEnum;

		public sealed override ScriptExportType DeclaringType => Template.DeclaringType;
		public abstract ScriptExportType Template { get; } 
		public abstract IReadOnlyList<ScriptExportType> Arguments { get; }

		public sealed override IReadOnlyList<ScriptExportMethod> Methods { get; } = new ScriptExportMethod[0];
		public sealed override IReadOnlyList<ScriptExportProperty> Properties { get; } = new ScriptExportProperty[0];
		public sealed override IReadOnlyList<ScriptExportField> Fields { get; } = new ScriptExportField[0];

		protected sealed override string Keyword => throw new NotSupportedException();

		protected sealed override bool IsStruct => throw new NotSupportedException();
		protected sealed override bool IsSerializable => throw new NotSupportedException();
	}
}
