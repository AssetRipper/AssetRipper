using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Export.UnityProjects.Scripts
{
	public class ScriptExporter : IAssetExporter
	{
		public ScriptExporter(IAssemblyManager assemblyManager, LibraryConfiguration configuration)
		{
			AssemblyManager = assemblyManager;
			Decompiler = new ScriptDecompiler(AssemblyManager)
			{
				LanguageVersion = configuration.ExportSettings.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version),
				ScriptContentLevel = configuration.ImportSettings.ScriptContentLevel,
			};
			ExportMode = configuration.ExportSettings.ScriptExportMode;
		}

		public IAssemblyManager AssemblyManager { get; }
		public ScriptExportMode ExportMode { get; }
		internal ScriptDecompiler Decompiler { get; }
		private bool HasDecompiled { get; set; } = false;
		private static long MonoScriptDecompiledFileID { get; } = ExportIdHandler.GetMainExportID((int)ClassIDType.MonoScript);
		private static UnityGuid UnityEngineGUID => new UnityGuid(0x1F55507F, 0xA1948D44, 0x4080F528, 0xC176C90E);

		public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IMonoScript script)
			{
				if (HasDecompiled)
				{
					exportCollection = new SingleRedirectExportCollection(asset, CreateExportPointer(script));
				}
				else
				{
					HasDecompiled = true;
					if (AssemblyManager.IsSet)
					{
						exportCollection = new ScriptExportCollection(this, script);
					}
					else
					{
						exportCollection = new EmptyScriptExportCollection(this, script);
					}
				}
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public AssemblyExportType GetExportType(IMonoScript script)
		{
			return GetExportType(script.GetAssemblyNameFixed());
		}

		public MetaPtr CreateExportPointer(IMonoScript script)
		{
			return GetExportType(script) switch
			{
				AssemblyExportType.Decompile => new(MonoScriptDecompiledFileID, ScriptHashing.CalculateScriptGuid(script), AssetType.Meta),
				AssemblyExportType.Skip => new(ScriptHashing.CalculateScriptFileID(script), UnityEngineGUID, AssetType.Meta),
				_ => new(ScriptHashing.CalculateScriptFileID(script), ScriptHashing.CalculateAssemblyGuid(script), AssetType.Meta),
			};
		}

		public AssemblyExportType GetExportType(string assemblyName)
		{
			if (ReferenceAssemblies.IsReferenceAssembly(assemblyName))
			{
				return AssemblyExportType.Skip;
			}
			else if (!AssemblyManager.IsSet)
			{
				return AssemblyExportType.Decompile;
			}
			else if (ExportMode is ScriptExportMode.Decompiled)
			{
				return AssemblyExportType.Decompile;
			}
			else if (ExportMode is ScriptExportMode.Hybrid)
			{
				return ReferenceAssemblies.IsPredefinedAssembly(assemblyName)
					? AssemblyExportType.Decompile
					: AssemblyExportType.Save;
			}
			else
			{
				return AssemblyExportType.Save;
			}
		}

		public AssetType ToExportType(IUnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
