using AssetRipper.Core;
using AssetRipper.Core.Attributes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.VersionHandling;
using System.Reflection;

namespace AssemblyValidator
{
	internal static class Loader
	{
		internal static void LoadHandlers(string handlerDirectory)
		{
			Console.WriteLine("Loading version handlers...");
			if (!Directory.Exists(handlerDirectory))
			{
				throw new DirectoryNotFoundException(handlerDirectory);
			}

			List<Type> handlerTypes = new();
			foreach (string filePath in Directory.GetFiles(handlerDirectory, "*.dll"))
			{
				Console.WriteLine($"Found assembly at {filePath}");
				Assembly assembly = Assembly.LoadFile(filePath);
				foreach (RegisterVersionHandlerAttribute handlerAttr in assembly.GetCustomAttributes<RegisterVersionHandlerAttribute>())
				{
					handlerTypes.Add(handlerAttr.HandlerType);
				}
			}
			Dictionary<UnityVersion, UnityHandlerBase> handlers = new();
			foreach (Type type in handlerTypes)
			{
				UnityHandlerBase versionHandler = (UnityHandlerBase)Activator.CreateInstance(type)!;
				handlers.Add(versionHandler.UnityVersion, versionHandler);
				Console.WriteLine($"Found version handler: {versionHandler.UnityVersion}");
			}
			Console.WriteLine("Finished loading version handlers.");
			List<Exception> exceptions = new();
			foreach((UnityVersion version, UnityHandlerBase handler) in handlers)
			{
				try
				{
					VirtualSerializedFile virtualSerializedFile = new VirtualSerializedFile(new AssetRipper.Core.Layout.LayoutInfo(default, default, default));
					foreach(int id in handler.GetAllValidIdNumbers())
					{
						handler.AssetFactory.CreateAsset(new AssetInfo(virtualSerializedFile, default, (ClassIDType)id));
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine($"Exception throw while processing {version}\n{ex}");
					exceptions.Add(ex);
				}
			}
			if(exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}
		}
	}
}
