using AssetRipper.SourceGenerated.Subclasses.PropertyName;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PropertyNameExtensions
{
	public static Utf8String GetIdString(this IPropertyName _this)
	{
		//When looking at decompiled games where ID is represented as a string, it seems to always be a serialized int.
		return _this.Has_Id_Int32() ? _this.Id_Int32.ToString() : _this.Id_Utf8String;
	}
}
