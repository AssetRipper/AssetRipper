using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporters.Scripts;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class MonoScript : NamedObject
	{
		public MonoScript(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadScript(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadExecutionOrder(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadPathName(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadNamespace(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool IsReadIsEditorScript(Version version)
		{
			return version.IsLess(2018, 2);
		}		

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsUInt32Hash(Version version)
		{
			return version.IsLess(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if(version.IsGreaterEqual(2018, 2))
			{
				return 5;
			}
			if (version.IsGreaterEqual(3, 4))
			{
				return 4;
			}
			if (version.IsGreaterEqual(3))
			{
				return 3;
			}
			// unknown (beta) version
			// return 2;
			return 1;
		}

		public bool IsScriptPresents()
		{
			if (IsReadNamespace(File.Version))
			{
				return File.AssemblyManager.IsPresent(AssemblyName, Namespace, ClassName);
			}
			else
			{
				return File.AssemblyManager.IsPresent(AssemblyName, ClassName);
			}
		}

		public ScriptStructure CreateStructure()
		{
			if(IsReadNamespace(File.Version))
			{
				if (File.AssemblyManager.IsValid(AssemblyName, Namespace, ClassName))
				{
					return File.AssemblyManager.CreateStructure(AssemblyName, Namespace, ClassName);
				}
			}
			else
			{
				if (File.AssemblyManager.IsValid(AssemblyName, ClassName))
				{
					return File.AssemblyManager.CreateStructure(AssemblyName, ClassName);
				}
			}
			return null;
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager)
		{
			if(IsReadNamespace(File.Version))
			{
				return File.AssemblyManager.CreateExportType(exportManager, AssemblyName, Namespace, ClassName);
			}
			else
			{
				return File.AssemblyManager.CreateExportType(exportManager, AssemblyName, ClassName);
			}
		}

		public ScriptInfo GetScriptInfo()
		{
			return File.AssemblyManager.GetScriptInfo(AssemblyName, ClassName);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadScript(reader.Flags))
			{
				m_defaultReferences = new Dictionary<string, PPtr<Object>>();

				Script = reader.ReadString();
				m_defaultReferences.Read(reader);
				Icon.Read(reader);
			}

			if (IsReadExecutionOrder(reader.Version))
			{
				ExecutionOrder = reader.ReadInt32();
				if (IsUInt32Hash(reader.Version))
				{
					PropertiesHash = reader.ReadUInt32();
				}
				else
				{
					PropertiesHash128.Read(reader);
				}
			}

			if (IsReadPathName(reader.Version))
			{
				PathName = reader.ReadString();
			}
			ClassName = reader.ReadString();
			if (IsReadNamespace(reader.Version))
			{
				Namespace = reader.ReadString();
			}
			AssemblyNameOrigin = reader.ReadString();
			AssemblyName = FilenameUtils.FixAssemblyName(AssemblyNameOrigin);
			if (IsReadIsEditorScript(reader.Version))
			{
				IsEditorScript = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			if(IsReadScript(file.Flags))
			{
				foreach (PPtr<Object> reference in DefaultReferences.Values)
				{
					yield return reference.FetchDependency(file, isLog, ToLogString, "DefaultReferences");
				}
				yield return Icon.FetchDependency(file, isLog, ToLogString, "m_Icon");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsReadScript(container.Flags))
			{
				node.Add("m_Script", Script);
				node.Add("m_DefaultReferences", DefaultReferences.ExportYAML(container));
				node.Add("m_Icon", Icon.ExportYAML(container));
			}
			node.Add("m_ExecutionOrder", ExecutionOrder);
			node.Add("m_ClassName", ClassName);
			node.Add("m_Namespace", IsReadNamespace(container.Version) ? Namespace : string.Empty);
			node.Add("m_AssemblyName", AssemblyNameOrigin);
			node.Add("m_IsEditorScript", IsEditorScript);
			return node;
		}

		public override string ExportName => Path.Combine(AssetsKeyWord, "Scripts");
		public override string ExportExtension => "cs";

		public string Script { get; private set; }
		public IReadOnlyDictionary<string, PPtr<Object>> DefaultReferences => m_defaultReferences;
		public int ExecutionOrder { get; private set; }
		public uint PropertiesHash { get; private set; }
		public string PathName {get; private set; }
		public string ClassName { get; private set; }
		public string Namespace { get; private set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		public string AssemblyName { get; private set; }
		public string AssemblyNameOrigin { get; private set; }
		public bool IsEditorScript { get; private set; }

		public PPtr<Object> Icon;
		public Hash128 PropertiesHash128;

		private Dictionary<string, PPtr<Object>> m_defaultReferences;
	}
}
