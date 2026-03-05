using System.Runtime.InteropServices;
using System.Text;

namespace AssetRipper.GUI.Web;

public static class WelcomeMessage
{
    // Necessary for forcing color support on Windows
    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

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
        EnableConsoleColors();

        // Print the gradient ASCII Art
        PrintGradient(AsciiArt);

        // Print the credit line in the end-gradient color (Violet #aa39ff)
        Console.WriteLine("\u001b[38;2;170;57;255mFixed By StevenVR\u001b[0m");

        Console.WriteLine();
        Console.WriteLine(Directions);
        Console.WriteLine();
    }

    private static void EnableConsoleColors()
    {
        IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
        if (hOut == IntPtr.Zero) return;
        if (GetConsoleMode(hOut, out uint dwMode))
        {
            dwMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(hOut, dwMode);
        }
    }

    private static void PrintGradient(string text)
    {
        // Start: #1d7af0
        const int startR = 29, startG = 122, startB = 240;
        // End: #aa39ff
        const int endR = 170, endG = 57, endB = 255;

        string[] lines = text.Split(Environment.NewLine);
        int maxLineLength = 0;
        foreach (string line in lines) if (line.Length > maxLineLength) maxLineLength = line.Length;

        StringBuilder sb = new StringBuilder();
        foreach (string line in lines)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                    continue;
                }

                double t = (double)i / maxLineLength;
                int r = (int)(startR + (endR - startR) * t);
                int g = (int)(startG + (endG - startG) * t);
                int b = (int)(startB + (endB - startB) * t);

                sb.Append($"\u001b[38;2;{r};{g};{b}m{c}");
            }
            sb.Append("\u001b[0m");
            sb.AppendLine();
        }
        Console.Write(sb.ToString());
    }
}
