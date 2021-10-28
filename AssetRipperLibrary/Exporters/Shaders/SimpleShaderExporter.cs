using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Shaders
{
	/// <summary>
	/// An exporter for the occasional situation where a shader asset actually contains the shader source code
	/// </summary>
	public class SimpleShaderExporter : BinaryAssetExporter
	{
		public override bool IsHandle(UnityObjectBase asset)
		{
			if (asset is ITextAsset textAsset && asset is IShader)
				return HasDecompiledShaderText(textAsset.RawData);
			else
				return false;
		}

		public override bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(((ITextAsset)asset).RawData);
				}
			}
			return true;
		}

		private static bool HasDecompiledShaderText(byte[] data)
		{
			if(!IsValidData(data))
				return false;

			string text = Encoding.UTF8.GetString(data);
			return !text.Contains("Program") && !text.Contains("SubProgram");
		}
	}
}
