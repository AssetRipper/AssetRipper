using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class MeshRenderer : Renderer
	{
		public MeshRenderer(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadVertex(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			if (IsReadVertex(stream.Version))
			{
				AdditionalVertexStreams.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if(!AdditionalVertexStreams.IsNull)
			{
				yield return AdditionalVertexStreams.FetchDependency(file, isLog, ToLogString, "m_AdditionalVertexStreams");
			}
		}
		
		public PPtr<Mesh> AdditionalVertexStreams;
	}
}
