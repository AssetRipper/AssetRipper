using System.Text;

namespace AssetRipper.GUI.Web;

public static class WelcomeMessage
{
	private const string AsciiArt = """
		                       _   _____  _                       
		    /\                | | |  __ \(_)                      
		   /  \   ___ ___  ___| |_| |__) |_ _ __  _ __   ___ _ __ 
		  / /\ \ / __/ __|/ _ \ __|  _  /| | '_ \| '_ \ / _ \ '__|
		 / ____ \\__ \__ \  __/ |_| | \ \| | |_) | |_) |  __/ |   
		/_/    \_\___/___/\___|\__|_|  \_\_| .__/| .__/ \___|_|   
		                                   | |   | |              
		                                   |_|   |_|              
		""";

	private const string Directions = """
		In a moment, a line will appear: "Now listening on:" followed by a url.
		Open that url in any web browser to access the AssetRipper user interface.
		""";

	public static void Print()
	{
		// Print the gradient ASCII Art
		PrintGradient(AsciiArt);

		// Print the credit line in the end-gradient color (Violet #aa39ff)
		Console.WriteLine("\u001b[38;2;170;57;255mFixed By StevenVR\u001b[0m");
		
		Console.WriteLine();
		Console.WriteLine(Directions);
		Console.WriteLine();
	}

	private static void PrintGradient(string text)
	{
		// Start Color: AssetRipper Blue (#1d7af0 / 29, 122, 240)
		const int startR = 29, startG = 122, startB = 240;
		
		// End Color: Electric Violet (#aa39ff / 170, 57, 255)
		const int endR = 170, endG = 57, endB = 255;

		string[] lines = text.Split(Environment.NewLine);
		int maxLineLength = 0;
		foreach (string line in lines)
		{
			if (line.Length > maxLineLength) maxLineLength = line.Length;
		}

		StringBuilder sb = new StringBuilder();

		foreach (string line in lines)
		{
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];

				// If whitespace, skip coloring to save buffer size, just append
				if (char.IsWhiteSpace(c))
				{
					sb.Append(c);
					continue;
				}

				// Calculate interpolation factor (0.0 to 1.0) based on horizontal position
				double t = (double)i / maxLineLength;

				// Linear interpolation for RGB
				int r = (int)(startR + (endR - startR) * t);
				int g = (int)(startG + (endG - startG) * t);
				int b = (int)(startB + (endB - startB) * t);

				// Append ANSI 24-bit color code
				sb.Append($"\u001b[38;2;{r};{g};{b}m{c}");
			}
			
			// Reset color at end of line and add newline
			sb.Append("\u001b[0m");
			sb.AppendLine();
		}

		Console.Write(sb.ToString());
	}
}
