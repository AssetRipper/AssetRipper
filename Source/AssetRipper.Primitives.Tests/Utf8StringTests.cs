namespace AssetRipper.Primitives.Tests;

public class Utf8StringTests
{
	[Test]
	public void StringEmptyIsEquivalentToUtf8StringEmpty()
	{
		Assert.Multiple(() =>
		{
			Assert.That(Utf8String.Empty.Data.Length, Is.EqualTo(0));
			Assert.That(Utf8String.Empty.String, Is.Empty);
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
			Assert.That(Utf8String.Empty == "");
			Assert.That("" == Utf8String.Empty);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
			Assert.That(new Utf8String(""), Is.EqualTo(Utf8String.Empty));
		});
	}

	/// <summary>
	/// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/utf8-string-literals"/>
	/// </summary>
	[Test]
	public void Utf8StringMadeFromUtf8LiteralIsTheSameAsMadeFromConstantString()
	{
		Utf8String dataString = "AssetRipperTestString"u8;
		Utf8String systemString = "AssetRipperTestString";
		Assert.That(dataString, Is.EqualTo(systemString));
	}
}
