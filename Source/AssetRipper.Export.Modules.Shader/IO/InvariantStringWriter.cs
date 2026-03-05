using System.Globalization;

namespace AssetRipper.Export.Modules.Shaders.IO;

public sealed class InvariantStringWriter : StringWriter
{
	public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
}
