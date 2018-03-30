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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_curves = stream.ReadArray<Channel>();
			AClassID = (ClassIDType)stream.ReadInt32();
		}

		public IReadOnlyList<Channel> Curves => m_curves;
		public ClassIDType AClassID { get; private set; }

		private Channel[] m_curves;
	}
}
