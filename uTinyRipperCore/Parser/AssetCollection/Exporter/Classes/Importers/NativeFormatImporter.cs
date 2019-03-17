using System;
using uTinyRipper.YAML;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters.Classes
{
	public class NativeFormatImporter : DefaultImporter
	{
		public NativeFormatImporter(Object mainObject)
		{
			if(mainObject == null)
			{
				throw new ArgumentNullException(nameof(mainObject));
			}
			m_mainObject = mainObject;
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			node.Add("mainObjectFileID", container.GetExportID(m_mainObject));
		}

		public override string Name => nameof(NativeFormatImporter);

		private readonly Object m_mainObject;
	}
}
