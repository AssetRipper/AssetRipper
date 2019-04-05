using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DotNetDxc;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Classes.Shaders.Exporters;

using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderDXExporter : ShaderTextExporter
	{
		public ShaderDXExporter(Version version, GPUPlatform graphicApi)
		{
			m_version = version;
			m_graphicApi = graphicApi;
		}

		private static bool IsOffset(GPUPlatform graphicApi)
		{
			return graphicApi != GPUPlatform.d3d9;
		}

		private static bool IsOffset5(Version version)
		{
			return version.IsEqual(5, 3);
		}

		public override void Export(byte[] shaderData, TextWriter writer)
		{
			int offset = 0;
			if (IsOffset(m_graphicApi))
			{
				offset = IsOffset5(m_version) ? 5 : 6;
				uint fourCC = BitConverter.ToUInt32(shaderData, offset);
				if (fourCC != DXBCFourCC)
				{
					throw new Exception("Magic number doesn't match");
				}
			}

			int length = shaderData.Length - offset;
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(length);
			Marshal.Copy(shaderData, offset, unmanagedPointer, length);

			unsafe
			{
				D3DCompiler.D3DCompiler.D3DDisassemble(unmanagedPointer, (uint)length, 0, null, out IDxcBlob disassembly);
				string disassemblyText = GetStringFromBlob(disassembly);
				byte[] textBytes = Encoding.UTF8.GetBytes(disassemblyText);
				using (MemoryStream memStream = new MemoryStream(textBytes))
				{
					using (BinaryReader reader = new BinaryReader(memStream))
					{
						Export(reader, writer);
					}
				}
			}

			Marshal.FreeHGlobal(unmanagedPointer);
		}

		private string GetStringFromBlob(IDxcBlob blob)
		{
			return Marshal.PtrToStringAnsi(blob.GetBufferPointer());
		}

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		private const uint DXBCFourCC = 0x43425844;

		private readonly Version m_version;
		private readonly GPUPlatform m_graphicApi;
	}
}
