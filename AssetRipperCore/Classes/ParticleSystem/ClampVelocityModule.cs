using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class ClampVelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasInWorldSpace(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasMultiplyDragByParticleSize(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasDrag(UnityVersion version) => version.IsGreaterEqual(2017, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			Magnitude.Read(reader);
			SeparateAxis = reader.ReadBoolean();
			if (HasInWorldSpace(reader.Version))
			{
				InWorldSpace = reader.ReadBoolean();
			}
			if (HasMultiplyDragByParticleSize(reader.Version))
			{
				MultiplyDragByParticleSize = reader.ReadBoolean();
				MultiplyDragByParticleVelocity = reader.ReadBoolean();
			}
			reader.AlignStream();

			Dampen = reader.ReadSingle();
			if (HasDrag(reader.Version))
			{
				Drag.Read(reader);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(XName, X.ExportYAML(container));
			node.Add(YName, Y.ExportYAML(container));
			node.Add(ZName, Z.ExportYAML(container));
			node.Add(MagnitudeName, Magnitude.ExportYAML(container));
			node.Add(SeparateAxisName, SeparateAxis);
			node.Add(InWorldSpaceName, InWorldSpace);
			node.Add(MultiplyDragByParticleSizeName, GetExportMultiplyDragByParticleSize(container.Version));
			node.Add(MultiplyDragByParticleVelocityName, GetExportMultiplyDragByParticleVelocity(container.Version));
			node.Add(DampenName, Dampen);
			node.Add(DragName, GetExportDrag(container.Version).ExportYAML(container));
			return node;
		}

		private bool GetExportMultiplyDragByParticleSize(UnityVersion version)
		{
			return HasMultiplyDragByParticleSize(version) ? MultiplyDragByParticleSize : true;
		}
		private bool GetExportMultiplyDragByParticleVelocity(UnityVersion version)
		{
			return HasMultiplyDragByParticleSize(version) ? MultiplyDragByParticleVelocity : true;
		}
		private MinMaxCurve GetExportDrag(UnityVersion version)
		{
			return HasDrag(version) ? Drag : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxis { get; set; }
		public bool InWorldSpace { get; set; }
		public bool MultiplyDragByParticleSize { get; set; }
		public bool MultiplyDragByParticleVelocity { get; set; }
		public float Dampen { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string MagnitudeName = "magnitude";
		public const string SeparateAxisName = "separateAxis";
		public const string InWorldSpaceName = "inWorldSpace";
		public const string MultiplyDragByParticleSizeName = "multiplyDragByParticleSize";
		public const string MultiplyDragByParticleVelocityName = "multiplyDragByParticleVelocity";
		public const string DampenName = "dampen";
		public const string DragName = "drag";

		public MinMaxCurve X = new();
		public MinMaxCurve Y = new();
		public MinMaxCurve Z = new();
		public MinMaxCurve Magnitude = new();
		public MinMaxCurve Drag = new();
	}
}
