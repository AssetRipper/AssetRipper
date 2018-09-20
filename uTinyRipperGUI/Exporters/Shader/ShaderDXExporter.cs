using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DotNetDxc;
using uTinyRipper.Classes.Shaders.Exporters;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderDXExporter : ShaderTextExporter
	{
		static ShaderDXExporter()
		{
			HlslDxcLib.DxcCreateInstanceFn = DefaultDxcLib.GetDxcCreateInstanceFn();
		}

		public override void Export(byte[] shaderData, TextWriter writer)
		{
			int offset = 0;
			uint fourCC = BitConverter.ToUInt32(shaderData, 6);
			if(fourCC == DXBCFourCC)
			{
				offset = 6;
			}
			else
			{
#warning HACK: TEMP:
				fourCC = BitConverter.ToUInt32(shaderData, 5);
				if (fourCC == DXBCFourCC)
				{
					offset = 5;
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

		internal static string GetStringFromBlob(IDxcLibrary library, IDxcBlob blob)
		{
			unsafe
			{
				blob = library.GetBlobAstUf16(blob);
				return new string(blob.GetBufferPointer(), 0, (int)(blob.GetBufferSize() / 2) - 1);
			}
		}

		private string GetStringFromBlob(IDxcBlob blob)
		{
			return GetStringFromBlob(Library, blob);
		}

		internal IDxcLibrary Library
		{
			get { return (library ?? (library = HlslDxcLib.CreateDxcLibrary())); }
		}

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		private const uint DXBCFourCC = 0x43425844;

		private IDxcLibrary library;
	}
}
