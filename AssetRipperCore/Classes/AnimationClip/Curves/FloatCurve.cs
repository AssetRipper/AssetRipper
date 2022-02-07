using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Curves
{
	public sealed class FloatCurve : IAsset, IDependent, IEquatable<FloatCurve>
	{
		public FloatCurve() { }

		public FloatCurve(FloatCurve copy, IReadOnlyList<KeyframeTpl<Float>> keyframes) : this(copy.Path, copy.Attribute, copy.ClassID, copy.Script, keyframes) { }

		public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<IMonoScript> script)
		{
			Path = path;
			Attribute = attribute;
			ClassID = classID;
			Script = script;
			Curve = new AnimationCurveTpl<Float>(false);
		}

		public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<IMonoScript> script, IReadOnlyList<KeyframeTpl<Float>> keyframes)
		{
			Path = path;
			Attribute = attribute;
			ClassID = classID;
			Script = script;
			Curve = new AnimationCurveTpl<Float>(keyframes);
		}

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Script.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Curve.Write(writer);
			writer.Write(Attribute);
			writer.Write(Path);
			writer.Write((int)ClassID);
			Script.Write(writer);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Script, ScriptName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(AttributeName, Attribute);
			node.Add(PathName, Path);
			node.Add(ClassIDName, (int)ClassID);
			node.Add(ScriptName, Script.ExportYAML(container));
			return node;
		}

		public override bool Equals(object obj)
		{
			if (obj is FloatCurve curve)
				return Equals(curve);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(FloatCurve other)
		{
			return 
				ClassID == other.ClassID &&
				Script == other.Script &&
				Attribute == other.Attribute &&
				Path == other.Path &&
				Curve.Equals(other.Curve);
		}

		public string Attribute { get; set; }
		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }
		public AnimationCurveTpl<Float> Curve { get; set; } = new();
		public PPtr<IMonoScript> Script { get; set; } = new();

		public const string CurveName = "curve";
		public const string AttributeName = "attribute";
		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string ScriptName = "script";
	}
}
