using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			X.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			Magnitude.Read(stream);
			SeparateAxis = stream.ReadBoolean();
			if (IsReadInWorldSpace(stream.Version))
			{
				InWorldSpace = stream.ReadBoolean();
			}
			if (IsReadMultiplyDragByParticleSize(stream.Version))
			{
				MultiplyDragByParticleSize = stream.ReadBoolean();
				MultiplyDragByParticleVelocity = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
			
			Dampen = stream.ReadSingle();
			if (IsReadDrag(stream.Version))
			{
				Drag.Read(stream);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("x", X.ExportYAML(container));
			node.Add("y", Y.ExportYAML(container));
			node.Add("z", Z.ExportYAML(container));
			node.Add("magnitude", Magnitude.ExportYAML(container));
			node.Add("separateAxis", SeparateAxis);
			node.Add("inWorldSpace", InWorldSpace);
			node.Add("multiplyDragByParticleSize", GetExportMultiplyDragByParticleSize(container.Version));
			node.Add("multiplyDragByParticleVelocity", GetExportMultiplyDragByParticleVelocity(container.Version));
			node.Add("dampen", Dampen);
			node.Add("drag", GetExportDrag(container.Version).ExportYAML(container));
			return node;
		}

		public bool SeparateAxis { get; private set; }
		public bool InWorldSpace { get; private set; }
		public bool MultiplyDragByParticleSize { get; private set; }
		public bool MultiplyDragByParticleVelocity { get; private set; }
		public float Dampen { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public MinMaxCurve Magnitude;
		public MinMaxCurve Drag;
	}
}
