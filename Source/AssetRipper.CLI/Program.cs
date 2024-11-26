using AssetRipper.GUI.Web;
using AssetRipper.Import.Logging;

Console.WriteLine(WelcomeMessage.AsciiArt);

Logger.Add(new FileLogger());
Logger.LogSystemInformation("AssetRipper");
Logger.Add(new ConsoleLogger());

static int Quit(string message)
{
	Logger.Error(message);
	Environment.Exit(1);
	return 1;
}

const string MODE_PROJECT = "unityproject";
const string MODE_CONTENT = "primarycontent";
if (args is not [ string rawMode, string rawSourceDir, string rawOutputDir ])
{
	return Quit(OperatingSystem.IsWindows()
		? $@"usage: .\AssetRipper.CLI.exe <{MODE_PROJECT}|{MODE_CONTENT}> ""C:\path\to\game"" ""C:\target\output\path"""
		: $"usage: ./AssetRipper.CLI <{MODE_PROJECT}|{MODE_CONTENT}> \"/path/to/game\" \"/target/output/path\"");
}

bool shouldExportPrimaryContent;
switch (rawMode)
{
	case MODE_PROJECT:
		shouldExportPrimaryContent = false;
		break;
	case MODE_CONTENT:
		shouldExportPrimaryContent = true;
		break;
	default:
		return Quit($"unrecognized mode \"{rawMode}\" (valid: {string.Join(" ", [ MODE_PROJECT, MODE_CONTENT ])})");
}

DirectoryInfo diSource = new(rawSourceDir);
if (!diSource.Exists)
{
	return Quit($"no such source dir \"{rawSourceDir}\"");
}

DirectoryInfo diOutput = new(rawOutputDir);
if (diOutput.Exists)
{
	if (diOutput.EnumerateFileSystemInfos().Any())
	{
		return Quit($"output dir \"{rawOutputDir}\" not empty");
	}
	// else fall through
}
else
{
	if (File.Exists(rawOutputDir))
	{
		return Quit($"output \"{rawOutputDir}\" exists but is not an empty directory");
	}
	diOutput.Create();
}

GameFileLoader.LoadAndProcess([ diSource.FullName ]);
if (shouldExportPrimaryContent)
{
	GameFileLoader.ExportPrimaryContent(diOutput.FullName);
}
else
{
	GameFileLoader.ExportUnityProject(diOutput.FullName);
}

return 0;
