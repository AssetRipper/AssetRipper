using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Equality;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public abstract class UnityAssetBase : IUnityAssetBase, IAlmostEquatable
	{
		public virtual void ReadEditor(AssetReader reader) => throw new NotSupportedException();

		public virtual void ReadRelease(AssetReader reader) => throw new NotSupportedException();

		public void Read(AssetReader reader)
		{
			if (reader.Flags.IsRelease())
			{
				ReadRelease(reader);
			}
			else
			{
				ReadEditor(reader);
			}
		}

		public virtual void WriteEditor(AssetWriter writer) => throw new NotSupportedException();

		public virtual void WriteRelease(AssetWriter writer) => throw new NotSupportedException();

		public void Write(AssetWriter writer)
		{
			if (writer.Flags.IsRelease())
			{
				WriteRelease(writer);
			}
			else
			{
				WriteEditor(writer);
			}
		}

		public virtual YamlNode ExportYamlEditor(IExportContainer container) => throw new NotSupportedException($"Editor yaml export is not supported for {GetType().FullName}");

		public virtual YamlNode ExportYamlRelease(IExportContainer container) => throw new NotSupportedException($"Release yaml export is not supported for {GetType().FullName}");

		public YamlNode ExportYaml(IExportContainer container)
		{
			if (container.ExportFlags.IsRelease())
			{
				return ExportYamlRelease(container);
			}
			else
			{
				return ExportYamlEditor(container);
			}
		}

		public virtual IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			return Array.Empty<PPtr<IUnityObjectBase>>();
		}

		public virtual List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		public virtual List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		public override string? ToString()
		{
			return this is IHasNameString hasName ? hasName.NameString : base.ToString();
		}

		private bool HasEqualMetadata([NotNullWhen(true)] object? obj)
		{
			if (obj is null)
			{
				return false;
			}

			if (this.GetType() != obj.GetType())
			{
				return false;
			}

			if (obj is UnityObjectBase unityObjectBase)
			{
				UnityObjectBase thisObject = (UnityObjectBase)this;
				return thisObject.SerializedFile == unityObjectBase.SerializedFile &&
					thisObject.ClassID == unityObjectBase.ClassID &&
					thisObject.PathID == unityObjectBase.PathID &&
					thisObject.GUID == unityObjectBase.GUID;
			}
			else
			{
				return true;
			}
		}

		/// <inheritdoc/>
		public bool AlmostEqualByProportion(object value, float maximumProportion)
		{
			if (HasEqualMetadata(value))
			{
				return AlmostEqualByProportion((UnityAssetBase)value, maximumProportion);
			}
			else
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public bool AlmostEqualByDeviation(object value, float maximumDeviation)
		{
			if (HasEqualMetadata(value))
			{
				return AlmostEqualByDeviation((UnityAssetBase)value, maximumDeviation);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Check if two objects are almost equal to each other by proportion
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="value">Another asset with the same type as this</param>
		/// <param name="maximumProportion"></param>
		/// <returns>True if the objects are equal or almost equal by proportion</returns>
		protected virtual bool AlmostEqualByProportion(UnityAssetBase value, float maximumProportion)
		{
			return ReferenceEquals(this, value);
		}

		/// <summary>
		/// Check if two objects are almost equal to each other by deviation
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="value">Another asset with the same type as this</param>
		/// <param name="maximumDeviation">The positive maximum value deviation between two near equal decimal values</param>
		/// <returns>True if the objects are equal or almost equal by deviation</returns>
		protected virtual bool AlmostEqualByDeviation(UnityAssetBase value, float maximumDeviation)
		{
			return ReferenceEquals(this, value);
		}
	}
}
