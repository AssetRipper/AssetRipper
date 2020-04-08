using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class PPtrLayout
	{
		public PPtrLayout(LayoutInfo info)
		{
			// NOTE: unknown version SerializedFiles.FormatVersion.Unknown_14
			IsLongID = info.Version.IsGreaterEqual(5);
		}

		public static void GenerateTypeTree(TypeTreeContext context, string type, string name)
		{
			PPtrLayout layout = context.Layout.PPtr;
			context.AddNode($"PPtr<{type}>", name);
			context.BeginChildren();
			context.AddInt32(layout.FileIDName);
			if (layout.IsLongID)
			{
				context.AddInt64(layout.PathIDName);
			}
			else
			{
				context.AddInt32(layout.PathIDName);
			}
			context.EndChildren();
		}

		public bool HasFileIndex => true;
		public bool HasPathID => true;

		public bool IsLongID { get; }

		public string FileIDName => "m_FileID";
		public string PathIDName => "m_PathID";
	}
}
