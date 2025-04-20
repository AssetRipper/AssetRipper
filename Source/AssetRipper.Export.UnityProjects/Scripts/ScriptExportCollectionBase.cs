using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.SourceGenerated.Classes.ClassID_1035;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Scripts;

public abstract class ScriptExportCollectionBase : ExportCollection
{
	public sealed override ScriptExporter AssetExporter { get; }

	public IMonoScript FirstScript { get; }

	public sealed override AssetCollection File => FirstScript.Collection;

	public sealed override IEnumerable<IMonoScript> Assets => [FirstScript];

	public ScriptExportCollectionBase(ScriptExporter assetExporter, IMonoScript firstScript)
	{
		AssetExporter = assetExporter;
		FirstScript = firstScript;
	}

	public sealed override bool Contains(IUnityObjectBase asset)
	{
		return ReferenceEquals(asset, FirstScript);
	}

	public sealed override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		ThrowIfLocal(isLocal);
		ThrowIfNotAsset(asset);
		return AssetExporter.CreateExportPointer(FirstScript);

		[StackTraceHidden]
		static void ThrowIfLocal(bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}
		}
	}

	public sealed override long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		ThrowIfNotAsset(asset);
		return CreateExportPointer(container, asset, false).FileID;
	}

	[StackTraceHidden]
	private void ThrowIfNotAsset(IUnityObjectBase asset)
	{
		if (!ReferenceEquals(FirstScript, asset))
		{
			throw new ArgumentException($"The asset must be the same one referenced in this collection.", nameof(asset));
		}
	}

	protected static string GetScriptsFolderName(string assemblyName)
	{
		return assemblyName
			is "Assembly-CSharp-firstpass"
			or "Assembly - CSharp - firstpass"
			or "Assembly-UnityScript-firstpass"
			or "Assembly - UnityScript - firstpass"
			? "Plugins"
			: "Scripts";
	}

	protected static void GetExportSubPath(string assembly, string @namespace, string @class, out string folderPath, out string fileName)
	{
		string assemblyFolder = SpecialFileNames.RemoveAssemblyFileExtension(assembly);
		string scriptsFolder = GetScriptsFolderName(assemblyFolder);
		string namespaceFolder = @namespace.Replace('.', Path.DirectorySeparatorChar);
		folderPath = FileSystem.FixInvalidPathCharacters(Path.Join(scriptsFolder, assemblyFolder, namespaceFolder));
		fileName = $"{FileSystem.FixInvalidPathCharacters(@class)}.cs";
	}

	protected static void GetExportSubPath(IMonoScript script, out string folderPath, out string fileName)
	{
		GetExportSubPath(script.GetAssemblyNameFixed(), script.Namespace.String, script.GetNonGenericClassName(), out folderPath, out fileName);
	}

	private protected static void GetExportSubPath(MonoScriptInfo script, out string folderPath, out string fileName)
	{
		GetExportSubPath(script.Assembly, script.Namespace, script.Class, out folderPath, out fileName);
	}

	protected static void OnScriptExported(IExportContainer container, IMonoScript script, string path, FileSystem fileSystem)
	{
		IMonoImporter importer = MonoImporter.Create(script.Collection, container.ExportVersion);
		importer.ExecutionOrder = (short)script.ExecutionOrder;
		Meta meta = new Meta(ScriptHashing.CalculateScriptGuid(script), importer);
		ExportMeta(container, meta, path, fileSystem);
	}
}
