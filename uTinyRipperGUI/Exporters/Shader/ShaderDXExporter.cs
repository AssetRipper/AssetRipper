using System;
using System.Runtime.InteropServices;
using DotNetDxc;
using DXShaderRestorer;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Converters.Shaders;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderDXExporter : ShaderTextExporter
	{
		public ShaderDXExporter(GPUPlatform graphicApi)
		{
			m_graphicApi = graphicApi;
		}

		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			byte[] exportData = subProgram.ProgramData;
			int dataOffset = DXShaderProgramRestorer.GetDataOffset(writer.Version, m_graphicApi, subProgram);
			int dataLength = exportData.Length - dataOffset;
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(dataLength);
			Marshal.Copy(exportData, dataOffset, unmanagedPointer, dataLength);

			D3DCompiler.D3DCompiler.D3DDisassemble(unmanagedPointer, (uint)dataLength, 0, null, out IDxcBlob disassembly);
			string disassemblyText = GetStringFromBlob(disassembly);
			ExportListing(writer, disassemblyText);

			Marshal.FreeHGlobal(unmanagedPointer);
		}

		private string GetStringFromBlob(IDxcBlob blob)
		{
			return Marshal.PtrToStringAnsi(blob.GetBufferPointer());
		}

		protected readonly GPUPlatform m_graphicApi;
	}
}
