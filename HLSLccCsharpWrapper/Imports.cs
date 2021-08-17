using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HLSLccCsharpWrapper
{
	public unsafe static class Imports
	{
		public const string DllName = "HLSLccWrapper";
		static Imports()
		{
			Pinvoke.DllLoader.PreloadDll(DllName);
		}

		[DllImport("HLSLccWrapper",CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern string ShaderTranslateFromFile([MarshalAs(UnmanagedType.LPStr)] string filepath, GLLang language, WrappedGlExtensions extensions);

		[DllImport("HLSLccWrapper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern string ShaderTranslateFromMem(byte[] data, GLLang lang, WrappedGlExtensions extensions);
	}
}
