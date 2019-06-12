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
		public MonoScript(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// ? and greater and Not Release
		/// </summary>
		public static bool IsReadScript(Version version, TransferInstructionFlags flags)
		{
			// unknown version (somewhere between 1.5.0 and 2.5.0)
			return !flags.IsRelease()/* && version.IsGreaterEqual()*/;
		}
		/// <summary>
		/// 1.5.0 to 2.6.0 and Not Release
		/// </summary>
		public static bool IsReadDefaultProperties(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(1, 5) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater and Not Release
		/// </summary>
		public static bool IsReadDefaultReferences(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadIcon(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.4.0 to 5.0.0 and Not Release
		/// </summary>
		public static bool IsReadEditorGraphData(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4) && version.IsLess(5);
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
			if (version.IsGreaterEqual(2018, 2))
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
			ScriptIdentifier scriptID = IsReadNamespace(File.Version) ?
				File.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			return File.AssemblyManager.IsPresent(scriptID);
		}

		public SerializableType GetBehaviourType()
		{
			ScriptIdentifier scriptID = IsReadNamespace(File.Version) ?
				File.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			if (File.AssemblyManager.IsValid(scriptID))
			{
				return File.AssemblyManager.GetSerializableType(scriptID);
			}
			return null;
		}

		public ScriptExportType GetExportType(ScriptExportManager exportManager)
		{
			ScriptIdentifier scriptID = IsReadNamespace(File.Version) ?
				File.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			return File.AssemblyManager.GetExportType(exportManager, scriptID);
		}

		public ScriptIdentifier GetScriptID()
		{
			return File.AssemblyManager.GetScriptID(AssemblyName, ClassName);
		}

		public override void Read(AssetReader reader)
		{
			ReadBase(reader);

#if UNIVERSAL
			if (IsReadScript(reader.Version, reader.Flags))
			{
				Script = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadDefaultProperties(reader.Version, reader.Flags))
			{
				DefaultProperties.Read(reader);
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
			if (IsReadEditorGraphData(reader.Version, reader.Flags))
			{
				EditorGraphData.Read(reader);
			}
#endif

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

#if UNIVERSAL
			if (IsReadDefaultProperties(file.Version, file.Flags))
			{
				yield return DefaultProperties.FetchDependency(file, isLog, ToLogString, DefaultReferencesName);
			}
			if (IsReadDefaultReferences(file.Version, file.Flags))
			{
				foreach (PPtr<Object> reference in DefaultReferences.Values)
				{
					yield return reference.FetchDependency(file, isLog, ToLogString, DefaultReferencesName);
				}
			}
			if (IsReadIcon(file.Version, file.Flags))
			{
				yield return Icon.FetchDependency(file, isLog, ToLogString, IconName);
			}
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = ExportBaseYAMLRoot(container);
			node.Add(ScriptName, GetScript(container.Version, container.Flags).ExportYAML());
			node.Add(DefaultReferencesName, GetDefaultReferences(container.Version, container.Flags).ExportYAML(container));
			node.Add(IconName, GetIcon(container.Version, container.Flags).ExportYAML(container));
			node.Add(ExecutionOrderName, ExecutionOrder);
			node.Add(ClassNameName, ClassName);
			node.Add(NamespaceName, GetNamespace(container.Version));
			node.Add(AssemblyNameName, GetAssemblyName(container.Version, container.Flags));
			node.Add(IsEditorScriptName, IsEditorScript);
			return node;
		}

		private IReadOnlyList<byte> GetScript(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScript(version, flags))
			{
				return Script;
			}
#endif
			return new byte[0];
		}

		private IReadOnlyDictionary<string, PPtr<Object>> GetDefaultReferences(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadDefaultReferences(version, flags))
			{
				return DefaultReferences;
			}
#endif
			return new Dictionary<string, PPtr<Object>>(0);
		}

		private PPtr<Object> GetIcon(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadIcon(version, flags))
			{
				return Icon;
			}
#endif
			return default;
		}

		private string GetNamespace(Version version)
		{
			return IsReadNamespace(version) ? Namespace : string.Empty;
		}

		private string GetAssemblyName(Version version, TransferInstructionFlags flags)
		{
			return IsReadAssemblyName(version, flags) ? AssemblyNameOrigin : string.Empty;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, "Scripts");
		public override string ExportExtension => "cs";

#if UNIVERSAL
		public IReadOnlyDictionary<string, PPtr<Object>> DefaultReferences => m_defaultReferences;
#endif
		public int ExecutionOrder { get; private set; }
		public string ClassName { get; private set; }
		public string Namespace { get; private set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		public string AssemblyName { get; private set; }
		public string AssemblyNameOrigin { get; private set; }
		public bool IsEditorScript { get; private set; }

		public const string DefaultPropertiesName = "m_DefaultProperties";
		public const string DefaultReferencesName = "m_DefaultReferences";
		public const string IconName = "m_Icon";
		public const string ExecutionOrderName = "m_ExecutionOrder";
		public const string ClassNameName = "m_ClassName";
		public const string NamespaceName = "m_Namespace";
		public const string AssemblyNameName = "m_AssemblyName";
		public const string IsEditorScriptName = "m_IsEditorScript";

#if UNIVERSAL
		public PPtr<MonoBehaviour> DefaultProperties;
		public PPtr<Object> Icon;
		/// <summary>
		/// PPtr<MonoBehaviour> previously
		/// </summary>
		public PPtr<Object> EditorGraphData;
#endif
		public Hash128 PropertiesHash;

#if UNIVERSAL
		private Dictionary<string, PPtr<Object>> m_defaultReferences;
#endif
	}
}
