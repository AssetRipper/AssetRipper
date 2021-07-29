using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public sealed class BuildSettings : Classes.Object
    {
        public string m_Version;

        public BuildSettings(ObjectReader reader) : base(reader)
        {
            var levels = reader.ReadStringArray();

            var hasRenderTexture = reader.ReadBoolean();
            var hasPROVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            m_Version = reader.ReadAlignedString();
        }
    }
}
