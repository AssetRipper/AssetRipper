using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLSLccCsharpWrapper
{
	[SupportedOSPlatform("windows")]
	[SupportedOSPlatform("linux")]
	public unsafe static class Imports
	{
		public const string DllName = "HLSLccWrapper";
		
		[DllImport("HLSLccWrapper",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern string ShaderTranslateFromFile([MarshalAs(UnmanagedType.LPStr)] string filepath, GLLang language, WrappedGlExtensions extensions);

		[DllImport("HLSLccWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern string ShaderTranslateFromMem(byte[] data, GLLang lang, WrappedGlExtensions extensions);
	}
}
