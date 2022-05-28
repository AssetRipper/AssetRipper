using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.InputManager
{
	public sealed class InputManager : GlobalGameManager
	{
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasUsePhysicalKeys(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		public InputManager(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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

			Axes = reader.ReadAssetArray<InputAxis>();

			if (HasUsePhysicalKeys(reader.Version))
			{
				reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AxesName, GetAxes(container.Version).ExportYaml(container));
			return node;
		}

		private IReadOnlyList<InputAxis> GetAxes(UnityVersion version)
		{
			if (ToSerializedVersion(version) >= 2)
			{
				return Axes;
			}

			List<InputAxis> axes = new List<InputAxis>(Axes.Length + 3);
			axes.AddRange(Axes);
			axes.Add(new InputAxis("Submit", "return", "joystick button 0"));
			axes.Add(new InputAxis("Submit", "enter", "space"));
			axes.Add(new InputAxis("Cancel", "escape", "joystick button 1"));
			return axes;
		}

		public InputAxis[] Axes { get; set; }

		public const string AxesName = "m_Axes";
	}
}
