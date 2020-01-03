using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	/// <summary>
	/// 3.5.0 - first introduction
	/// </summary>
	public sealed class GradientLayout
	{
		public GradientLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(5, 6))
			{
				// ColorRBGA32 has been replaced by ColorRBGAf
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			if (info.Version.IsLess(5, 6))
			{
				HasKey0_32 = true;
				HasKey1_32 = true;
				HasKey2_32 = true;
				HasKey3_32 = true;
				HasKey4_32 = true;
				HasKey5_32 = true;
				HasKey6_32 = true;
				HasKey7_32 = true;
			}
			else
			{
				HasKey0 = true;
				HasKey1 = true;
				HasKey2 = true;
				HasKey3 = true;
				HasKey4 = true;
				HasKey5 = true;
				HasKey6 = true;
				HasKey7 = true;
			}
			if (info.Version.IsGreaterEqual(5, 5))
			{
				HasMode = true;
			}

			if (info.Version.IsGreaterEqual(5, 6))
			{
				Name = TypeTreeUtils.GradientName;
			}
			else
			{
				Name = TypeTreeUtils.GradientNEWName;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			GradientLayout layout = context.Layout.Serialized.Gradient;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			if (layout.Version == 1)
			{
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key0Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key1Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key2Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key3Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key4Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key5Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key6Name);
				ColorRGBA32Layout.GenerateTypeTree(context, layout.Key7Name);
			}
			else
			{
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key0Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key1Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key2Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key3Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key4Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key5Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key6Name);
				ColorRGBAfLayout.GenerateTypeTree(context, layout.Key7Name);
			}

			context.AddInt16(layout.Ctime0Name);
			context.AddInt16(layout.Ctime1Name);
			context.AddInt16(layout.Ctime2Name);
			context.AddInt16(layout.Ctime3Name);
			context.AddInt16(layout.Ctime4Name);
			context.AddInt16(layout.Ctime5Name);
			context.AddInt16(layout.Ctime6Name);
			context.AddInt16(layout.Ctime7Name);

			context.AddInt16(layout.Atime0Name);
			context.AddInt16(layout.Atime1Name);
			context.AddInt16(layout.Atime2Name);
			context.AddInt16(layout.Atime3Name);
			context.AddInt16(layout.Atime4Name);
			context.AddInt16(layout.Atime5Name);
			context.AddInt16(layout.Atime6Name);
			context.AddInt16(layout.Atime7Name);

			if (layout.HasMode)
			{
				context.AddInt32(layout.ModeName);
			}
			context.AddByte(layout.NumColorKeysName);
			context.AddByte(layout.NumAlphaKeysName);
			context.EndChildren();
		}

		public int Version { get; }

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey0Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey1Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey2Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey3Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey4Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey5Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey6Invariant => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasKey7Invariant => true;
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey0_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey1_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey2_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey3_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey4_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey5_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey6_32 { get; }
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public bool HasKey7_32 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey0 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey1 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey2 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey3 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey4 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey5 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey6 { get; }
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public bool HasKey7 { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime0 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime1 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime2 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime3 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime4 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime5 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime6 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasCtime7 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime0 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime1 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime2 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime3 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime4 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime5 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime6 => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasAtime7 => true;
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public bool HasMode { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasNumColorKeys => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasNumAlphaKeys => true;

		public string Name { get; }
		public string Key0Name = "key0";
		public string Key1Name = "key1";
		public string Key2Name = "key2";
		public string Key3Name = "key3";
		public string Key4Name = "key4";
		public string Key5Name = "key5";
		public string Key6Name = "key6";
		public string Key7Name = "key7";
		public string Ctime0Name = "ctime0";
		public string Ctime1Name = "ctime1";
		public string Ctime2Name = "ctime2";
		public string Ctime3Name = "ctime3";
		public string Ctime4Name = "ctime4";
		public string Ctime5Name = "ctime5";
		public string Ctime6Name = "ctime6";
		public string Ctime7Name = "ctime7";
		public string Atime0Name = "atime0";
		public string Atime1Name = "atime1";
		public string Atime2Name = "atime2";
		public string Atime3Name = "atime3";
		public string Atime4Name = "atime4";
		public string Atime5Name = "atime5";
		public string Atime6Name = "atime6";
		public string Atime7Name = "atime7";
		public string ModeName = "m_Mode";
		public string NumColorKeysName = "m_NumColorKeys";
		public string NumAlphaKeysName = "m_NumAlphaKeys";
	}
}
