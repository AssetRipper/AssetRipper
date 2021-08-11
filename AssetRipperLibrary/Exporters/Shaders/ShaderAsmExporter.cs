using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Library.Exporters.Shaders.DirectX;
using DotNetDxc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Library.Exporters.Shaders
{
	[SupportedOSPlatform("windows")]
	public class ShaderAsmExporter : CustomShaderTextExporter
	{
		public ShaderAsmExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}
		public override string Extension => ".asm";
		public static string Disassemble(byte[] exportData, UnityVersion version, GPUPlatform m_graphicApi)
		{
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(m_graphicApi))
			{
				dataOffset = DXDataHeader.GetDataOffset(version, m_graphicApi);
				uint fourCC = BitConverter.ToUInt32(exportData, dataOffset);
				if (fourCC != DXBCFourCC)
				{
					throw new Exception("Magic number doesn't match");
				}
			}

			int dataLength = exportData.Length - dataOffset;
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(dataLength);
			Marshal.Copy(exportData, dataOffset, unmanagedPointer, dataLength);

			D3DCompiler.D3DCompiler.D3DDisassemble(unmanagedPointer, (uint)dataLength, (uint)0, null, out DotNetDxc.IDxcBlob disassembly);
			string disassemblyText = GetStringFromBlob(disassembly);
			Marshal.FreeHGlobal(unmanagedPointer);
			return disassemblyText;
		}
		public override void DoExport(string filePath, UnityVersion version, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;
			string disassemblyText = Disassemble(exportData, version, m_graphicApi);
			File.WriteAllText(filePath, disassemblyText);
		}
		private static string GetStringFromBlob(IDxcBlob blob)
		{
			return Marshal.PtrToStringAnsi(blob.GetBufferPointer());
		}
		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		protected const uint DXBCFourCC = 0x43425844;

		protected readonly GPUPlatform m_graphicApi;
	}
}
