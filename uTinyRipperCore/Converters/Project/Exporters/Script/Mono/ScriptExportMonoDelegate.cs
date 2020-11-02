using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace uTinyRipper.Converters.Script.Mono
{
	public sealed class ScriptExportMonoDelegate : ScriptExportDelegate
	{
		public ScriptExportMonoDelegate(TypeDefinition @delegate)
		{
			if (@delegate == null)
			{
				throw new ArgumentNullException(nameof(@delegate));
			}
			if (!IsDelegate(@delegate))
			{
				throw new Exception("Type isn't delegate");
			}

			Type = @delegate;

			NestedName = ScriptExportMonoType.GetNestedName(Type);
			Module = ScriptExportMonoType.GetModuleName(Type);
			FullName = ScriptExportMonoType.GetFullName(Type, Module);
		}

		public static bool IsDelegate(TypeDefinition type)
		{
			if (type.BaseType == null)
			{
				return false;
			}
			return type.BaseType.Namespace == SystemName && type.BaseType.Name == MulticastDelegateName;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_return = CreateReturnType(manager);
			m_parameters = CreateParameterTypes(manager);

			if (Type.IsNested)
			{
				m_declaringType = manager.RetrieveType(Type.DeclaringType);
				AddAsNestedDelegate();
			}
		}

		private ScriptExportType CreateReturnType(IScriptExportManager manager)
		{
			foreach (MethodDefinition method in Type.Methods)
			{
				if (method.Name == InvokeMethodName)
				{
					return manager.RetrieveType(method.ReturnType);
				}
			}
			throw new Exception($"Invoke method '{InvokeMethodName}' wasn't found");
		}

		private IReadOnlyList<ScriptExportParameter> CreateParameterTypes(IScriptExportManager manager)
		{
			foreach (MethodDefinition method in Type.Methods)
			{
				if (method.Name == InvokeMethodName)
				{
					ScriptExportParameter[] parameters = new ScriptExportParameter[method.Parameters.Count];
					for (int i = 0; i < parameters.Length; i++)
					{
						parameters[i] = manager.RetrieveParameter(method.Parameters[i]);
					}
					return parameters;
				}
			}
			throw new Exception($"Invoke method '{InvokeMethodName}' wasn't found");
		}

		public override string FullName { get; }
		public override string CleanName => Type.Name;
		public override string NestedName { get; }
		public override string TypeName => Type.Name;
		public override string Namespace => DeclaringType == null ? Type.Namespace : DeclaringType.Namespace;
		public override string Module { get; }

		public override ScriptExportType DeclaringType => m_declaringType;
		public override ScriptExportType Return => m_return;
		public override IReadOnlyList<ScriptExportParameter> Parameters => m_parameters;

		public override string Keyword
		{
			get
			{
				if (Type.IsPublic || Type.IsNestedPublic)
				{
					return PublicKeyWord;
				}
				if (Type.IsNestedPrivate)
				{
					return PrivateKeyWord;
				}
				if (Type.IsNestedFamily)
				{
					return ProtectedKeyWord;
				}
				return InternalKeyWord;
			}
		}

		private TypeDefinition Type { get; }

		private ScriptExportType m_declaringType;
		private ScriptExportType m_return;
		private IReadOnlyList<ScriptExportParameter> m_parameters;
	}
}
