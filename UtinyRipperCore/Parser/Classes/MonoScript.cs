using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class MonoScript : NamedObject
	{
		public MonoScript(AssetInfo assetInfo):
			base(assetInfo)
		{
			if (IsReadScript(File.Platform))
			{
				m_defaultReferences = new Dictionary<string, PPtr<Object>>();
			}
		}

		/// <summary>
		/// Engine Package
		/// </summary>
		public static bool IsReadScript(Platform platform)
		{
			return platform == Platform.NoTarget;
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
		/// Less than 5.0.0
		/// </summary>
		private static bool IsUInt32Hash(Version version)
		{
			return version.IsLess(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 4;
			}
			
			if (version.IsGreaterEqual(3, 4))
			{
				return 4;
			}
#warning unknown:
			if (version.IsGreater(3, 0, 0, VersionType.Beta, 1))
			{
				return 3;
			}
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if(IsReadScript(stream.Platform))
			{
				m_defaultReferences = new Dictionary<string, PPtr<Object>>();

				Script = stream.ReadStringAligned();
				m_defaultReferences.Read(stream);
				Icon.Read(stream);
			}

			if (IsReadExecutionOrder(stream.Version))
			{
				ExecutionOrder = stream.ReadInt32();
				if (IsUInt32Hash(stream.Version))
				{
					PropertiesHash = stream.ReadUInt32();
				}
				else
				{
					PropertiesHash128.Read(stream);
				}
			}

			if (IsReadPathName(stream.Version))
			{
				PathName = stream.ReadStringAligned();
			}
			ClassName = stream.ReadStringAligned();
			if (IsReadNamespace(stream.Version))
			{
				Namespace = stream.ReadStringAligned();
			}
			AssemblyName = stream.ReadStringAligned();
			IsEditorScript = stream.ReadBoolean();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			if(IsReadScript(file.Platform))
			{
				foreach (PPtr<Object> ptr in DefaultReferences.Values)
				{
					Object @object = ptr.FindObject(file);
					if (@object == null)
					{
						if (isLog)
						{
							Logger.Log(LogType.Warning, LogCategory.Export, $"{ToLogString()} DefaultReferences {ptr.ToLogString(file)} wasn't found ");
						}
					}
					else
					{
						yield return @object;
					}
				}

				Object icon = Icon.FindObject(file);
				if (icon == null)
				{
					if (isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"{ToLogString()} m_Icon {icon.ToLogString()} wasn't found ");
					}
				}
				else
				{
					yield return icon;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			if (IsReadScript(exporter.Platform))
			{
				node.Add("m_Script", Script);
				node.Add("m_DefaultReferences", DefaultReferences.ExportYAML(exporter));
				node.Add("m_Icon", Icon.ExportYAML(exporter));
			}
			node.Add("m_ExecutionOrder", ExecutionOrder);
			node.Add("m_ClassName", ClassName);
			node.Add("m_Namespace", IsReadNamespace(exporter.Version) ? Namespace : string.Empty);
			node.Add("m_AssemblyName", AssemblyName);
			node.Add("m_IsEditorScript", IsEditorScript);
			return node;
		}

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
		public bool IsEditorScript { get; private set; }

		public PPtr<Object> Icon;
		public Hash128 PropertiesHash128;

		private Dictionary<string, PPtr<Object>> m_defaultReferences;
	}
}
