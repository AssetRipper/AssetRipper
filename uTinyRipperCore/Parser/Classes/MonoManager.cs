using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class MonoManager : GlobalGameManager
	{
		public MonoManager(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadHasCompileErrors(Version version)
		{
			return version.IsLess(3, 0, 0);
		}
		/// <summary>
		/// 1.6.0 to 3.0.0 exclusive
		/// </summary>
		public static bool IsReadCustomDlls(Version version)
		{
			return version.IsGreaterEqual(1, 6) && version.IsLess(3, 0, 0);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadAssemblyIdentifiers(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadAssemblyTypes(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private int GetSerializedVersion(Version version)
		{
			if(Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if(version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_scripts = reader.ReadAssetArray<PPtr<MonoScript>>();
			if(IsReadHasCompileErrors(reader.Version))
			{
				HasCompileErrors = reader.ReadBoolean();
				if(IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				EngineDllModDate.Read(reader);
			}
			if (IsReadCustomDlls(reader.Version))
			{
				m_customDlls = reader.ReadStringArray();
			}
			m_assemblyNames = reader.ReadStringArray();
			if(IsReadAssemblyIdentifiers(reader.Version))
			{
				m_assemblyIdentifiers = reader.ReadStringArray();
			}
			if (IsReadAssemblyTypes(reader.Version))
			{
				m_assemblyTypes = reader.ReadInt32Array();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<MonoScript> script in Scripts)
			{
				yield return script.FetchDependency(file, isLog, ToLogString, "m_Scripts");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<PPtr<MonoScript>> Scripts => m_scripts;
		public bool HasCompileErrors { get; private set; }
		public IReadOnlyList<string> CustomDlls => m_customDlls;
		public IReadOnlyList<string> AssemblyNames => m_assemblyNames;
		public IReadOnlyList<string> AssemblyIdentifiers => m_assemblyIdentifiers;
		public IReadOnlyList<int> AssemblyTypes => m_assemblyTypes;

		public DateTime EngineDllModDate;

		private PPtr<MonoScript>[] m_scripts;
		private string[] m_customDlls;
		private string[] m_assemblyNames;
		private string[] m_assemblyIdentifiers;
		private int[] m_assemblyTypes;
	}
}
