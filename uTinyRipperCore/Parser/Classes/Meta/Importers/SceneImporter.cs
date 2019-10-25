using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uTinyRipper.AssetExporters.Classes
{
	public sealed class SceneImporter : DefaultImporter
	{
		protected override bool IsExportExternalObjects => false;
	}
}
