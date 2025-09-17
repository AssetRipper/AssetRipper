using AssetRipper.DocExtraction.DataStructures;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;
using System.Diagnostics;

namespace AssetRipper.DocExtraction.ConsoleApp;

internal static class Program
{
	private static readonly UnityVersion MinimumUnityVersion = new UnityVersion(3, 5);
	static void Main(string[] args)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();

		//ExtractAndSaveIndividually(@"G:\TypeTreeDumps\UnityInstallations", @"E:\UnityDocumentation");
		ExtractAndSaveConsolidated(@"G:\TypeTreeDumps\UnityInstallations", @"consolidated.json");

		stopwatch.Stop();
		Console.WriteLine($"Finished in {stopwatch.ElapsedMilliseconds} ms");
	}

	private static void ExtractDocumentation2020()
	{
		string engineXmlPath = @"C:\Users\jrpri\Documents\UnityInstallations\2020.2.0f1\Editor\Data\Managed\UnityEngine.xml";
		string editorXmlPath = @"C:\Users\jrpri\Documents\UnityInstallations\2020.2.0f1\Editor\Data\Managed\UnityEditor.xml";
		string engineDllPath = @"C:\Users\jrpri\Documents\UnityInstallations\2020.2.0f1\Editor\Data\Managed\UnityEngine.dll";
		string editorDllPath = @"C:\Users\jrpri\Documents\UnityInstallations\2020.2.0f1\Editor\Data\Managed\UnityEditor.dll";
		UnityVersion unityVersion = new UnityVersion(2020, 2, 0, UnityVersionType.Final, 1);
		string outputDirectory = Environment.CurrentDirectory;
		string versionString = unityVersion.LessThan(5) ? unityVersion.ToStringWithoutType() : unityVersion.ToString();
		DocumentationFile documentationFile = DocumentationExtractor.ExtractDocumentation(versionString, engineXmlPath, editorXmlPath, engineDllPath, editorDllPath);
		documentationFile.SaveAsJson(Path.Combine(outputDirectory, $"{versionString}.json"));
	}

	private static void ExtractAndSaveIndividually(string inputDirectory, string outputDirectory)
	{
		Directory.CreateDirectory(outputDirectory);
		foreach (DocumentationFile documentationFile in ExtractAllDocumentation(inputDirectory))
		{
			documentationFile.SaveAsJson(Path.Combine(outputDirectory, $"{documentationFile.UnityVersion}.json"));
			Console.WriteLine(documentationFile.UnityVersion);
		}
	}

	private static void ExtractAndSaveConsolidated(string inputDirectory, string outputPath)
	{
		HistoryFile historyFile = new();
		Dictionary<string, ClassHistory> classes = historyFile.Classes;
		Dictionary<string, EnumHistory> enums = historyFile.Enums;
		Dictionary<string, StructHistory> structs = historyFile.Structs;
		foreach (DocumentationFile documentationFile in ExtractAllDocumentation(inputDirectory))
		{
			UnityVersion version = UnityVersion.Parse(documentationFile.UnityVersion);

			ProcessListIntoDictionary<ClassHistory, DataMemberHistory, ClassDocumentation, DataMemberDocumentation>(
				version,
				classes,
				documentationFile.Classes);
			ProcessListIntoDictionary<StructHistory, DataMemberHistory, StructDocumentation, DataMemberDocumentation>(
				version,
				structs,
				documentationFile.Structs);
			ProcessListIntoDictionary<EnumHistory, EnumMemberHistory, EnumDocumentation, EnumMemberDocumentation>(
				version,
				enums,
				documentationFile.Enums);

			Console.WriteLine(documentationFile.UnityVersion);
		}
		historyFile.SaveAsJson(outputPath);
	}

	private static void ProcessListIntoDictionary<THistory, TMemberHistory, TDocumentation, TMemberDocumentation>(
		UnityVersion version,
		Dictionary<string, THistory> dictionary,
		List<TDocumentation> list)
		where TMemberDocumentation : DocumentationBase, new()
		where TDocumentation : TypeDocumentation<TMemberDocumentation>, new()
		where TMemberHistory : HistoryBase, new()
		where THistory : TypeHistory<TMemberHistory, TMemberDocumentation>, new()
	{
		HashSet<string> processedClasses = new();
		foreach (TDocumentation @class in list)
		{
			string fullName = @class.FullName.ToString();
			if (dictionary.TryGetValue(fullName, out THistory? classHistory))
			{
				classHistory.Add(version, @class);
			}
			else
			{
				classHistory = new();
				classHistory.Initialize(version, @class);
				dictionary.Add(fullName, classHistory);
			}
			processedClasses.Add(fullName);
		}
		foreach ((string fullName, THistory classHistory) in dictionary)
		{
			if (!processedClasses.Contains(fullName))
			{
				classHistory.Add(version, null);
			}
		}
	}

	private static IEnumerable<DocumentationFile> ExtractAllDocumentation(string inputDirectory)
	{
		foreach ((UnityVersion unityVersion, string versionFolder) in GetUnityDirectories(inputDirectory))
		{
			string engineXmlPath = Path.Combine(versionFolder, @"Editor\Data\Managed\UnityEngine.xml");
			string editorXmlPath = Path.Combine(versionFolder, @"Editor\Data\Managed\UnityEditor.xml");
			string engineDllPath = Path.Combine(versionFolder, @"Editor\Data\Managed\UnityEngine.dll");
			string editorDllPath = Path.Combine(versionFolder, @"Editor\Data\Managed\UnityEditor.dll");
			yield return DocumentationExtractor.ExtractDocumentation(unityVersion.ToString(), engineXmlPath, editorXmlPath, engineDllPath, editorDllPath);
		}
	}

	private static List<(UnityVersion, string)> GetUnityDirectories(string inputDirectory)
	{
		List<(UnityVersion, string)> list = new();
		foreach (string versionFolder in Directory.GetDirectories(inputDirectory))
		{
			UnityVersion unityVersion = UnityVersion.Parse(Path.GetFileName(versionFolder));
			if (unityVersion < MinimumUnityVersion)
			{
				continue;
			}
			else if (unityVersion.LessThan(4, 5))
			{
				string infoPlistPath = Path.Combine(versionFolder, "Editor/Data/PlaybackEngines/macstandaloneplayer/UnityPlayer.app/Contents/Info.plist");
				UnityVersion actualVersion = XmlDocumentParser.ExtractUnityVersionFromXml(infoPlistPath);
				list.Add((actualVersion, versionFolder));
			}
			else if (unityVersion.LessThan(4, 6, 2))
			{
				string infoPlistPath = Path.Combine(versionFolder, "Editor/Data/PlaybackEngines/macstandalonesupport/Variations/universal_development/UnityPlayer.app/Contents/Info.plist");
				UnityVersion actualVersion = XmlDocumentParser.ExtractUnityVersionFromXml(infoPlistPath);
				list.Add((actualVersion, versionFolder));
			}
			else if (unityVersion.Equals(4, 6, 4))//This particular version doesn't have Info.plist
			{
				list.Add((new UnityVersion(4, 6, 4, UnityVersionType.Final, 1), versionFolder));
			}
			else if (unityVersion.LessThan(5))
			{
				string infoPlistPath = Path.Combine(versionFolder, "Editor/Data/PlaybackEngines/macstandalonesupport/Variations/universal_development_mono/UnityPlayer.app/Contents/Info.plist");
				UnityVersion actualVersion = XmlDocumentParser.ExtractUnityVersionFromXml(infoPlistPath);
				list.Add((actualVersion, versionFolder));
			}
			else
			{
				list.Add((unityVersion, versionFolder));
			}
		}
		list.Sort((pair1, pair2) => pair1.Item1.CompareTo(pair2.Item1));
		return list;
	}
}