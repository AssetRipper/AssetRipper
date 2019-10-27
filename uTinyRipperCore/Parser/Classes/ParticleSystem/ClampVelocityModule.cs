using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ClampVelocityModule : ParticleSystemModule
	{
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadInWorldSpace(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadMultiplyDragByParticleSize(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadDrag(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			Magnitude.Read(reader);
			SeparateAxis = reader.ReadBoolean();
			if (IsReadInWorldSpace(reader.Version))
			{
				InWorldSpace = reader.ReadBoolean();
			}
			if (IsReadMultiplyDragByParticleSize(reader.Version))
			{
				MultiplyDragByParticleSize = reader.ReadBoolean();
				MultiplyDragByParticleVelocity = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
			
			Dampen = reader.ReadSingle();
			if (IsReadDrag(reader.Version))
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

		private bool GetExportMultiplyDragByParticleSize(Version version)
		{
			return IsReadMultiplyDragByParticleSize(version) ? MultiplyDragByParticleSize : true;
		}
		private bool GetExportMultiplyDragByParticleVelocity(Version version)
		{
			return IsReadMultiplyDragByParticleSize(version) ? MultiplyDragByParticleVelocity : true;
		}
		private MinMaxCurve GetExportDrag(Version version)
		{
			return IsReadDrag(version) ? Drag : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxis { get; private set; }
		public bool InWorldSpace { get; private set; }
		public bool MultiplyDragByParticleSize { get; private set; }
		public bool MultiplyDragByParticleVelocity { get; private set; }
		public float Dampen { get; private set; }

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

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve Magnitude;
		public MinMaxCurve Drag;
	}
}
