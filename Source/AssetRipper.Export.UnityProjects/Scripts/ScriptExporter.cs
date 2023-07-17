using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
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
				LanguageVersion = configuration.ScriptLanguageVersion.ToCSharpLanguageVersion(configuration.Version),
				ScriptContentLevel = configuration.ScriptContentLevel,
			};
			ExportMode = configuration.ScriptExportMode;
		}

		public IAssemblyManager AssemblyManager { get; }
		public ScriptExportMode ExportMode { get; }
		internal ScriptDecompiler Decompiler { get; }

		public bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IMonoScript script)
			{
				exportCollection = new ScriptExportCollection(this, script.Collection.Bundle);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
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
