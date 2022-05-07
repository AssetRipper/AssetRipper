﻿using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class RotationBySpeedModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasAxes(UnityVersion version) => version.IsGreaterEqual(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAxes(reader.Version))
			{
				X.Read(reader);
				Y.Read(reader);
			}
			Curve.Read(reader);
			if (HasAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream();
			}

			Range.Read(reader);
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(XName, GetExportX(container.Version).ExportYaml(container));
			node.Add(YName, GetExportY(container.Version).ExportYaml(container));
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(SeparateAxesName, SeparateAxes);
			node.Add(RangeName, Range.ExportYaml(container));
			return node;
		}

		private MinMaxCurve GetExportX(UnityVersion version)
		{
			return HasAxes(version) ? X : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportY(UnityVersion version)
		{
			return HasAxes(version) ? Y : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxes { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string CurveName = "curve";
		public const string SeparateAxesName = "separateAxes";
		public const string RangeName = "range";

		public MinMaxCurve X = new();
		public MinMaxCurve Y = new();
		public MinMaxCurve Curve = new();
		public Vector2f Range = new();
	}
}
