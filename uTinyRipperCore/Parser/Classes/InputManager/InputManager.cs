using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.InputManagers;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class InputManager : GlobalGameManager
	{
		public InputManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			return ToSerializedVersion(version);
		}

		private static int ToSerializedVersion(Version version)
		{
			// added some new default Axes
			if (version.IsGreaterEqual(4, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_axes = reader.ReadAssetArray<InputAxis>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Axes", GetAxes(container.Version).ExportYAML(container));
			return node;
		}

		private IReadOnlyList<InputAxis> GetAxes(Version version)
		{
			if (ToSerializedVersion(version) >= 2)
			{
				return Axes;
			}

			List<InputAxis> axes = new List<InputAxis>(Axes.Count + 3);
			axes.AddRange(Axes);
			axes.Add(new InputAxis("Submit", "return", "joystick button 0"));
			axes.Add(new InputAxis("Submit", "enter", "space"));
			axes.Add(new InputAxis("Cancel", "escape", "joystick button 1"));
			return axes;
		}

		public IReadOnlyList<InputAxis> Axes => m_axes;

		private InputAxis[] m_axes;
	}
}
