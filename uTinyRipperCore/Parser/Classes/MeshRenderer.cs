using System.Collections.Generic;

namespace uTinyRipper.Classes
{
	public sealed class MeshRenderer : Renderer
	{
		public MeshRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadVertex(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadVertex(reader.Version, reader.Flags))
			{
				AdditionalVertexStreams.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AdditionalVertexStreams, AdditionalVertexStreamsName);
		}

		public const string AdditionalVertexStreamsName = "m_AdditionalVertexStreams";

		public PPtr<Mesh> AdditionalVertexStreams;
	}
}
