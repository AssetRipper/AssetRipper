using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public class Transform : Component, ITransform
	{
		/// <summary>
		/// 4.5.0 and greater and Not Release
		/// </summary>
		public static bool HasRootOrder(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(4, 5) && !flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool HasLocalEulerAnglesHint(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && !flags.IsRelease();
		}
		
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasConstrainProportionsScale(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		public Transform(AssetLayout layout) : base(layout)
		{
			Children = Array.Empty<PPtr<Transform>>();
		}

		public Transform(AssetInfo assetInfo) : base(assetInfo) { }

		public string GetRootPath()
		{
			string pre = string.Empty;
			if (!Father.IsNull)
			{
				pre = Father.GetAsset(File).GetRootPath() + PathSeparator;
			}
			return pre + GameObject.GetAsset(File).Name;
		}

		public int GetSiblingIndex()
		{
			if (Father.IsNull)
			{
				return 0;
			}
			Transform father = Father.GetAsset(File);
			for (int i = 0; i < father.Children.Length; i++)
			{
				PPtr<Transform> child = father.Children[i];
				if (child.PathID == PathID)
				{
					return i;
				}
			}
			throw new Exception("Transorm hasn't been found among father's children");
		}

		public Transform FindChild(string path)
		{
			if (path.Length == 0)
			{
				return this;
			}
			return FindChild(path, 0);
		}

		public override IUnityObjectBase Convert(IExportContainer container)
		{
			return TransformConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			LocalRotation.Read(reader);
			LocalPosition.Read(reader);
			LocalScale.Read(reader);

			if (HasConstrainProportionsScale(reader.Version))
			{
				if (!reader.Flags.IsRelease())
				{
					reader.ReadBoolean(); //ConstraintProportionsScale
				}

				reader.AlignStream(); //Either way we have to align here.
			}
			
			Children = reader.ReadAssetArray<PPtr<Transform>>();
			Father.Read(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			LocalRotation.Write(writer);
			LocalPosition.Write(writer);
			LocalScale.Write(writer);
			Children.Write(writer);
			Father.Write(writer);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Children, ChildrenName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(Father, FatherName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(LocalRotationName, LocalRotation.ExportYAML(container));
			node.Add(LocalPositionName, LocalPosition.ExportYAML(container));
			node.Add(LocalScaleName, LocalScale.ExportYAML(container));
			node.Add(ChildrenName, Children.ExportYAML(container));
			node.Add(FatherName, Father.ExportYAML(container));
			node.Add(RootOrderName, RootOrder);
			node.Add(LocalEulerAnglesHintName, LocalEulerAnglesHint.ExportYAML(container));
			return node;
		}

		private Transform FindChild(string path, int startIndex)
		{
			int separatorIndex = path.IndexOf(PathSeparator, startIndex);
			string childName = separatorIndex == -1 ?
				path.Substring(startIndex, path.Length - startIndex) :
				path.Substring(startIndex, separatorIndex - startIndex);
			foreach (PPtr<Transform> childPtr in Children)
			{
				Transform child = childPtr.GetAsset(File);
				GameObject.GameObject childGO = child.GameObject.GetAsset(File);
				if (childGO.Name == childName)
				{
					return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
				}
			}
			return null;
		}

		public PPtr<Transform>[] Children { get; set; }
		private int RootOrder => GetSiblingIndex();
		private Vector3f LocalEulerAnglesHint => LocalRotation.ToEuler();

		public const char PathSeparator = '/';

		public Quaternionf LocalRotation;
		public Vector3f LocalPosition;
		public Vector3f LocalScale;
		public PPtr<Transform> Father;

		public const string TransformName = "Transform";
		public const string LocalRotationName = "m_LocalRotation";
		public const string LocalPositionName = "m_LocalPosition";
		public const string LocalScaleName = "m_LocalScale";
		public const string ChildrenName = "m_Children";
		public const string FatherName = "m_Father";
		public const string RootOrderName = "m_RootOrder";
		public const string LocalEulerAnglesHintName = "m_LocalEulerAnglesHint";
	}
}
