using System.Collections.Generic;

namespace SevenZip.CommandLineParser
{
	public enum SwitchType
	{
		Simple,
		PostMinus,
		LimitedPostString,
		UnLimitedPostString,
		PostChar
	}

	public class SwitchForm
	{
		public string IDString;
		public SwitchType Type;
		public bool Multi;
		public int MinLen;
		public int MaxLen;
		public string PostCharSet;

		public SwitchForm(string idString, SwitchType type, bool multi,
			int minLen, int maxLen, string postCharSet)
		{
			IDString = idString;
			Type = type;
			Multi = multi;
			MinLen = minLen;
			MaxLen = maxLen;
			PostCharSet = postCharSet;
		}
		public SwitchForm(string idString, SwitchType type, bool multi, int minLen) : this(idString, type, multi, minLen, 0, "") { }
		public SwitchForm(string idString, SwitchType type, bool multi) : this(idString, type, multi, 0) { }
	}

	public class SwitchResult
	{
		public bool ThereIs;
		public bool WithMinus;
		public List<string> PostStrings = new List<string>();
		public int PostCharIndex;
		public SwitchResult()
		{
			ThereIs = false;
		}
	}

	public class CommandForm
	{
		public string IDString = "";
		public bool PostStringMode = false;
		public CommandForm(string idString, bool postStringMode)
		{
			IDString = idString;
			PostStringMode = postStringMode;
		}
	}

	class CommandSubCharsSet
	{
		public string Chars = "";
		public bool EmptyAllowed = false;
	}
}
