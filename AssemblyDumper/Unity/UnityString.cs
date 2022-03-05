namespace AssemblyDumper.Unity
{
	public class UnityString
	{
		private string  @string = "";

		public uint Index { get; set; }
		public string String { get => @string; set => @string = value ?? ""; }
	}
}