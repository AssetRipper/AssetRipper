using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files;
using System.IO;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes with Type ID numbers<br/>
	/// In other words, the classes that inherit from Object
	/// </summary>
	public class UnityObjectBase : UnityAssetBase
	{
		public virtual long PathID => -1;
		public virtual ClassIDType ClassID => ClassIDType.UnknownType;
		public virtual string ExportPath => Path.Combine(AssetsKeyword, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public UnityVersion BundleUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		public HideFlags ObjectHideFlags { get; set; }

		public const string AssetsKeyword = "Assets";
		protected const string AssetExtension = "asset";
	}
}
