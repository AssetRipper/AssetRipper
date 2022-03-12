using DotNetDxc;
using System;
using System.Runtime.InteropServices;

namespace ShaderTextRestorer.Handlers
{
	public static class D3DHandler
	{
		public static bool IsD3DAvailable() => OperatingSystem.IsWindows();

		public static bool IsCompatible(uint magicNumber) => magicNumber == DXBCFourCC;

		public static bool TryGetShaderText(byte[] data, int dataOffset, out string shaderText)
		{
			if (!OperatingSystem.IsWindows())
			{
				shaderText = null;
				return false;
			}

			int dataLength = data.Length - dataOffset;
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(dataLength);
			Marshal.Copy(data, dataOffset, unmanagedPointer, dataLength);

			D3DCompiler.D3DCompiler.D3DDisassemble(unmanagedPointer, (uint)dataLength, 0, null, out IDxcBlob disassembly);
			shaderText = GetStringFromBlob(disassembly);

			Marshal.FreeHGlobal(unmanagedPointer);

			return !string.IsNullOrEmpty(shaderText);
		}

		private static string GetStringFromBlob(IDxcBlob blob)
		{
			return Marshal.PtrToStringAnsi(blob.GetBufferPointer());
		}

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		const uint DXBCFourCC = 0x43425844;
	}
}
