using AssetRipper.GUI.Web;
using AssetRipper.Import.Utils;
using System.Reflection;

internal static class Program
{
	[STAThread]
	private static int Main(string[] args)
	{
		Console.WriteLine("### AssetRipper CLI ###");

		Dictionary<string, string> arguments = ParseArguments(args);

		// Configure settings using reflection
		ConfigureSettings(arguments);

		// Check if the arguments contain the input and output paths
		if (!arguments.ContainsKey("InputPath") || !arguments.ContainsKey("OutputPath"))
		{
			Console.WriteLine("Please provide the input and output paths.");
			return 1;
		}

		string InputPath = arguments["InputPath"];
		string OutputPath = arguments["OutputPath"];

		if (!IsValidPath(InputPath))
		{
			Console.WriteLine("The provided path to the game files is invalid. Please provide a valid path.");
			return 1;
		}
		if (!IsValidPath(OutputPath))
		{
			Console.WriteLine("The provided path to export to is invalid. Please provide a valid path.");
			return 1;
		}
		if (!Directory.Exists(InputPath))
		{
			Console.WriteLine("The provided path to the game files does not exist. Please provide a valid path.");
			return 1;
		}

		Console.WriteLine("Loading game files...");
		GameFileLoader.LoadAndProcess(new List<string> { InputPath });
		if (GameFileLoader.IsLoaded)
		{
			Console.WriteLine("Game file loaded successfully and ready to export");
			Console.WriteLine("Exporting Unity project...");
			GameFileLoader.ExportUnityProject(OutputPath);
			Console.WriteLine("Exported Unity project successfully to " + OutputPath);
			return 0;
		}
		else
		{
			Console.WriteLine("Failed to load game file from " + InputPath);
			return 1;
		}
	}

	/// <summary>
	/// Configures settings using reflection based on provided arguments
	/// </summary>
	private static void ConfigureSettings(Dictionary<string, string> arguments)
	{
		// Map of settings categories to their corresponding objects
		var settingsMap = new Dictionary<string, object>
		{
			{ "ImportSettings", GameFileLoader.Settings.ImportSettings },
			{ "ProcessingSettings", GameFileLoader.Settings.ProcessingSettings },
			{ "ExportSettings", GameFileLoader.Settings.ExportSettings }
		};

		// Process each argument
		foreach (var arg in arguments)
		{
			// Skip input/output paths as they are handled separately
			if (arg.Key == "InputPath" || arg.Key == "OutputPath")
				continue;

			bool found = false;

			// Try to find and set the property in any of our settings objects
			foreach (var settingsPair in settingsMap)
			{
				var settingsObj = settingsPair.Value;
				var propertyInfo = settingsObj.GetType().GetProperty(arg.Key);

				if (propertyInfo != null)
				{
					// We found the property, now set its value
					Console.WriteLine($"Setting {arg.Key} to {arg.Value}");

					try
					{
						// Get the property type
						Type propertyType = propertyInfo.PropertyType;

						// Parse the value according to its type
						object parsedValue = ParseValueByType(arg.Value, propertyType);

						// Set the property
						propertyInfo.SetValue(settingsObj, parsedValue);

						// Display the updated value
						Console.WriteLine(propertyInfo.GetValue(settingsObj));

						found = true;
						break;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error setting {arg.Key}: {ex.Message}");
					}
				}
			}

			if (!found)
			{
				Console.WriteLine($"Warning: Setting {arg.Key} not found in any settings object");
			}
		}

		Console.WriteLine("Set Settings!");
	}

	/// <summary>
	/// Parses a string value to the appropriate type
	/// </summary>
	private static object ParseValueByType(string value, Type targetType)
	{
		// Handle null value
		if (value == null)
			return null;

		// Handle different types
		if (targetType == typeof(bool) || targetType == typeof(bool?))
		{
			return bool.Parse(value);
		}
		else if (targetType.IsEnum)
		{
			// Parse enum by integer value
			if (int.TryParse(value, out int intValue))
			{
				return Enum.ToObject(targetType, intValue);
			}
			// Parse enum by name
			return Enum.Parse(targetType, value, true);
		}
		else if (targetType == typeof(int) || targetType == typeof(int?))
		{
			return int.Parse(value);
		}
		else if (targetType == typeof(float) || targetType == typeof(float?))
		{
			return float.Parse(value);
		}
		else if (targetType == typeof(string))
		{
			return value;
		}

		// Add more type handling as needed

		throw new ArgumentException($"Unsupported type: {targetType.Name}");
	}

	static bool IsValidPath(string path)
	{
		// Check if the string contains any invalid characters
		char[] invalidChars = Path.GetInvalidPathChars();
		if (path.IndexOfAny(invalidChars) >= 0)
		{
			return false; // Contains invalid characters
		}
		return true;
	}

	// Simple argument parsing function
	static Dictionary<string, string> ParseArguments(string[] args)
	{
		var arguments = new Dictionary<string, string>();

		for (int i = 0; i < args.Length; i++)
		{
			// Handle flags (e.g., -verbose)
			if (args[i].StartsWith("-"))
			{
				string key = args[i].TrimStart('-');

				// Check if there's a value after the flag
				if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
				{
					arguments[key] = args[i + 1]; // Save the flag with its value
					i++; // Skip the next argument since it's the value
				}
				else
				{
					arguments[key] = null; // It's a flag without a value
				}
			}
		}

		return arguments;
	}
}
