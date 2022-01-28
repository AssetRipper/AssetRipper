using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public sealed class MonoScript : TextAsset, IMonoScript
	{
		public MonoScript(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasScript(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 1.5.0 to 2.6.0 and Not Release
		/// </summary>
		public static bool HasDefaultProperties(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(1, 5) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater and Not Release
		/// </summary>
		public static bool HasDefaultReferences(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.4.0 and greater and Not Release
		/// </summary>
		public static bool HasIcon(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.4.0 to 5.0.0 and Not Release
		/// </summary>
		public static bool HasEditorGraphData(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4) && version.IsLess(5);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasExecutionOrder(UnityVersion version) => version.IsGreaterEqual(3, 4);
		/// <summary>
		/// 3.4.0 and greater and Release
		/// </summary>
		public static bool HasPropertiesHash(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 4) && flags.IsRelease();
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasPathName(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasNamespace(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Less than 2018.1.2 or Release
		/// </summary>
		public static bool HasAssemblyName(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(2018, 1, 2);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasIsEditorScript(UnityVersion version) => version.IsLess(2018, 2);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsUInt32Hash(UnityVersion version) => version.IsLess(5);

		public override void Read(AssetReader reader)
		{
			ReadNamedObject(reader);

			if (HasExecutionOrder(reader.Version))
			{
				ExecutionOrder = reader.ReadInt32();
			}
			if (HasPropertiesHash(reader.Version, reader.Flags))
			{
				if (IsUInt32Hash(reader.Version))
				{
					uint hash = reader.ReadUInt32();
					m_PropertiesHash = new Hash128(hash);
				}
				else
				{
					m_PropertiesHash.Read(reader);
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
				AssemblyName = reader.ReadString();
			}
			if (HasIsEditorScript(reader.Version))
			{
				IsEditorScript = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
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

		private byte[] GetScript(UnityVersion version, TransferInstructionFlags flags)
		{
			return System.Array.Empty<byte>();
		}

		private IReadOnlyDictionary<string, PPtr<Object.Object>> GetDefaultReferences(UnityVersion version, TransferInstructionFlags flags)
		{
			return new Dictionary<string, PPtr<Object.Object>>(0);
		}

		private PPtr<Object.Object> GetIcon(UnityVersion version, TransferInstructionFlags flags)
		{
			return new();
		}

		private string GetNamespace(UnityVersion version)
		{
			return HasNamespace(version) ? Namespace : string.Empty;
		}

		private string GetAssemblyName(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasAssemblyName(version, flags) ? AssemblyName : string.Empty;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, "Scripts");
		public override string ExportExtension => "cs";

		public int ExecutionOrder { get; set; }
		public string ClassName { get; set; }
		public string Namespace { get; set; }
		/// <summary>
		/// AssemblyIdentifier previously
		/// </summary>
		public string AssemblyName { get; set; }
		public bool IsEditorScript { get; set; }

		public const string MonoScriptName = "MonoScript";
		public const string DefaultPropertiesName = "m_DefaultProperties";
		public const string DefaultReferencesName = "m_DefaultReferences";
		public const string IconName = "m_Icon";
		public const string ExecutionOrderName = "m_ExecutionOrder";
		public const string ClassNameName = "m_ClassName";
		public const string NamespaceName = "m_Namespace";
		public const string AssemblyNameName = "m_AssemblyName";
		public const string AssemblyIdentifierName = "m_AssemblyIdentifier";
		public const string IsEditorScriptName = "m_IsEditorScript";

		private Hash128 m_PropertiesHash = new();
		public Hash128 PropertiesHash => m_PropertiesHash;
	}
}
