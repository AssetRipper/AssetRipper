using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporters.Scripts;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class MonoScript : TextAsset
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
		/// 2.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadDefaultReferences(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 5);
		}
		/// <summary>
		/// 3.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadIcon(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadExecutionOrder(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.4.0 and greater and Release
		/// </summary>
		public static bool IsReadPropertiesHash(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3, 4) && flags.IsRelease();
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
		/// Release or less than 2018.1.2
		/// </summary>
		public static bool IsReadAssemblyName(Version version, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				return true;
			}
			return version.IsLess(2018, 1, 2);
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
			ReadBase(reader);

			// unknown version
			if (IsReadScript(reader.Flags))
			{
				Script = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadDefaultReferences(reader.Version, reader.Flags))
			{
				m_defaultReferences = new Dictionary<string, PPtr<Object>>();
				m_defaultReferences.Read(reader);
			}
			if (IsReadIcon(reader.Version, reader.Flags))
			{
				Icon.Read(reader);
			}
			// TODO: 3.4.0 - 
			// PPtr<MonoBehaviour> m_EditorGraphData

			if (IsReadExecutionOrder(reader.Version))
			{
				ExecutionOrder = reader.ReadInt32();
			}
			if (IsReadPropertiesHash(reader.Version, reader.Flags))
			{
				if (IsUInt32Hash(reader.Version))
				{
					uint hash = reader.ReadUInt32();
					PropertiesHash = new Hash128(hash);
				}
				else
				{
					PropertiesHash.Read(reader);
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
			if (IsReadAssemblyName(reader.Version, reader.Flags))
			{
				AssemblyNameOrigin = reader.ReadString();
				AssemblyName = FilenameUtils.FixAssemblyName(AssemblyNameOrigin);
			}
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
			YAMLMappingNode node = ExportBaseYAMLRoot(container);
			node.Add(ScriptName, Script.ExportYAML());
			node.Add(DefaultReferencesName, DefaultReferences.ExportYAML(container));
			node.Add(IconName, Icon.ExportYAML(container));
			node.Add(ExecutionOrderName, ExecutionOrder);
			node.Add(ClassNameName, ClassName);
			node.Add(NamespaceName, IsReadNamespace(container.Version) ? Namespace : string.Empty);
			node.Add(AssemblyNameName, AssemblyNameOrigin);
			node.Add(IsEditorScriptName, IsEditorScript);
			return node;
		}

		public override string ExportName => Path.Combine(AssetsKeyWord, "Scripts");
		public override string ExportExtension => "cs";

		public IReadOnlyDictionary<string, PPtr<Object>> DefaultReferences => m_defaultReferences;
		public int ExecutionOrder { get; private set; }
		public string ClassName { get; private set; }
		public string Namespace { get; private set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		public string AssemblyName { get; private set; }
		public string AssemblyNameOrigin { get; private set; }
		public bool IsEditorScript { get; private set; }

		public const string DefaultReferencesName = "m_DefaultReferences";
		public const string IconName = "m_Icon";
		public const string ExecutionOrderName = "m_ExecutionOrder";
		public const string ClassNameName = "m_ClassName";
		public const string NamespaceName = "m_Namespace";
		public const string AssemblyNameName = "m_AssemblyName";
		public const string IsEditorScriptName = "m_IsEditorScript";

		public PPtr<Object> Icon;
		public Hash128 PropertiesHash;

		private Dictionary<string, PPtr<Object>> m_defaultReferences;
	}
}
