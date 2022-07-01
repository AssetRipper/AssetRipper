using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class FontAssetExportCollection : AssetExportCollection
	{
		public FontAssetExportCollection(FontAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			byte[] fontData = ((IFont)asset).FontData_C128;
			uint type = BitConverter.ToUInt32(fontData, 0);
			return type == OttoAsciiFourCC ? "otf" : "ttf";
		}

		/// <summary>
		/// OTTO ascii
		/// </summary>
		private const int OttoAsciiFourCC = 0x4F54544F;
	}
}
