using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationEvent : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadObjectReferenceParameter(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadIntParameter(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();

			FunctionName = stream.ReadStringAligned();
			StringParameter = stream.ReadStringAligned();
			if (IsReadObjectReferenceParameter(stream.Version))
			{
				ObjectReferenceParameter.Read(stream);
				FloatParameter = stream.ReadSingle();
			}
			if (IsReadIntParameter(stream.Version))
			{
				IntParameter = stream.ReadInt32();
			}
			MessageOptions = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("time", Time);
			node.Add("functionName", FunctionName);
			node.Add("data", StringParameter);
			node.Add("objectReferenceParameter", ObjectReferenceParameter.ExportYAML(exporter));
			node.Add("floatParameter", FloatParameter);
			node.Add("intParameter", IntParameter);
			node.Add("messageOptions", MessageOptions);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			if(!ObjectReferenceParameter.IsNull)
			{
				Object @object = ObjectReferenceParameter.FindObject(file);
				if (@object == null)
				{
					if (isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"AnimationEvent's objectReferenceParameter {ObjectReferenceParameter.ToLogString(file)} wasn't found ");
					}
				}
				else
				{
					yield return @object;
				}
			}
		}

		public float Time { get; private set; }
		public string FunctionName { get; private set; }
		public string StringParameter { get; private set; }
		public float FloatParameter { get; private set; }
		public int IntParameter { get; private set; }
		public int MessageOptions { get; private set; }

		public PPtr<Object> ObjectReferenceParameter;
	}
}
