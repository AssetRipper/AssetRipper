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
		Console.WriteLine(AsciiArt);
		Console.WriteLine();
		Console.WriteLine(Directions);
		Console.WriteLine();
	}
}
