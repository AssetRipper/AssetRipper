using UnityEngine.UIElements;

namespace AssetRipper.Tests.Fixtures;

[UxmlElement]
internal class UxmlSourceFixture
{
	public string Value { get; set; } = "Preserved";

	public class UxmlSerializedData
	{
	}

	public class UnrelatedNestedType
	{
	}

	[UxmlObject]
	public class NestedObject
	{
		public class UxmlSerializedData
		{
		}
	}
}
