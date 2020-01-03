using uTinyRipper.YAML;
using uTinyRipper.Classes.TrailRenderers;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public sealed class TrailRenderer : Renderer
	{
		public TrailRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// Width and Color has been replaced by Parameters
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasParameters(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasEmitting(Version version) => version.IsGreaterEqual(2018, 2);

		public override Object Convert(IExportContainer container)
		{
			return TrailRendererConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Time = reader.ReadSingle();
			if (HasParameters(reader.Version))
			{
				Parameters.Read(reader);
			}
			else
			{
				float startWidth = reader.ReadSingle();
				float endWidth = reader.ReadSingle();
				Parameters.WidthCurve = new AnimationCurveTpl<Float>(startWidth, endWidth, KeyframeTpl<Float>.DefaultFloatWeight);
				Colors.Read(reader);
			}
			MinVertexDistance = reader.ReadSingle();
			Autodestruct = reader.ReadBoolean();
			if (HasEmitting(reader.Version))
			{
				Emitting = reader.ReadBoolean();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			writer.Write(Time);
			if (HasParameters(writer.Version))
			{
				Parameters.Write(writer);
			}
			else
			{
				writer.Write(StartWidth);
				writer.Write(EndWidth);
				Colors.Write(writer);
			}
			writer.Write(MinVertexDistance);
			writer.Write(Autodestruct);
			if (HasEmitting(writer.Version))
			{
				writer.Write(Emitting);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			if (HasParameters(container.ExportVersion))
			{
				node.Add(ParametersName, Parameters.ExportYAML(container));
			}
			else
			{
				node.Add(StartWidthName, StartWidth);
				node.Add(EndWidthName, EndWidth);
				node.Add(ColorsName, Colors.ExportYAML(container));
			}
			node.Add(MinVertexDistanceName, MinVertexDistance);
			node.Add(AutodestructName, Autodestruct);
			if (HasEmitting(container.ExportVersion))
			{
				node.Add(EmittingName, Emitting);
			}
			return node;
		}

		public float Time { get; set; }
		public float StartWidth => Parameters.WidthCurve.Curve[0].Value;
		public float EndWidth => Parameters.WidthCurve.Curve[1].Value;
		public float MinVertexDistance { get; set; }
		public bool Autodestruct { get; set; }
		public bool Emitting { get; set; }

		public const string TimeName = "m_Time";
		public const string StartWidthName = "m_StartWidth";
		public const string EndWidthName = "m_EndWidth";
		public const string ColorsName = "m_Colors";
		public const string ParametersName = "m_Parameters";
		public const string MinVertexDistanceName = "m_MinVertexDistance";
		public const string AutodestructName = "m_Autodestruct";
		public const string EmittingName = "m_Emitting";

		public TrailRenderers.Gradient Colors;
		public LineParameters Parameters;
	}
}
