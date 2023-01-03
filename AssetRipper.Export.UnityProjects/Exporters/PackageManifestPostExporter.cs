using AssetRipper.Core.IO;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters
{
	public sealed class PackageManifestPostExporter : IPostExporter
	{
		/// <summary>
		/// This should be consistent from 2019.2 to at least 2023
		/// </summary>
		private const string PackageJsonContentNew = """
			{
			  "dependencies": {
			    "com.unity.modules.ai": "1.0.0",
			    "com.unity.modules.androidjni": "1.0.0",
			    "com.unity.modules.animation": "1.0.0",
			    "com.unity.modules.assetbundle": "1.0.0",
			    "com.unity.modules.audio": "1.0.0",
			    "com.unity.modules.cloth": "1.0.0",
			    "com.unity.modules.director": "1.0.0",
			    "com.unity.modules.imageconversion": "1.0.0",
			    "com.unity.modules.imgui": "1.0.0",
			    "com.unity.modules.jsonserialize": "1.0.0",
			    "com.unity.modules.particlesystem": "1.0.0",
			    "com.unity.modules.physics": "1.0.0",
			    "com.unity.modules.physics2d": "1.0.0",
			    "com.unity.modules.screencapture": "1.0.0",
			    "com.unity.modules.terrain": "1.0.0",
			    "com.unity.modules.terrainphysics": "1.0.0",
			    "com.unity.modules.tilemap": "1.0.0",
			    "com.unity.modules.ui": "1.0.0",
			    "com.unity.modules.uielements": "1.0.0",
			    "com.unity.modules.umbra": "1.0.0",
			    "com.unity.modules.unityanalytics": "1.0.0",
			    "com.unity.modules.unitywebrequest": "1.0.0",
			    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
			    "com.unity.modules.unitywebrequestaudio": "1.0.0",
			    "com.unity.modules.unitywebrequesttexture": "1.0.0",
			    "com.unity.modules.unitywebrequestwww": "1.0.0",
			    "com.unity.modules.vehicles": "1.0.0",
			    "com.unity.modules.video": "1.0.0",
			    "com.unity.modules.vr": "1.0.0",
			    "com.unity.modules.wind": "1.0.0",
			    "com.unity.modules.xr": "1.0.0"
			  }
			}
			""";

		/// <summary>
		/// This should be consistent from the beginning until 2019.2, when com.unity.modules.androidjni was added.
		/// </summary>
		private const string PackageJsonContentOld = """
			{
			  "dependencies": {
			    "com.unity.modules.ai": "1.0.0",
			    "com.unity.modules.animation": "1.0.0",
			    "com.unity.modules.assetbundle": "1.0.0",
			    "com.unity.modules.audio": "1.0.0",
			    "com.unity.modules.cloth": "1.0.0",
			    "com.unity.modules.director": "1.0.0",
			    "com.unity.modules.imageconversion": "1.0.0",
			    "com.unity.modules.imgui": "1.0.0",
			    "com.unity.modules.jsonserialize": "1.0.0",
			    "com.unity.modules.particlesystem": "1.0.0",
			    "com.unity.modules.physics": "1.0.0",
			    "com.unity.modules.physics2d": "1.0.0",
			    "com.unity.modules.screencapture": "1.0.0",
			    "com.unity.modules.terrain": "1.0.0",
			    "com.unity.modules.terrainphysics": "1.0.0",
			    "com.unity.modules.tilemap": "1.0.0",
			    "com.unity.modules.ui": "1.0.0",
			    "com.unity.modules.uielements": "1.0.0",
			    "com.unity.modules.umbra": "1.0.0",
			    "com.unity.modules.unityanalytics": "1.0.0",
			    "com.unity.modules.unitywebrequest": "1.0.0",
			    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
			    "com.unity.modules.unitywebrequestaudio": "1.0.0",
			    "com.unity.modules.unitywebrequesttexture": "1.0.0",
			    "com.unity.modules.unitywebrequestwww": "1.0.0",
			    "com.unity.modules.vehicles": "1.0.0",
			    "com.unity.modules.video": "1.0.0",
			    "com.unity.modules.vr": "1.0.0",
			    "com.unity.modules.wind": "1.0.0",
			    "com.unity.modules.xr": "1.0.0"
			  }
			}
			""";

		public void DoPostExport(Ripper ripper)
		{
			string packagesDirectory = Path.Combine(ripper.Settings.ProjectRootPath, "Packages");
			Directory.CreateDirectory(packagesDirectory);
			using Stream fileStream = File.Create(Path.Combine(packagesDirectory, "manifest.json"));
			using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
			if (ripper.Settings.Version.IsGreaterEqual(2019, 2))
			{
				writer.Write(PackageJsonContentNew);
			}
			else
			{
				writer.Write(PackageJsonContentOld);
			}
		}
	}
}
