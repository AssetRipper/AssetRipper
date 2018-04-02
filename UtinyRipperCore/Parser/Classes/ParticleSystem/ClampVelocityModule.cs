using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class ClampVelocityModule : ParticleSystemModule
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

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("x", X.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("magnitude", Magnitude.ExportYAML(exporter));
			node.Add("separateAxis", SeparateAxis);
			node.Add("inWorldSpace", InWorldSpace);
			node.Add("multiplyDragByParticleSize", MultiplyDragByParticleSize);
			node.Add("multiplyDragByParticleVelocity", MultiplyDragByParticleVelocity);
			node.Add("dampen", Dampen);
			node.Add("drag", Drag.ExportYAML(exporter));
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
