using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystemForceField
{
	public sealed class ParticleSystemForceFieldParameters : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Shape = (ParticleSystemForceFieldShape)reader.ReadInt32();
			StartRange = reader.ReadSingle();
			EndRange = reader.ReadSingle();
			Length = reader.ReadSingle();
			GravityFocus = reader.ReadSingle();
			RotationRandomness.Read(reader);
			DirectionCurveX.Read(reader);
			DirectionCurveY.Read(reader);
			DirectionCurveZ.Read(reader);
			GravityCurve.Read(reader);
			RotationSpeedCurve.Read(reader);
			RotationAttractionCurve.Read(reader);
			DragCurve.Read(reader);
			VectorField.Read(reader);
			VectorFieldSpeedCurve.Read(reader);
			VectorFieldAttractionCurve.Read(reader);
			MultiplyDragByParticleSize = reader.ReadBoolean();
			MultiplyDragByParticleVelocity = reader.ReadBoolean();
			reader.AlignStream();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(VectorField, VectorFieldName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ShapeName, (int)Shape);
			node.Add(StartRangeName, StartRange);
			node.Add(EndRangeName, EndRange);
			node.Add(LengthName, Length);
			node.Add(GravityFocusName, GravityFocus);
			node.Add(RotationRandomnessName, RotationRandomness.ExportYaml(container));
			node.Add(DirectionCurveXName, DirectionCurveX.ExportYaml(container));
			node.Add(DirectionCurveYName, DirectionCurveY.ExportYaml(container));
			node.Add(DirectionCurveZName, DirectionCurveZ.ExportYaml(container));
			node.Add(GravityCurveName, GravityCurve.ExportYaml(container));
			node.Add(RotationSpeedCurveName, RotationSpeedCurve.ExportYaml(container));
			node.Add(RotationAttractionCurveName, RotationAttractionCurve.ExportYaml(container));
			node.Add(DragCurveName, DragCurve.ExportYaml(container));
			node.Add(VectorFieldName, VectorField.ExportYaml(container));
			node.Add(VectorFieldSpeedCurveName, VectorFieldSpeedCurve.ExportYaml(container));
			node.Add(VectorFieldAttractionCurveName, VectorFieldAttractionCurve.ExportYaml(container));
			node.Add(MultiplyDragByParticleSizeName, MultiplyDragByParticleSize);
			node.Add(MultiplyDragByParticleVelocityName, MultiplyDragByParticleVelocity);
			return node;
		}

		public ParticleSystemForceFieldShape Shape { get; set; }
		public float StartRange { get; set; }
		public float EndRange { get; set; }
		public float Length { get; set; }
		public float GravityFocus { get; set; }
		public bool MultiplyDragByParticleSize { get; set; }
		public bool MultiplyDragByParticleVelocity { get; set; }

		public const string ShapeName = "m_Shape";
		public const string StartRangeName = "m_StartRange";
		public const string EndRangeName = "m_EndRange";
		public const string LengthName = "m_Length";
		public const string GravityFocusName = "m_GravityFocus";
		public const string RotationRandomnessName = "m_RotationRandomness";
		public const string DirectionCurveXName = "m_DirectionCurveX";
		public const string DirectionCurveYName = "m_DirectionCurveY";
		public const string DirectionCurveZName = "m_DirectionCurveZ";
		public const string GravityCurveName = "m_GravityCurve";
		public const string RotationSpeedCurveName = "m_RotationSpeedCurve";
		public const string RotationAttractionCurveName = "m_RotationAttractionCurve";
		public const string DragCurveName = "m_DragCurve";
		public const string VectorFieldName = "m_VectorField";
		public const string VectorFieldSpeedCurveName = "m_VectorFieldSpeedCurve";
		public const string VectorFieldAttractionCurveName = "m_VectorFieldAttractionCurve";
		public const string MultiplyDragByParticleSizeName = "m_MultiplyDragByParticleSize";
		public const string MultiplyDragByParticleVelocityName = "m_MultiplyDragByParticleVelocity";

		public Vector2f RotationRandomness = new();
		public MinMaxCurve DirectionCurveX = new();
		public MinMaxCurve DirectionCurveY = new();
		public MinMaxCurve DirectionCurveZ = new();
		public MinMaxCurve GravityCurve = new();
		public MinMaxCurve RotationSpeedCurve = new();
		public MinMaxCurve RotationAttractionCurve = new();
		public MinMaxCurve DragCurve = new();
		public PPtr<Texture3D> VectorField = new();
		public MinMaxCurve VectorFieldSpeedCurve = new();
		public MinMaxCurve VectorFieldAttractionCurve = new();
	}
}
