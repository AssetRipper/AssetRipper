using System.Collections.Generic;
using System.IO;
using uTinyRipper.Converters;
using uTinyRipper.Converters.Script;
using uTinyRipper.YAML;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Classes.Misc;
#if UNIVERSAL
using System.Linq;
#endif

namespace uTinyRipper.Classes
{
	public sealed class MonoScript : TextAsset
	{
		public MonoScript(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
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

		/// <summary>
		/// Not Release (NOTE: unknown version 1.5.0-2.5.0)
		/// </summary>
		public static bool HasScript(Version version, TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 1.5.0 to 2.6.0 and Not Release
		/// </summary>
		public static bool HasDefaultProperties(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(1, 5) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater and Not Release
		/// </summary>
		public static bool HasDefaultReferences(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.4.0 and greater and Not Release
		/// </summary>
		public static bool HasIcon(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.4.0 to 5.0.0 and Not Release
		/// </summary>
		public static bool HasEditorGraphData(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4) && version.IsLess(5);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasExecutionOrder(Version version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.4.0 and greater and Release
		/// </summary>
		public static bool HasPropertiesHash(Version version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 4) && flags.IsRelease();
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasPathName(Version version) => version.IsLess(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasNamespace(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Less than 2018.1.2 or Release
		/// </summary>
		public static bool HasAssemblyName(Version version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(2018, 1, 2);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasIsEditorScript(Version version) => version.IsLess(2018, 2);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsUInt32Hash(Version version) => version.IsLess(5);

		public bool IsScriptPresents()
		{
			ScriptIdentifier scriptID = HasNamespace(File.Version) ?
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			return File.Collection.AssemblyManager.IsPresent(scriptID);
		}

		public SerializableType GetBehaviourType()
		{
			ScriptIdentifier scriptID = HasNamespace(File.Version) ?
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			if (File.Collection.AssemblyManager.IsValid(scriptID))
			{
				return File.Collection.AssemblyManager.GetSerializableType(scriptID);
			}
			return null;
		}

		public ScriptExportType GetExportType(ScriptExportManager exportManager)
		{
			ScriptIdentifier scriptID = HasNamespace(File.Version) ?
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, Namespace, ClassName) :
				File.Collection.AssemblyManager.GetScriptID(AssemblyName, ClassName);
			return File.Collection.AssemblyManager.GetExportType(exportManager, scriptID);
		}

		public ScriptIdentifier GetScriptID()
		{
			return File.Collection.AssemblyManager.GetScriptID(AssemblyName, ClassName);
		}

		public override void Read(AssetReader reader)
		{
			ReadNamedObject(reader);

#if UNIVERSAL
			if (HasScript(reader.Version, reader.Flags))
			{
				Script = reader.ReadByteArray();
				reader.AlignStream();
			}
			if (HasDefaultProperties(reader.Version, reader.Flags))
			{
				DefaultProperties.Read(reader);
			}
			if (HasDefaultReferences(reader.Version, reader.Flags))
			{
				m_defaultReferences = new Dictionary<string, PPtr<Object>>();
				m_defaultReferences.Read(reader);
			}
			if (HasIcon(reader.Version, reader.Flags))
			{
				Icon.Read(reader);
			}
			if (HasEditorGraphData(reader.Version, reader.Flags))
			{
				EditorGraphData.Read(reader);
			}
#endif

			if (HasExecutionOrder(reader.Version))
			{
				ExecutionOrder = reader.ReadInt32();
			}
			if (HasPropertiesHash(reader.Version, reader.Flags))
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

			if (HasPathName(reader.Version))
			{
				PathName = reader.ReadString();
			}
			ClassName = reader.ReadString();
			if (HasNamespace(reader.Version))
			{
				Namespace = reader.ReadString();
			}
			if (HasAssemblyName(reader.Version, reader.Flags))
			{
				AssemblyNameOrigin = reader.ReadString();
				AssemblyName = FilenameUtils.FixAssemblyName(AssemblyNameOrigin);
			}
			if (HasIsEditorScript(reader.Version))
			{
				IsEditorScript = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

#if UNIVERSAL
			if (HasDefaultProperties(context.Version, context.Flags))
			{
				yield return context.FetchDependency(DefaultProperties, DefaultReferencesName);
			}
			if (HasDefaultReferences(context.Version, context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(DefaultReferences.Select(t => t.Value), DefaultReferencesName))
				{
					yield return asset;
				}
			}
			if (HasIcon(context.Version, context.Flags))
			{
				yield return context.FetchDependency(Icon, IconName);
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

		private byte[] GetScript(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasScript(version, flags))
			{
				return Script;
			}
#endif
			return System.Array.Empty<byte>();
		}

		private IReadOnlyDictionary<string, PPtr<Object>> GetDefaultReferences(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasDefaultReferences(version, flags))
			{
				return DefaultReferences;
			}
#endif
			return new Dictionary<string, PPtr<Object>>(0);
		}

		private PPtr<Object> GetIcon(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasIcon(version, flags))
			{
				return Icon;
			}
#endif
			return default;
		}

		private string GetNamespace(Version version)
		{
			return HasNamespace(version) ? Namespace : string.Empty;
		}

		private string GetAssemblyName(Version version, TransferInstructionFlags flags)
		{
			return HasAssemblyName(version, flags) ? AssemblyNameOrigin : string.Empty;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, "Scripts");
		public override string ExportExtension => "cs";

#if UNIVERSAL
		public IReadOnlyDictionary<string, PPtr<Object>> DefaultReferences => m_defaultReferences;
#endif
		public int ExecutionOrder { get; set; }
		public string ClassName { get; set; }
		public string Namespace { get; set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		public string AssemblyName { get; set; }
		public string AssemblyNameOrigin { get; set; }
		public bool IsEditorScript { get; set; }

		public const string DefaultPropertiesName = "m_DefaultProperties";
		public const string DefaultReferencesName = "m_DefaultReferences";
		public const string IconName = "m_Icon";
		public const string ExecutionOrderName = "m_ExecutionOrder";
		public const string ClassNameName = "m_ClassName";
		public const string NamespaceName = "m_Namespace";
		public const string AssemblyNameName = "m_AssemblyName";
		public const string AssemblyIdentifierName = "m_AssemblyIdentifier";
		public const string IsEditorScriptName = "m_IsEditorScript";

#if UNIVERSAL
		public PPtr<MonoBehaviour> DefaultProperties;
		/// <summary>
		/// PPtr<Texture> previously
		/// </summary>
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
