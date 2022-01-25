using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Curves
{
	public struct PPtrCurve : IAsset, IDependent
	{
		public PPtrCurve(PPtrCurve copy, IReadOnlyList<PPtrKeyframe> keyframes) : this(copy.Path, copy.Attribute, copy.ClassID, copy.Script, keyframes) { }

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<IMonoScript> script)
		{
			Attribute = attribute;
			Path = path;
			ClassID = classID;
			Script = script;
			Curve = null;
		}

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<IMonoScript> script, IReadOnlyList<PPtrKeyframe> keyframes) : this(path, attribute, classID, script)
		{
			Curve = new PPtrKeyframe[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				Curve[i] = keyframes[i];
			}
		}

		public static bool operator ==(PPtrCurve left, PPtrCurve right)
		{
			if (left.Attribute != right.Attribute)
			{
				return false;
			}
			if (left.Path != right.Path)
			{
				return false;
			}
			if (left.ClassID != right.ClassID)
			{
				return false;
			}
			if (left.Script != right.Script)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(PPtrCurve left, PPtrCurve right)
		{
			if (left.Attribute != right.Attribute)
			{
				return true;
			}
			if (left.Path != right.Path)
			{
				return true;
			}
			if (left.ClassID != right.ClassID)
			{
				return true;
			}
			if (left.Script != right.Script)
			{
				return true;
			}
			return false;
		}

		public void Read(AssetReader reader)
		{
			Curve = reader.ReadAssetArray<PPtrKeyframe>();
			if (IsAlignCurve(reader.Version))
			{
				reader.AlignStream();
			}

			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Script.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Curve.Write(writer);
			if (IsAlignCurve(writer.Version))
			{
				writer.AlignStream();
			}

			writer.Write(Attribute);
			writer.Write(Path);
			writer.Write((int)ClassID);
			Script.Write(writer);
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Curve, CurveName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(Script, ScriptName);
		}

		public override bool Equals(object obj)
		{
			if (obj is PPtrCurve pptrCurve)
			{
				return this == pptrCurve;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 113;
			unchecked
			{
				hash = hash + 457 * Attribute.GetHashCode();
				hash = hash * 433 + Path.GetHashCode();
				hash = hash * 223 + ClassID.GetHashCode();
				hash = hash * 911 + Script.GetHashCode();
			}
			return hash;
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsAlignCurve(UnityVersion version) => version.IsGreaterEqual(2017);

		public PPtrKeyframe[] Curve { get; set; }
		public string Attribute { get; set; }
		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }

		public PPtr<IMonoScript> Script;

		public const string CurveName = "curve";
		public const string AttributeName = "attribute";
		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string ScriptName = "script";
	}
}
