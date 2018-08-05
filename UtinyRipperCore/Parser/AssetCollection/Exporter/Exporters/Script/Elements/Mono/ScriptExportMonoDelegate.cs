using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.Exporters.Scripts.Mono
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
		}

		public static bool IsDelegate(TypeDefinition type)
		{
			if(type.BaseType == null)
			{
				return false;
			}
			return type.Namespace == SystemName && type.BaseType.Name == MulticastDelegateName;
		}

		public override void Init(IScriptExportManager manager)
		{
			m_return = CreateReturnType(manager);
			m_parameters = CreateParameterTypes(manager);

			m_module = Path.GetFileNameWithoutExtension(Type.Scope.Name);
			m_fullName = $"[{Module}]{Type.FullName}";
		}

		public override ScriptExportType GetContainer(IScriptExportManager manager)
		{
			if (Type.IsNested)
			{
				return manager.RetrieveType(Type.DeclaringType);
			}
			else
			{
				return this;
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

		public override string FullName => m_fullName;
		public override string Name => Type.Name;
		public override string Namespace => Type.Namespace;
		public override string Module => m_module;

		protected override ScriptExportType Return => m_return;
		protected override IReadOnlyList<ScriptExportParameter> Parameters => m_parameters;

		protected override string Keyword
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

		private ScriptExportType m_return;
		private IReadOnlyList<ScriptExportParameter> m_parameters;
		private string m_fullName;
		private string m_module;
	}
}
