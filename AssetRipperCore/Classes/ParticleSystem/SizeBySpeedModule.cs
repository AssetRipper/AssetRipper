﻿using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class SizeBySpeedModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasAxes(UnityVersion version) => version.IsGreaterEqual(5, 4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Curve.Read(reader);
			if (HasAxes(reader.Version))
			{
				Y.Read(reader);
				Z.Read(reader);
			}
			Range.Read(reader);
			if (HasAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(YName, GetExportY(container.Version).ExportYaml(container));
			node.Add(ZName, GetExportZ(container.Version).ExportYaml(container));
			node.Add(RangeName, Range.ExportYaml(container));
			node.Add(SeparateAxesName, SeparateAxes);
			return node;
		}

		private MinMaxCurve GetExportY(UnityVersion version)
		{
			return HasAxes(version) ? Y : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}
		private MinMaxCurve GetExportZ(UnityVersion version)
		{
			return HasAxes(version) ? Z : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}

		public bool SeparateAxes { get; set; }

		public const string CurveName = "curve";
		public const string YName = "y";
		public const string ZName = "z";
		public const string RangeName = "range";
		public const string SeparateAxesName = "separateAxes";

		public MinMaxCurve Curve = new();
		public MinMaxCurve Y = new();
		public MinMaxCurve Z = new();
		public Vector2f Range = new();
	}
}
