using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Tools.MonoBehaviourTester;

internal static class Program
{
	public static PlatformGameStructure PlatformStructure = null!;
	public static BaseManager AssemblyManager = null!;

	public static int Main(string[] args)
	{
		Logger.Add(new ConsoleLogger(false));
		
		if (args.Length < 3)
		{
			Logger.Error($"Usage: {Path.GetFileName(Environment.ProcessPath)} <game path> <mono behavior assembly> <mono behavior FQN>");
			return 1;
		}

		string gamePath = args[0];
		string monoBehaviorAssembly = args[1];
		string monoBehaviorFQN = args[2];
		
		Logger.Info("Determining platform...");
		
		PlatformChecker.CheckPlatform(new() { gamePath }, out PlatformGameStructure? platformStructure, out MixedGameStructure? _);
		if (platformStructure == null)
		{
			Logger.Error("Game structure is not supported");
			return 1;
		}

		PlatformStructure = platformStructure;
		PlatformStructure.CollectFiles(true);
		
		ScriptingBackend backend = PlatformStructure.Backend;
		
		Logger.Info("Initializing assembly manager...");
		
		AssemblyManager = backend switch
		{
			ScriptingBackend.Mono => new MonoManager(OnRequestAssembly),
			ScriptingBackend.IL2Cpp => new IL2CppManager(OnRequestAssembly, ScriptContentLevel.Level2),
			_ => new BaseManager(OnRequestAssembly),
		};

		try
		{
			//Loads any Mono or IL2Cpp assemblies
			AssemblyManager.Initialize(PlatformStructure);
		}
		catch (Exception ex)
		{
			Logger.Error(LogCategory.Import, "Could not initialize assembly manager. Switching to the 'Unknown' scripting backend.");
			Logger.Error(ex);
			AssemblyManager = new BaseManager(OnRequestAssembly);
		}
		
		Logger.Info("Parsing requested type name...");

		string ns = string.Empty;
		string type = monoBehaviorFQN;

		if (monoBehaviorFQN.Contains('.'))
		{
			ns = monoBehaviorFQN[..monoBehaviorFQN.LastIndexOf('.')];
			type = monoBehaviorFQN[(monoBehaviorFQN.LastIndexOf('.') + 1)..];
		}

		Logger.Info($"Building serializable type for namespace: {ns}, type: {type}, assembly: {monoBehaviorAssembly}...");
		
		ScriptIdentifier typeId = new(monoBehaviorAssembly, ns, type);

		try
		{
			if (AssemblyManager.TryGetSerializableType(typeId, out SerializableType? serializableType, out string? failureReason))
			{
				Logger.Info($"Got serializable type: {serializableType}");
				PrintSerializationInfo(serializableType);
			}
			else
			{
				Logger.Error($"Could not build serializable type - {failureReason}");
				return 2;
			}
		}
		catch (Exception e)
		{
			Logger.Error($"Could not build serializable type - {e.Message}");
			return 1;
		}

		return 0;
	}

	private static void OnRequestAssembly(string assembly)
	{
		string? assemblyPath = PlatformStructure.RequestAssembly(assembly);
		if (assemblyPath is null)
		{
			Logger.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
			return;
		}

		AssemblyManager.Load(assemblyPath);
		Logger.Info(LogCategory.Import, $"Assembly '{assembly}' has been loaded");
	}

	private static void PrintSerializationInfo(SerializableType type, int indent = 1)
	{
		foreach (SerializableType.Field field in type.Fields)
		{
			string typeName = field.Type.ToString();
			if (field.ArrayDepth > 0)
			{
				typeName += string.Join("", Enumerable.Repeat("[]", field.ArrayDepth));
			}

			string ptrString = field.Type.IsEnginePointer() ? ", serialized as engine pointer" : "";
			
			Logger.Info("".PadLeft(indent * 4) + $"{field.Name} ({typeName}, {field.Type.Type}{ptrString})");

			if (field.Type.Type == PrimitiveType.Complex)
			{
				PrintSerializationInfo(field.Type, indent + 1);
			}
		}
	}
}
