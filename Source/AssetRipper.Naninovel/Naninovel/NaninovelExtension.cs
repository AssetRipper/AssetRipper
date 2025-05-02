using System;
using System.IO;
using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Import.Structure.Assembly.Serializable;

namespace AssetRipper.Naninovel
{
    public static class NaninovelExtension
    {
        private static readonly NaninovelScriptParser parser = new();

        public static bool TryParseNaninovelScript(this SerializableValue value, string scriptName)
        {
            try
            {
                if (parser.CanParse(scriptName))
                {
                    parser.ParseScript(value, scriptName);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Naninovel script {scriptName}: {ex.Message}");
                return false;
            }
        }

        public static void LoadNaninovelAssemblies(string managedPath)
        {
            if (!Directory.Exists(managedPath))
            {
                throw new DirectoryNotFoundException($"Managed directory not found: {managedPath}");
            }

            foreach (var assembly in parser.NaninovelAssemblies)
            {
                var assemblyPath = Path.Combine(managedPath, assembly);
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        parser.LoadAssembly(assemblyPath);
                        Console.WriteLine($"Loaded Naninovel assembly: {assembly}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading assembly {assembly}: {ex.Message}");
                    }
                }
            }
        }
    }
}