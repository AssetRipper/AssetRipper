using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Project.Exporters.Engine
{
	public struct EngineBuiltInAsset
	{
		public EngineBuiltInAsset(uint exportID, uint parameter, bool isF)
		{
			ExportID = exportID;
			Parameter = parameter;
			m_isF = isF;
		}

		public MetaPtr ToExportPointer()
		{
			return new MetaPtr(ExportID, GUID, AssetType.Internal);
		}

		public UnityGUID GUID => m_isF ? EngineBuiltInAssets.FGUID : EngineBuiltInAssets.EGUID;

		public bool IsValid => ExportID != 0;
		public uint ExportID { get; }
		public uint Parameter { get; }

		/// <summary>
		///  Is assets located in DefaultResources file
		/// </summary>
		private readonly bool m_isF;
	}
}
