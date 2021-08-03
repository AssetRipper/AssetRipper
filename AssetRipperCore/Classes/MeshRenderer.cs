using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class MeshRenderer : Renderer.Renderer
	{
		public MeshRenderer(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasVertex(Version version, TransferInstructionFlags flags) => version.IsGreaterEqual(5) && flags.IsRelease();

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasVertex(reader.Version, reader.Flags))
			{
				AdditionalVertexStreams.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AdditionalVertexStreams, AdditionalVertexStreamsName);
		}

		public const string AdditionalVertexStreamsName = "m_AdditionalVertexStreams";

		public PPtr<Mesh.Mesh> AdditionalVertexStreams;
	}
}
