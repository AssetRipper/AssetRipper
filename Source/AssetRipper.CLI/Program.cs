using AssetRipper.GUI.Web;

namespace UnityAssetExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: UnityAssetExtractor <inputBundlePath> <outputProjectPath>");
                return;
            }

            string inputBundlePath = args[0];
            string outputProjectPath = args[1];

            if (!File.Exists(inputBundlePath))
            {
                Console.WriteLine($"Input bundle file not found: {inputBundlePath}");
                return;
            }

            try
            {
                // Load and process the Unity bundle
                GameFileLoader.LoadAndProcess(new[] { inputBundlePath });

                // Export the data to a Unity project
                GameFileLoader.ExportUnityProject(outputProjectPath);

                Console.WriteLine($"Data extracted and Unity project created at: {outputProjectPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
