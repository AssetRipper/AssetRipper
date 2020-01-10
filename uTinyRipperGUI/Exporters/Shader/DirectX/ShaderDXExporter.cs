using System;
using System.Runtime.InteropServices;
using DotNetDxc;
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
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(m_graphicApi))
			{
				dataOffset = DXDataHeader.GetDataOffset(writer.Version, m_graphicApi);
				uint fourCC = BitConverter.ToUInt32(exportData, dataOffset);
				if (fourCC != DXBCFourCC)
				{
					throw new Exception("Magic number doesn't match");
				}
			}

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

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		protected const uint DXBCFourCC = 0x43425844;

		protected readonly GPUPlatform m_graphicApi;
	}
}
