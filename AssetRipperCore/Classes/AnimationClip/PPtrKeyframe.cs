using AssetRipper.Core.Converters.Game;
using AssetRipper.Core.Project;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public class PPtrKeyframe : IAsset, IDependent
	{
		public PPtrKeyframe() { }
		public PPtrKeyframe(float time, PPtr<Object.Object> script)
		{
			Time = time;
			Value = script;
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(PPtrKeyframe), name);
			context.BeginChildren();
			context.AddSingle(TimeName);
			context.AddPPtr(nameof(Object.Object), ValueName);
			context.EndChildren();
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

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Value, ValueName);
		}

		public float Time { get; set; }

		public const string TimeName = "time";
		public const string ValueName = "value";

		public PPtr<Object.Object> Value = new();
	}
}
