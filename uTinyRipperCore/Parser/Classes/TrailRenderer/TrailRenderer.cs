using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.TrailRenderers;
using uTinyRipper.Classes.ParticleSystems;

namespace uTinyRipper.Classes
{
	public sealed class TrailRenderer : Renderer
	{
		public TrailRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadRenderer(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadParameters(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2019.2.0 and greater
		/// </summary>
		public static bool IsReadEmitting(Version version)
		{
			return version.IsGreaterEqual(2018, 2, 2);
		}
		private static int GetSerializedVersion(Version version)
		{
			// LineParameters has been added
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			if (IsReadRenderer(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadBase(reader);
			}
			Time = reader.ReadSingle();
			if (IsReadParameters(reader.Version))
			{
				Parameters.Read(reader);
			}
			else
			{
				StartWidth = reader.ReadSingle();
				EndWidth = reader.ReadSingle();
				Colors.Read(reader);
			}
			MinVertexDistance = reader.ReadSingle();
			Autodestruct = reader.ReadBoolean();
			if (IsReadEmitting(reader.Version))
			{
				Emitting = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			if (IsReadParameters(container.ExportVersion))
			{
				node.Add(ParametersName, GetParameters(container.Version).ExportYAML(container));
			}
			else
			{
				node.Add(StartWidthName, StartWidth);
				node.Add(EndWidthName, EndWidth);
				node.Add(ColorsName, Colors.ExportYAML(container));
			}
			node.Add(MinVertexDistanceName, MinVertexDistance);
			node.Add(AutodestructName, Autodestruct);
			if (IsReadEmitting(container.ExportVersion))
			{
				node.Add(EmittingName, Emitting);
			}
			return node;
		}

		private LineParameters GetParameters(Version version)
		{
			if (IsReadParameters(version))
			{
				return Parameters;
			} else
			{
				LineParameters parameters = new LineParameters();
				parameters.ColorGradient = Colors;
				parameters.WidthCurve = new AnimationClips.AnimationCurveTpl<Float>(StartWidth, EndWidth, Float.DefaultWeight);
				return parameters;
			}
		}

		public float Time { get; private set; }
		public float StartWidth { get; private set; }
		public float EndWidth { get; private set; }
		public float MinVertexDistance { get; private set; }
		public bool Autodestruct { get; private set; }
		public bool Emitting { get; private set; }

		public const string TimeName = "m_Time";
		public const string StartWidthName = "m_StartWidth";
		public const string EndWidthName = "m_EndWidth";
		public const string ColorsName = "m_Colors";
		public const string ParametersName = "m_Parameters";
		public const string MinVertexDistanceName = "m_MinVertexDistance";
		public const string AutodestructName = "m_Autodestruct";
		public const string EmittingName = "m_Emitting";

		public LineParameters Parameters;
		public Gradient Colors;
	}
}
