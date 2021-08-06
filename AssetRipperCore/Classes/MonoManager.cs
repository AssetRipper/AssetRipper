using AssetRipper.Core.Project;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using DateTime = AssetRipper.Core.Classes.Misc.DateTime;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

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
		/// 2017.1 and greater
		/// </summary>
		public static bool HasAssemblyTypes(UnityVersion version) => version.IsGreaterEqual(2017);

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

			Scripts = reader.ReadAssetArray<PPtr<MonoScript>>();
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
			AssemblyNames = reader.ReadStringArray();
			if (HasAssemblyIdentifiers(reader.Version))
			{
				AssemblyIdentifiers = reader.ReadStringArray();
			}
			if (HasAssemblyTypes(reader.Version))
			{
				AssemblyTypes = reader.ReadInt32Array();
			}
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Scripts, ScriptsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<MonoScript>[] Scripts { get; set; }
		public bool HasCompileErrors { get; set; }
		public string[] CustomDlls { get; set; }
		public string[] AssemblyNames { get; set; }
		public string[] AssemblyIdentifiers { get; set; }
		public int[] AssemblyTypes { get; set; }

		public const string ScriptsName = "m_Scripts";

		public DateTime EngineDllModDate;
	}
}
