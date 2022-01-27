using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class MeshRenderer : Renderer.Renderer
	{
		public MeshRenderer(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasVertex(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(5) && flags.IsRelease();

		/// <summary>
		/// 2020 and greater.
		/// </summary>
		public static bool HasEnlightenVertexStream(UnityVersion version) => version.IsGreaterEqual(2020);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasVertex(reader.Version, reader.Flags))
			{
				AdditionalVertexStreams.Read(reader);
			}

			if (HasEnlightenVertexStream(reader.Version))
			{
				EnlightenVertexStream.Read(reader);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AdditionalVertexStreams, AdditionalVertexStreamsName);
		}

		public const string AdditionalVertexStreamsName = "m_AdditionalVertexStreams";

		public PPtr<Mesh.Mesh> AdditionalVertexStreams = new();

		public PPtr<Mesh.Mesh> EnlightenVertexStream = new();
	}
}
