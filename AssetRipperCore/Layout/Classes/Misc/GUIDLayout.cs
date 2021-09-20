using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.Game;

namespace AssetRipper.Core.Layout.Classes.Misc
{
	public sealed class GUIDLayout
	{
		public GUIDLayout(LayoutInfo info) { }

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			GUIDLayout layout = context.Layout.Misc.GUID;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddUInt32(layout.Data0Name);
			context.AddUInt32(layout.Data1Name);
			context.AddUInt32(layout.Data2Name);
			context.AddUInt32(layout.Data3Name);
			context.EndChildren();
		}

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasData0 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasData1 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasData2 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasData3 => true;

		public string Name => nameof(UnityGUID);
		public string Data0Name => "data[0]";
		public string Data1Name => "data[1]";
		public string Data2Name => "data[2]";
		public string Data3Name => "data[3]";
	}
}
