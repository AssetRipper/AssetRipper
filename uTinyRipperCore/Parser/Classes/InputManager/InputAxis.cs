using uTinyRipper.YAML;
using uTinyRipper.Converters;

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

		public static int ToSerializedVersion(Version version)
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
			reader.AlignStream();
			
			Type = (InputAxisType)reader.ReadInt32();
			Axis = (InputAxesDirection)reader.ReadInt32();
			JoyNum = (JoystickType)reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(NameName, Name);
			node.Add(DescriptiveNameName, DescriptiveName);
			node.Add(DescriptiveNegativeNameName, DescriptiveNegativeName);
			node.Add(NegativeButtonName, NegativeButton);
			node.Add(PositiveButtonName, PositiveButton);
			node.Add(AltNegativeButtonName, AltNegativeButton);
			node.Add(AltPositiveButtonName, AltPositiveButton);
			node.Add(GravityName, Gravity);
			node.Add(DeadName, Dead);
			node.Add(SensitivityName, Sensitivity);
			node.Add(SnapName, Snap);
			node.Add(InvertName, Invert);
			node.Add(TypeName, (int)Type);
			node.Add(AxisName, (int)Axis);
			node.Add(JoyNumName, (int)JoyNum);
			return node;
		}

		public string Name { get; set; }
		public string DescriptiveName { get; set; }
		public string DescriptiveNegativeName { get; set; }
		public string NegativeButton { get; set; }
		public string PositiveButton { get; set; }
		public string AltNegativeButton { get; set; }
		public string AltPositiveButton { get; set; }
		public float Gravity { get; set; }
		public float Dead { get; set; }
		public float Sensitivity { get; set; }
		public bool Snap { get; set; }
		public bool Invert { get; set; }
		public InputAxisType Type { get; set; }
		public InputAxesDirection Axis { get; set; }
		public JoystickType JoyNum { get; set; }

		public const string NameName = "m_Name";
		public const string DescriptiveNameName = "descriptiveName";
		public const string DescriptiveNegativeNameName = "descriptiveNegativeName";
		public const string NegativeButtonName = "negativeButton";
		public const string PositiveButtonName = "positiveButton";
		public const string AltNegativeButtonName = "altNegativeButton";
		public const string AltPositiveButtonName = "altPositiveButton";
		public const string GravityName = "gravity";
		public const string DeadName = "dead";
		public const string SensitivityName = "sensitivity";
		public const string SnapName = "snap";
		public const string InvertName = "invert";
		public const string TypeName = "type";
		public const string AxisName = "axis";
		public const string JoyNumName = "joyNum";
	}
}
