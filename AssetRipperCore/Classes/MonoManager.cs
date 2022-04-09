using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using DateTime = AssetRipper.Core.Classes.Misc.DateTime;


namespace AssetRipper.Core.Classes
{
	public sealed class MonoManager : GlobalGameManager
	{
		public MonoManager(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasHasCompileErrors(UnityVersion version) => version.IsLess(3, 0, 0);
		/// <summary>
		/// 1.6.0 to 3.0.0 exclusive
		/// </summary>
		public static bool HasCustomDlls(UnityVersion version) => version.IsGreaterEqual(1, 6) && version.IsLess(3, 0, 0);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasAssemblyIdentifiers(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// Less than 2020.2
		/// </summary>
		public static bool HasAssemblyNames(UnityVersion version) => version.IsLess(2020, 2);
		/// <summary>
		/// 2017.1 and greater but less than 2020.2
		/// </summary>
		public static bool HasAssemblyTypes(UnityVersion version) => version.IsGreaterEqual(2017) && version.IsLess(2020, 2);
		/// <summary>
		/// At least 2020.2
		/// </summary>
		public static bool HasScriptHashes(UnityVersion version) => version.IsGreaterEqual(2020, 2);
		/// <summary>
		/// At least 2020.2
		/// </summary>
		public static bool HasRuntimeClassHashes(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

		private int GetSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasScriptHashes(reader.Version))
			{
				ScriptHashes = new Dictionary<Hash128, Hash128>();
				ScriptHashes.Read(reader);
			}

			if (HasRuntimeClassHashes(reader.Version))
			{
				RuntimeClassHashes = new Dictionary<uint, Hash128>();
				RuntimeClassHashes.Read(reader);
			}

			Scripts = reader.ReadAssetArray<PPtr<IMonoScript>>();
			if (HasHasCompileErrors(reader.Version))
			{
				HasCompileErrors = reader.ReadBoolean();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream();
				}

				EngineDllModDate.Read(reader);
			}
			if (HasCustomDlls(reader.Version))
			{
				CustomDlls = reader.ReadStringArray();
			}

			if (HasAssemblyNames(reader.Version))
			{
				AssemblyNames = reader.ReadStringArray();
			}

			if (HasAssemblyIdentifiers(reader.Version))
			{
				AssemblyIdentifiers = reader.ReadStringArray();
			}
			if (HasAssemblyTypes(reader.Version))
			{
				AssemblyTypes = reader.ReadInt32Array();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Scripts, ScriptsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<IMonoScript>[] Scripts { get; set; }
		public bool HasCompileErrors { get; set; }
		public string[] CustomDlls { get; set; }
		public string[] AssemblyNames { get; set; }
		public string[] AssemblyIdentifiers { get; set; }
		public int[] AssemblyTypes { get; set; }

		public Dictionary<Hash128, Hash128> ScriptHashes { get; set; }

		public Dictionary<uint, Hash128> RuntimeClassHashes { get; set; }

		public const string ScriptsName = "m_Scripts";

		public DateTime EngineDllModDate = new();
	}
}
