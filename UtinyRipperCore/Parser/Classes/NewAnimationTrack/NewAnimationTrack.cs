using System.Collections.Generic;
using UtinyRipper.Classes.NewAnimationTracks;

namespace UtinyRipper.Classes
{
	public sealed class NewAnimationTrack : BaseAnimationTrack
	{
		public NewAnimationTrack(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_curves = reader.ReadArray<Channel>();
			AClassID = (ClassIDType)reader.ReadInt32();
		}

		public IReadOnlyList<Channel> Curves => m_curves;
		public ClassIDType AClassID { get; private set; }

		private Channel[] m_curves;
	}
}
