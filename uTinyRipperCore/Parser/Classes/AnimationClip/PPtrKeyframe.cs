using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct PPtrKeyframe : IAssetReadable, IYAMLExportable, IDependent
	{
		public PPtrKeyframe(float time, PPtr<Object> script)
		{
			Time = time;
			Value = script;
		}

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Value.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TimeName, Time);
			node.Add(ValueName, Value.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Value, ValueName);
		}

		public float Time { get; private set; }

		public const string TimeName = "time";
		public const string ValueName = "value";

		public PPtr<Object> Value;
	}
}
