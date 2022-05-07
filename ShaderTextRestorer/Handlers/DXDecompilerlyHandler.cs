using AssetRipper.Core.Logging;
using DXDecompiler;
using DXDecompiler.Decompiler;
using DXDecompiler.Util;
using System;


namespace ShaderTextRestorer.Handlers
{
	public static class DXDecompilerlyHandler
	{
		public static bool TryDisassemble(byte[] data, int offset, out string disassemblyText) => TryDisassemble(GetRelevantData(data, offset), out disassemblyText);
		public static bool TryDisassemble(byte[] data, out string disassemblyText)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (data.Length == 0)
				throw new ArgumentException("inputData cannot have zero length");

			try
			{
				var programType = GetProgramType(data);
				switch (programType)
				{
					case DXProgramType.DXBC:
						var container = new BytecodeContainer(data);
						disassemblyText = container.ToString();
						return !string.IsNullOrEmpty(disassemblyText);
					case DXProgramType.DX9:
						disassemblyText = DXDecompiler.DX9Shader.AsmWriter.Disassemble(data);
						return !string.IsNullOrEmpty(disassemblyText);
				}
			}
			catch(Exception ex)
			{
				Logger.Error(LogCategory.Export, $"DXDecompilerly threw an exception while attempting to disassemble a shader");
				Logger.Verbose(LogCategory.Export, ex.ToString());
			}

			disassemblyText = null;
			return false;
		}

		public static bool TryDecompile(byte[] data, int offset, out string decompiledText) => TryDecompile(GetRelevantData(data, offset), out decompiledText);
		public static bool TryDecompile(byte[] data, out string decompiledText)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (data.Length == 0)
				throw new ArgumentException("inputData cannot have zero length");

			try
			{
				var programType = GetProgramType(data);
				switch (programType)
				{
					case DXProgramType.DXBC:
						decompiledText = HLSLDecompiler.Decompile(data);
						Logger.Info(LogCategory.Export, $"DXDecompilerly successfully decompiled a DXBC shader");
						return !string.IsNullOrEmpty(decompiledText);
					case DXProgramType.DX9:
						decompiledText = DXDecompiler.DX9Shader.HlslWriter.Decompile(data);
						Logger.Info(LogCategory.Export, $"DXDecompilerly successfully decompiled a DX9 shader");
						return !string.IsNullOrEmpty(decompiledText);
				}
			}
			catch (Exception ex)
			{
				Logger.Verbose(LogCategory.Export, $"DXDecompilerly threw an exception while attempting to decompile a shader");
				Logger.Verbose(LogCategory.Export, ex.ToString());
			}

			decompiledText = null;
			return false;
		}

		private static DXProgramType GetProgramType(byte[] data)
		{
			if (data.Length < 4)
			{
				return DXProgramType.Unknown;
			}
			uint dxbcHeader = BitConverter.ToUInt32(data, 0);
			if (dxbcHeader == "DXBC".ToFourCc())
			{
				return DXProgramType.DXBC;
			}
			if (dxbcHeader == 0xFEFF2001)
			{
				return DXProgramType.DXBC;
			}
			var dx9ShaderType = (DXDecompiler.DX9Shader.ShaderType)BitConverter.ToUInt16(data, 2);
			if (dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Vertex ||
				dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Pixel ||
				dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Effect)
			{
				return DXProgramType.DX9;
			}
			return DXProgramType.Unknown;
		}

		private static byte[] GetRelevantData(byte[] bytes, int offset)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (offset < 0 || offset > bytes.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			int size = bytes.Length - offset;
			byte[] result = new byte[size];
			for (int i = 0; i < size; i++)
			{
				result[i] = bytes[i + offset];
			}
			return result;
		}

		private enum DXProgramType
		{
			Unknown,
			DX9,
			DXBC
		}
	}
}
