using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Classes.Shaders.Exporters;
using uTinyRipper.SerializedFiles;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderAssetExporter : IAssetExporter
	{
		static ShaderAssetExporter()
		{
			s_isSupported = true;
			try
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				Module module = assembly.GetModules()[0];
				Type type = module.GetType($"{nameof(DotNetDxc)}.{nameof(DotNetDxc.DefaultDxcLib)}");
				MethodInfo mi = type.GetMethod(nameof(DotNetDxc.DefaultDxcLib.DxcCreateInstance), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				Marshal.Prelink(mi);
			}
			catch(DllNotFoundException)
			{
				s_isSupported = false;
			}
		}

		public bool IsHandle(Object asset)
		{
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path, null);
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
				Shader shader = (Shader)asset;
				shader.ExportBinary(container, fileStream, ShaderExporterInstantiator);
			}
			callback?.Invoke(container, asset, path);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset);
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}

		private static ShaderTextExporter ShaderExporterInstantiator(ShaderGpuProgramType programType)
		{
			if(s_isSupported && programType.IsDX())
			{
				return new ShaderDXExporter();
			}
			return Shader.DefaultShaderExporterInstantiator(programType);
		}

		/// <summary>
		/// DXCompiler is not supported by all OS versions
		/// </summary>
		private static bool s_isSupported;
	}
}
