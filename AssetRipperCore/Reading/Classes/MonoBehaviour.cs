using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Reading.Classes
{
	public sealed class MonoBehaviour : Behaviour
	{
		public PPtr<MonoScript> m_Script;
		public string m_Name;

		public MonoBehaviour(ObjectReader reader) : base(reader)
		{
			m_Script = new PPtr<MonoScript>(reader);
			m_Name = reader.ReadAlignedString();
		}
	}
}
