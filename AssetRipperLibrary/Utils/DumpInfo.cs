using AssetRipper.Core;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.ArchiveFiles;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Parser.Files.WebFiles;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_116;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Library.Utils
{
	public class DumpInfo
	{
		public static void DumpAllFileInfo(string gameDir, string exportPath)
		{
			Util.PrepareExportDirectory(exportPath);
			foreach (string file in AllFilesInFolder(gameDir))
			{
				string ext = Path.GetExtension(file);
				if (ext != "" && ext != ".assets" && ext != ".unity3d" && ext != ".bundle")
				{
					continue;
				}

				string relPath = Util.GetRelativePath(file, gameDir);
				relPath = Path.GetDirectoryName(relPath);
				if (Directory.Exists(file))
				{

				}
				DumpFile(file, Path.Combine(exportPath, relPath));
			}
		}
		static void DumpFileInfo(SerializedFile container, string exportPath)
		{
			Console.WriteLine($"Dumping container {container.Name }");
			Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
			using StreamWriter sw = new($"{exportPath}.txt");
			WriteFileInfo(container, sw);
			sw.WriteLine("");
			DumpObjectInfo(container, sw);
			if (container.Name == "globalgamemanagers")
			{
				IBuildSettings? buildSettings = (IBuildSettings?)container.FetchAssets().FirstOrDefault(asset => asset is IBuildSettings);
				if (buildSettings != null)
				{
					sw.WriteLine("");
					DumpBuildSettings(buildSettings, sw);
				}
				IMonoManager? monoManager = (IMonoManager?)container.FetchAssets().FirstOrDefault(asset => asset is IMonoManager);
				if (monoManager != null)
				{
					sw.WriteLine("");
					DumpMonoManager(monoManager, sw);
				}
			}
		}
		static void DumpFileListInfo(FileList fileList, string exportPath)
		{
			Console.WriteLine($"Dumping FileList {Path.GetFileName(exportPath)}");
			Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
			using StreamWriter sw = new($"{exportPath}.txt");
			if (fileList is BundleFile bf)
			{
				DumpBundleFileInfo(bf, sw);
			}
			if (fileList is ArchiveFile af)
			{
				//TODO
				sw.WriteLine($"ArchiveFile");
			}
			if (fileList is WebFile wf)
			{
				//TODO
				sw.WriteLine($"WebFile");
			}
			sw.WriteLine($"ResourceFile count {fileList.ResourceFiles.Count}");
			foreach (ResourceFile resourceFile in fileList.ResourceFiles)
			{
				sw.WriteLine($"ResourceFile: Name {resourceFile.Name}");
			}
			sw.WriteLine($"");
			sw.WriteLine($"SerializedFile count {fileList.SerializedFiles.Count}");
			foreach (SerializedFile serializedFile in fileList.SerializedFiles)
			{
				sw.WriteLine("");
				WriteFileInfo(serializedFile, sw);
				sw.WriteLine("");
				DumpObjectInfo(serializedFile, sw);
			}
		}
		static void DumpBundleFileInfo(BundleFile bundleFile, StreamWriter sw)
		{
			sw.WriteLine("BundleFile");
			Core.Parser.Files.BundleFile.Parser.BundleMetadata metadata = bundleFile.Metadata;
			Core.Parser.Files.BundleFile.Header.BundleHeader header = bundleFile.Header;
			sw.WriteLine("  TODO");
		}
		static void DumpObjectInfo(SerializedFile file, StreamWriter sw)
		{
			sw.WriteLine("{0,-40}, {1,30}, {2,-32}, {3}, {4}, {5}, {6}",
				"Name", "ClassID", "GUID", "FileIndex", "PathID", "IsValid", "Extra");
			Dictionary<long, Core.Interfaces.IUnityObjectBase> lookup = file.FetchAssets().ToDictionary(a => a.PathID, a => a);
			foreach (Core.Interfaces.IUnityObjectBase asset in file.FetchAssets())
			{
				string name = Util.GetName(asset);
				PPtr<Core.Interfaces.IUnityObjectBase> pptr = asset.SerializedFile.CreatePPtr(asset);
				string extra = "";
				if (asset is IMonoScript ms)
				{
					string scriptName = $"[{ms.GetAssemblyNameFixed()}]";
					if (!string.IsNullOrEmpty(ms.Namespace_C115.String))
					{
						scriptName += $"{ms.Namespace_C115.String}.";
					}

					scriptName += $"{ms.ClassName_C115.String}:{HashToString(ms.GetPropertiesHash())}";
					extra = scriptName;
				}
				if (asset is IShader shader)
				{
					if (shader.Has_CompressedBlob_C48())
					{
						IEnumerable<Core.Classes.Shader.Enums.GpuProgramType.ShaderGpuProgramType> programTypes = shader.ReadBlobs()
							.SelectMany(b => b.SubPrograms)
							.Select(sp => sp.GetProgramType(file.Version))
							.Distinct();
						extra += string.Format("Platforms: {0} ProgramTypes: {1}",
							string.Join(", ", shader.Platforms_C48 ?? Array.Empty<uint>()),
							string.Join(", ", programTypes));
					}
					else
					{
						extra += "Platforms: Text";
					}

				}
				sw.WriteLine($"{name,-40}, {asset.ClassID,30}, {asset.GUID}, {pptr.FileIndex,9}, {asset.PathID,6}, {extra}");
			}
			sw.WriteLine();
			sw.WriteLine("Cannot parse");
			sw.WriteLine("{0,-6}, {1,-40}, {2,-6}, {3,-15}, {4,-8}, {5,-11}, {6,-9}, {7,-8}",
	"FileID", "ClassID", "TypeID", "ScriptTypeIndex", "Stripped", "IsDestroyed", "ByteStart", "ByteSize");
			foreach (Core.Parser.Files.SerializedFiles.Parser.ObjectInfo info in file.Metadata.Object)
			{
				if (lookup.ContainsKey(info.FileID))
				{
					continue;
				}

				sw.WriteLine($"{info.FileID,-6}, {info.ClassID,-40}, {info.TypeID,-6}, {info.ScriptTypeIndex,-15}, {info.Stripped,-8}, {info.IsDestroyed,-11}, {info.ByteStart,-9}, {info.ByteSize,-8}");
			}
		}
		static void UnknownFileType(object file, string filepath, string exportPath)
		{
			Console.WriteLine($"Unknown file {filepath}({file.GetType().Name })");
			Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
			using StreamWriter sw = new StreamWriter($"{exportPath}.err.txt");
			sw.WriteLine($"Can't dump file {file.GetType().FullName}");
		}
		public static List<string> AllFilesInFolder(string folder)
		{
			List<string> result = new List<string>();

			foreach (string f in Directory.GetFiles(folder))
			{
				result.Add(f);
			}

			foreach (string d in Directory.GetDirectories(folder))
			{
				result.AddRange(AllFilesInFolder(d));
			}
			return result;
		}
		static void WriteFileInfo(SerializedFile container, StreamWriter sw)
		{
			sw.WriteLine($"  File: {container.Name}");
			sw.WriteLine($"	File.Collection: {container.Collection}");
			sw.WriteLine($"	File.Platform: {container.Platform}");
			sw.WriteLine($"	File.Version: {container.Version}");
			foreach (Core.Parser.Files.SerializedFiles.Parser.FileIdentifier dep in container.Dependencies)
			{
				sw.WriteLine($"	File.Dependency: {dep}");
				sw.WriteLine($"	  Dependency.AssetPath: {dep.AssetPath}");
			}
			if (container.Metadata != null)
			{
				//TODO container.Metadata.Hierarchy
				bool SerializeTypeTrees = container.Metadata.EnableTypeTree;
				SerializedType[] Types = container.Metadata.Types;
				UnityVersion Version = container.Metadata.UnityVersion;
				Core.Parser.Files.BuildTarget Platform = container.Metadata.TargetPlatform;
				sw.WriteLine($"	File.Metadata.Hierarchy:");
				sw.WriteLine($"		Hierarchy.Version: {Version}");
				sw.WriteLine($"		Hierarchy.Platform: {Platform}");
				sw.WriteLine($"		Hierarchy.SerializeTypeTrees: {SerializeTypeTrees}");
				sw.WriteLine($"		Hierarchy.Types: {Types.Length}");
				if (Types.Length > 0)
				{
					sw.WriteLine("			{0,-18}, {1}, {2}, {3,-32}, {4,-32}, {5}",
						"ClassId", "IsStrippedType", "ScriptID", "ScriptHash", "TypeHash", "NodeCount");
				}
				foreach (SerializedType type in Types)
				{
					ClassIDType ClassID = type.TypeID;
					bool IsStrippedType = type.IsStrippedType;
					short ScriptID = type.ScriptTypeIndex;
					TypeTree Tree = type.OldType;
					Hash128 ScriptHash = type.ScriptID;
					Hash128 TypeHash = type.OldTypeHash;
					string nodeCount = Tree == null ? "Null" : Tree.Nodes.Count.ToString();
					sw.WriteLine("			{0,-18}, {1,14}, {2,8}, {3}, {4}, {5}",
						ClassID.ToString(), IsStrippedType, ScriptID, HashToString(ScriptHash), HashToString(TypeHash), nodeCount);
				}
			}
			else
			{
				sw.WriteLine($"	File.Metadata.Hierarchy: Null");
			}
			//TODO container.Metadata.Entries
			/*sw.WriteLine($"	File.Metadata.Entries: {container.Metadata.Entries.Length}");

			var factory = new AssetFactory();
			foreach (var entry in container.Metadata.Entries)
			{
				AssetInfo assetInfo = new AssetInfo(container, entry.PathID, entry.ClassID);
				Object asset = factory.CreateAsset(assetInfo);
				if (asset == null)
				{
					sw.WriteLine($"	  Unimplemented Asset: {entry.ClassID}, {entry.ScriptID}, {entry.TypeID}, {entry.PathID}, {entry.IsStripped}");
				}
			}*/
		}
		public static string HashToString(Hash128 hash)
		{
			byte[] data = BitConverter.GetBytes(hash.Data0)
				.Concat(BitConverter.GetBytes(hash.Data1))
				.Concat(BitConverter.GetBytes(hash.Data2))
				.Concat(BitConverter.GetBytes(hash.Data3))
				.ToArray();
			return BitConverter.ToString(data).Replace("-", "");
		}
		public static void DumpBuildSettings(IBuildSettings buildSettings, StreamWriter sw)
		{
			sw.WriteLine("BuildSettings");
			/*sw.WriteLine($"  Version: {buildSettings.Version_C141}");
			sw.WriteLine($"  Scenes {buildSettings.Scenes_C141.Length}");
			for (int i = 0; i < buildSettings.Scenes_C141.Length; i++)
			{
				string scene = buildSettings.Scenes_C141[i].String;
				sw.WriteLine($"	{i}: {scene}");
			}
			sw.WriteLine($"  PreloadedPlugins {buildSettings.PreloadedPlugins_C141}");
			for (int i = 0; i < buildSettings.PreloadedPlugins_C141.Length; i++)
			{
				string? preloadedPlugin = buildSettings.PreloadedPlugins_C141?[i].String;
				sw.WriteLine($"	{i}: {preloadedPlugin}");
			}
			sw.WriteLine($"  BuildTags {buildSettings.BuildTags_C141.Length}");
			for (int i = 0; i < buildSettings.BuildTags_C141.Length; i++)
			{
				string? buildTag = buildSettings.BuildTags_C141?[i].String;
				sw.WriteLine($"	{i}: {buildTag}");
			}
			sw.WriteLine($"  RuntimeClassHashes {buildSettings.RuntimeClassHashes.Count}");
			foreach (KeyValuePair<int, Hash128> kv in buildSettings.RuntimeClassHashes.OrderBy(kv => kv.Key))
			{
				sw.WriteLine($"	{kv.Key}: {HashToString(kv.Value)}");
			}
			sw.WriteLine($"  ScriptHashes {buildSettings.ScriptHashes.Count}");
			foreach (KeyValuePair<Hash128, Hash128> kv in buildSettings.ScriptHashes)
			{
				sw.WriteLine($"	{HashToString(kv.Key)}: {HashToString(kv.Value)}");
			}*/
		}
		public static void DumpMonoManager(IMonoManager monoManager, StreamWriter sw)
		{
			sw.WriteLine("MonoManager");
			/*sw.WriteLine($"  HasCompileErrors {monoManager.HasCompileErrors}");
			sw.WriteLine($"  EngineDllModDate {monoManager.EngineDllModDate}");
			sw.WriteLine($"  CustomDlls {monoManager.CustomDlls?.Length}");
			foreach (string dll in monoManager.CustomDlls ?? Array.Empty<string>())
			{
				sw.WriteLine($"    {dll}");
			}
			sw.WriteLine($"  AssemblyIdentifiers {monoManager.AssemblyIdentifiers?.Length}");
			foreach (string dll in monoManager.AssemblyIdentifiers ?? Array.Empty<string>())
			{
				sw.WriteLine($"    {dll}");
			}
			sw.WriteLine($"  AssemblyNames {monoManager.AssemblyNames?.Length}");
			foreach (string dll in monoManager.AssemblyNames ?? Array.Empty<string>())
			{
				sw.WriteLine($"    {dll}");
			}
			sw.WriteLine($"  AssemblyTypes {monoManager.AssemblyTypes?.Length}");
			foreach (int dll in monoManager.AssemblyTypes ?? Array.Empty<int>())
			{
				sw.WriteLine($"    {dll}");
			}
			sw.WriteLine($"  Scripts {monoManager.Scripts.Length}");
			foreach (PPtr<IMonoScript> dll in monoManager.Scripts ?? Array.Empty<PPtr<IMonoScript>>())
			{
				sw.WriteLine($"    {dll}");
			}*/
		}
		static void DumpFile(string filepath, string exportPath)
		{
			string filename = Path.GetFileName(filepath);
			try
			{
				object file = Util.LoadFile(filepath);
				if (file is SerializedFile serializedFile)
				{
					DumpFileInfo(serializedFile, Path.Combine(exportPath, filename));
				}
				if (file is BundleFile bundleFile)
				{
					DumpFileListInfo(bundleFile, Path.Combine(exportPath, filename));
				}
				if (file is ArchiveFile archiveFile)
				{
					DumpFileListInfo(archiveFile, Path.Combine(exportPath, filename));
				}
				if (file is WebFile webfile)
				{
					DumpFileListInfo(webfile, Path.Combine(exportPath, filename));
				}
				if (file is ResourceFile resourceFile)
				{
					UnknownFileType(resourceFile, filepath, Path.Combine(exportPath, filename));
				}
			}
			catch (Exception ex)
			{
				string errMessage = $"Error dumping file {filepath}\n{ex.ToString()}";
				Logger.Log(LogType.Error, LogCategory.General, errMessage);
				Directory.CreateDirectory(exportPath);
				File.WriteAllText($"{exportPath}/{filename}.err.txt", errMessage);
			}
		}
		public static void DumpTypeTree(string filePath, string exportPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
			object file = Util.LoadFile(filePath);
			List<SerializedFile> seralizedFiles = new List<SerializedFile>();
			if (file is SerializedFile sf)
			{
				seralizedFiles.Add(sf);
			}
			else if (file is BundleFile bundleFile)
			{
				seralizedFiles.AddRange(bundleFile.SerializedFiles);
			}
			else
			{
				throw new Exception();
			}
			using StreamWriter sw = new StreamWriter(exportPath);
			foreach (SerializedFile serializedFile in seralizedFiles)
			{
				DumpTypeInfo(serializedFile, sw);
			}
		}

		internal static void DumpTypeInfo(SerializedFile serializedFile, StreamWriter sw)
		{
			foreach (Core.Interfaces.IUnityObjectBase asset in serializedFile.FetchAssets().Where(asset => asset is IMonoScript))
			{
				IMonoScript monoScript = (IMonoScript)asset;
				sw.WriteLine($"\t[{monoScript.GetAssemblyNameFixed()}]{monoScript.Namespace_C115}.{monoScript.ClassName_C115} - {HashToString(monoScript.GetPropertiesHash())}");

			}
			sw.WriteLine($"SerializedFile");
			sw.WriteLine($"Name {serializedFile.Name}");
			sw.WriteLine($"NameOrigin {serializedFile.NameOrigin}");
			sw.WriteLine($"Platform {serializedFile.Platform}");
			sw.WriteLine($"Version {serializedFile.Version}");

			sw.WriteLine($"Preloads:");
			foreach (FileIdentifier ptr in serializedFile.Metadata.Externals)
			{
				sw.WriteLine($"\t{ptr}");
			}
			Core.Parser.Files.SerializedFiles.Parser.SerializedFileMetadata hierarchy = serializedFile.Metadata;
			sw.WriteLine($"TypeTree Version {hierarchy.UnityVersion}");
			sw.WriteLine($"TypeTree Platform {hierarchy.TargetPlatform}");
			bool SerializeTypeTrees = hierarchy.EnableTypeTree;
			sw.WriteLine($"TypeTree SerializeTypeTrees {SerializeTypeTrees}");
			sw.WriteLine($"");
			foreach (SerializedType type in hierarchy.Types)
			{
				ClassIDType ClassID = type.TypeID;
				short ScriptID = type.ScriptTypeIndex;
				bool IsStrippedType = type.IsStrippedType;
				TypeTree Tree = type.OldType;
				Hash128 ScriptHash = type.ScriptID;
				Hash128 TypeHash = type.OldTypeHash;

				IMonoScript? monoScript = serializedFile.FetchAssets().FirstOrDefault(asset => asset is IMonoScript ms && ms.GetPropertiesHash() == TypeHash) as IMonoScript;
				string scriptType = monoScript == null ? "\tNo Script" : $"\tMonoScript is [{monoScript.GetAssemblyNameFixed()}]{monoScript.Namespace_C115}.{monoScript.ClassName_C115}";
				sw.WriteLine(scriptType);
				sw.WriteLine($"\tType: ClassID {ClassID}, ScriptID {ScriptID}, IsStrippedType {IsStrippedType}, ScriptHash {HashToString(ScriptHash)}, TypeHash {HashToString(TypeHash)}");
				string Dump = Tree.Dump;
				sw.WriteLine($"\t{Dump}");
				sw.WriteLine($"");
			}
			sw.WriteLine($"");
		}
	}
}
