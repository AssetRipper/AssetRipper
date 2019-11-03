using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct AnimationEvent : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasObjectReferenceParameter(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasIntParameter(Version version) => version.IsGreaterEqual(3);

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();

			FunctionName = reader.ReadString();
			StringParameter = reader.ReadString();
			if (HasObjectReferenceParameter(reader.Version))
			{
				ObjectReferenceParameter.Read(reader);
				FloatParameter = reader.ReadSingle();
			}
			if (HasIntParameter(reader.Version))
			{
				IntParameter = reader.ReadInt32();
			}
			MessageOptions = reader.ReadInt32();
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(ObjectReferenceParameter, ObjectReferenceParameterName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TimeName, Time);
			node.Add(FunctionNameName, FunctionName);
			node.Add(DataName, StringParameter);
			node.Add(ObjectReferenceParameterName, ObjectReferenceParameter.ExportYAML(container));
			node.Add(FloatParameterName, FloatParameter);
			node.Add(IntParameterName, IntParameter);
			node.Add(MessageOptionsName, MessageOptions);
			return node;
		}

		public float Time { get; set; }
		public string FunctionName { get; set; }
		/// <summary>
		/// Data
		/// </summary>
		public string StringParameter { get; set; }
		public float FloatParameter { get; set; }
		public int IntParameter { get; set; }
		public int MessageOptions { get; set; }

		public const string TimeName = "time";
		public const string FunctionNameName = "functionName";
		public const string DataName = "data";
		public const string ObjectReferenceParameterName = "objectReferenceParameter";
		public const string FloatParameterName = "floatParameter";
		public const string IntParameterName = "intParameter";
		public const string MessageOptionsName = "messageOptions";

		public PPtr<Object> ObjectReferenceParameter;
	}
}
