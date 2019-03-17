using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystemForceFields
{
	public struct ParticleSystemForceFieldParameters : IAssetReadable, IYAMLExportable
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
			reader.AlignStream(AlignType.Align4);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return VectorField.FetchDependency(file, isLog, () => nameof(ParticleSystemForceFieldParameters), VectorFieldName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ShapeName, (int)Shape);
			node.Add(StartRangeName, StartRange);
			node.Add(EndRangeName, EndRange);
			node.Add(LengthName, Length);
			node.Add(GravityFocusName, GravityFocus);
			node.Add(RotationRandomnessName, RotationRandomness.ExportYAML(container));
			node.Add(DirectionCurveXName, DirectionCurveX.ExportYAML(container));
			node.Add(DirectionCurveYName, DirectionCurveY.ExportYAML(container));
			node.Add(DirectionCurveZName, DirectionCurveZ.ExportYAML(container));
			node.Add(GravityCurveName, GravityCurve.ExportYAML(container));
			node.Add(RotationSpeedCurveName, RotationSpeedCurve.ExportYAML(container));
			node.Add(RotationAttractionCurveName, RotationAttractionCurve.ExportYAML(container));
			node.Add(DragCurveName, DragCurve.ExportYAML(container));
			node.Add(VectorFieldName, VectorField.ExportYAML(container));
			node.Add(VectorFieldSpeedCurveName, VectorFieldSpeedCurve.ExportYAML(container));
			node.Add(VectorFieldAttractionCurveName, VectorFieldAttractionCurve.ExportYAML(container));
			node.Add(MultiplyDragByParticleSizeName, MultiplyDragByParticleSize);
			node.Add(MultiplyDragByParticleVelocityName, MultiplyDragByParticleVelocity);
			return node;
		}

		public ParticleSystemForceFieldShape Shape { get; private set; }
		public float StartRange { get; private set; }
		public float EndRange { get; private set; }
		public float Length { get; private set; }
		public float GravityFocus { get; private set; }
		public bool MultiplyDragByParticleSize { get; private set; }
		public bool MultiplyDragByParticleVelocity { get; private set; }

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

		public Vector2f RotationRandomness;
		public MinMaxCurve DirectionCurveX;
		public MinMaxCurve DirectionCurveY;
		public MinMaxCurve DirectionCurveZ;
		public MinMaxCurve GravityCurve;
		public MinMaxCurve RotationSpeedCurve;
		public MinMaxCurve RotationAttractionCurve;
		public MinMaxCurve DragCurve;
		public PPtr<Texture3D> VectorField;
		public MinMaxCurve VectorFieldSpeedCurve;
		public MinMaxCurve VectorFieldAttractionCurve;
	}
}
