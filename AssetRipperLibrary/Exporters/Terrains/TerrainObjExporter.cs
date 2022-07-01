using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters.Terrains
{
	public sealed class TerrainObjExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITerrainData;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new TerrainObjExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			string text = ExportTerrainToObj((ITerrainData)asset);
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}

			using (FileStream fileStream = File.Create(path))
			{
				using StreamWriter sw = new StreamWriter(fileStream);
				sw.Write(text);
			}
			return true;
		}

		private static string ExportTerrainToObj(ITerrainData terrain)
		{
			StringBuilder sb = new StringBuilder();
			int tw = Math.Max(terrain.Heightmap_C156.Width, terrain.Heightmap_C156.Resolution);
			int th = Math.Max(terrain.Heightmap_C156.Height, terrain.Heightmap_C156.Resolution);

			//Vector3f meshScale = terrain.Heightmap.Scale;
			Vector3f meshScale = new Vector3f(-1, 1, 1);
			meshScale = new Vector3f(meshScale.X / (tw - 1), meshScale.Y, meshScale.Z / (th - 1));
			Vector2f uvScale = new Vector2f(1.0f / (tw - 1), 1.0f / (th - 1));

			int w = th;
			int h = tw;

			int startX = 0;
			int startY = 0;

			float[,] tData = GetHeights(terrain);
			Vector3f[] tVertices = new Vector3f[w * h];
			Vector2f[] tUV = new Vector2f[w * h];

			int[] tPolys = new int[(w - 1) * (h - 1) * 6];

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					Vector3f pos = new Vector3f(-(startY + y), tData[startX + x, startY + y], (startX + x));
					tVertices[y * w + x] = Vector3f.Scale(meshScale, pos);
					tUV[y * w + x] = Vector2f.Scale(new Vector2f(x, y), uvScale);
				}
			}
			int index = 0;
			for (int y = 0; y < h - 1; y++)
			{
				for (int x = 0; x < w - 1; x++)
				{
					tPolys[index++] = (y * w) + x;
					tPolys[index++] = ((y + 1) * w) + x;
					tPolys[index++] = (y * w) + x + 1;
					tPolys[index++] = ((y + 1) * w) + x;
					tPolys[index++] = ((y + 1) * w) + x + 1;
					tPolys[index++] = (y * w) + x + 1;
				}
			}

			for (int i = 0; i < tVertices.Length; i++)
			{
				sb.AppendFormat("v {0} {1} {2}\n", tVertices[i].X, tVertices[i].Y, tVertices[i].Z);
			}
			for (int i = 0; i < tUV.Length; i++)
			{
				sb.AppendFormat("vt {0} {1}\n", tUV[i].X, tUV[i].Y);
			}
			for (int i = 0; i < tPolys.Length; i += 3)
			{
				int x = tPolys[i] + 1;
				int y = tPolys[i + 1] + 1;
				int z = tPolys[i + 2] + 1;
				sb.AppendFormat("f {0} {1} {2}\n", x, y, z);
			}
			return sb.ToString();
		}

		private static float[,] GetHeights(ITerrainData terrain)
		{
			int width = Math.Max(terrain.Heightmap_C156.Width, terrain.Heightmap_C156.Resolution);
			int height = Math.Max(terrain.Heightmap_C156.Height, terrain.Heightmap_C156.Resolution);
			short[] heights = terrain.Heightmap_C156.Heights;
			float[,] result = new float[width, height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					result[x, y] = (float)heights[x + y * width] / short.MaxValue;
				}
			}
			return result;
		}
	}
}
