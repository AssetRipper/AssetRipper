using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class PPtrKeyframe : IAsset, IDependent, IEquatable<PPtrKeyframe>
	{
		public PPtrKeyframe() { }
		public PPtrKeyframe(float time, PPtr<Object.Object> script)
		{
			Time = time;
			Value = script;
		}

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Value.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Time);
			Value.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TimeName, Time);
			node.Add(ValueName, Value.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Value, ValueName);
		}

		public override bool Equals(object obj)
		{
			if (obj is PPtrKeyframe keyframe)
			{
				return this.Equals(keyframe);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(PPtrKeyframe other)
		{
			return Time == other.Time && Value == other.Value;
		}

		public float Time { get; set; }

		public const string TimeName = "time";
		public const string ValueName = "value";

		public PPtr<Object.Object> Value = new();
	}
}
