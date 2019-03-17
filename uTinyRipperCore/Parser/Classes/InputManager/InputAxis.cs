using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.InputManagers;

namespace uTinyRipper.Classes.InputManagers
{
	public struct InputAxis : IAssetReadable, IYAMLExportable
	{
		public InputAxis(string name, string positive, string altPositive)
		{
			Name = name;
			DescriptiveName = string.Empty;
			DescriptiveNegativeName = string.Empty;
			NegativeButton = string.Empty;
			PositiveButton = positive;
			AltNegativeButton = string.Empty;
			AltPositiveButton = altPositive;
			Gravity = 1000.0f;
			Dead = 0.001f;
			Sensitivity = 1000.0f;
			Snap = false;
			Invert = false;
			Type = InputAxisType.KeyOrMouseButton;
			Axis = InputAxesDirection.X;
			JoyNum = JoystickType.AllJoysticks;
		}

		private static int GetSerializedVersion(Version version)
		{
			// this is min version
			return 3;
		}

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			DescriptiveName = reader.ReadString();
			DescriptiveNegativeName = reader.ReadString();
			NegativeButton = reader.ReadString();
			PositiveButton = reader.ReadString();
			AltNegativeButton = reader.ReadString();
			AltPositiveButton = reader.ReadString();
			Gravity = reader.ReadSingle();
			Dead = reader.ReadSingle();
			Sensitivity = reader.ReadSingle();
			Snap = reader.ReadBoolean();
			Invert = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			Type = (InputAxisType)reader.ReadInt32();
			Axis = (InputAxesDirection)reader.ReadInt32();
			JoyNum = (JoystickType)reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Name", Name);
			node.Add("descriptiveName", DescriptiveName);
			node.Add("descriptiveNegativeName", DescriptiveNegativeName);
			node.Add("negativeButton", NegativeButton);
			node.Add("positiveButton", PositiveButton);
			node.Add("altNegativeButton", AltNegativeButton);
			node.Add("altPositiveButton", AltPositiveButton);
			node.Add("gravity", Gravity);
			node.Add("dead", Dead);
			node.Add("sensitivity", Sensitivity);
			node.Add("snap", Snap);
			node.Add("invert", Invert);
			node.Add("type", (int)Type);
			node.Add("axis", (int)Axis);
			node.Add("joyNum", (int)JoyNum);
			return node;
		}

		public string Name { get; private set; }
		public string DescriptiveName { get; private set; }
		public string DescriptiveNegativeName { get; private set; }
		public string NegativeButton { get; private set; }
		public string PositiveButton { get; private set; }
		public string AltNegativeButton { get; private set; }
		public string AltPositiveButton { get; private set; }
		public float Gravity { get; private set; }
		public float Dead { get; private set; }
		public float Sensitivity { get; private set; }
		public bool Snap { get; private set; }
		public bool Invert { get; private set; }
		public InputAxisType Type { get; private set; }
		public InputAxesDirection Axis { get; private set; }
		public JoystickType JoyNum { get; private set; }
	}
}
