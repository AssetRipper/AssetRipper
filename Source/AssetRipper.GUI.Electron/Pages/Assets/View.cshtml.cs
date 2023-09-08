using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_82;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;
		public IUnityObjectBase Asset { get; private set; } = default!;

		public string ImageSource
		{
			get
			{
				DirectBitmap bitmap = Bitmap;
				if (bitmap != default)
				{
					MemoryStream stream = new();
					bitmap.SaveAsPng(stream);
					return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		private DirectBitmap Bitmap
		{
			get
			{
				switch (Asset)
				{
					case ITexture2D texture:
						{
							if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
							{
								return bitmap;
							}
						}
						goto default;
					case ITerrainData terrainData:
						return TerrainHeatmapExporter.GetBitmap(terrainData);
					default:
						return default;
				}
			}
		}

		public string Text
		{
			get
			{
				return Asset switch
				{
					IShader shader => DumpShaderDataAsText(shader),
					IMonoScript monoScript => DecompileMonoScript(monoScript),
					ITextAsset textAsset => textAsset.Script_C49,
					_ => "",
				};
			}
		}

		public IEnumerable<(string, IUnityObjectBase)> CustomReferenceProperties
		{
			get
			{
				List<(string, IUnityObjectBase)> list = new();
				if (Asset.MainAsset is not null && Asset.MainAsset != Asset)
				{
					list.Add(("Main Asset", Asset.MainAsset));
				}
				switch (Asset)
				{
					case IComponent component:
						{
							if (component.GameObject_C2P is { } gameObject)
							{
								list.Add(("GameObject", gameObject));
								if (component is IRenderer && gameObject.TryGetComponent<IMeshFilter>()?.Mesh_C33P is { } rendererMesh)
								{
									list.Add(("Mesh", rendererMesh));
								}
							}
							if (component is IMonoBehaviour monoBehaviour && monoBehaviour.Script_C114P is { } monoScript)
							{
								list.Add(("Script", monoScript));
							}
							else if (component is IMeshFilter meshFilter && meshFilter.Mesh_C33P is { } mesh)
							{
								list.Add(("Mesh", mesh));
							}
							else if (component is IAudioSource audioSource && audioSource.AudioClip_C82P is { } audioSourceClip)
							{
								list.Add(("Audio Clip", audioSourceClip));
							}
						}
						break;
					case IGameObject gameObject:
						{
							if (gameObject.TryGetComponent(out ITransform? transform))
							{
								list.Add(("Transform", transform));
							}
						}
						break;
					case IMaterial material:
						{
							if (material.Shader_C21P is { } shader)
							{
								list.Add(("Shader", shader));
							}
						}
						break;
					case SpriteInformationObject spriteInformationObject:
						{
							list.Add(("Texture", spriteInformationObject.Texture));
						}
						break;
					default:
						break;
				}
				return list.Count != 0 ? list : Enumerable.Empty<(string, IUnityObjectBase)>();
			}
		}

		public IEnumerable<(string, string)> CustomStringProperties
		{
			get
			{
				switch (Asset)
				{
					case IMonoScript monoScript:
						{
							return ToPropertyArray(monoScript);
						}
					case IMonoBehaviour monoBehaviour:
						{
							if (monoBehaviour.Script_C114P is { } monoScript)
							{
								return ToPropertyArray(monoScript);
							}
							else
							{
								goto default;
							}
						}
					case ITexture2D texture2D:
						{
							return new (string, string)[]
							{
								("Width", texture2D.Width_C28.ToString()),
								("Height", texture2D.Height_C28.ToString()),
								("Format", texture2D.Format_C28E.ToString()),
							};
						}
					case IMesh mesh:
						{
							return new (string, string)[]
							{
								("Vertex Count", mesh.VertexData_C43.VertexCount.ToString()),
								("Submesh Count", mesh.SubMeshes_C43.Count.ToString()),
							};
						}
					case IAudioClip audioClip:
						{
							return new (string, string)[]
							{
								("Channels", audioClip.Channels_C83.ToString()),
								("Frequency", audioClip.Frequency_C83.ToString()),
								("Length", audioClip.Length_C83.ToString()),
							};
						}
					case ITerrainData terrainData:
						{
							return new (string, string)[]
							{
								("Width", terrainData.Heightmap_C156.GetWidth().ToString()),
								("Height", terrainData.Heightmap_C156.GetHeight().ToString()),
							};
						}
					default:
						return Enumerable.Empty<(string, string)>();
				}

				static (string, string)[] ToPropertyArray(IMonoScript monoScript)
				{
					return new (string, string)[]
					{
						("Assembly Name", monoScript.AssemblyName_C115),
						("Namespace", monoScript.Namespace_C115),
						("Class Name", monoScript.ClassName_C115),
					};
				}
			}
		}

		public ViewModel(ILogger<ViewModel> logger)
		{
			_logger = logger;
		}

		public IActionResult OnGet(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogError("Path is null");
				return Redirect("/");
			}
			else if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetAsset(AssetPath.FromJson(path), out IUnityObjectBase? asset))
			{
				Asset = asset;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}

		private static string DumpShaderDataAsText(IShader shader)
		{
			MemoryStream stream = new();
			DummyShaderTextExporter.ExportShader(shader, stream);

			return Encoding.UTF8.GetString(GetStreamData(stream));

			static ReadOnlySpan<byte> GetStreamData(MemoryStream stream)
			{
				if (stream.TryGetBuffer(out ArraySegment<byte> buffer))
				{
					return buffer;
				}
				else
				{
					return stream.ToArray();
				}
			}
		}

		private static string DecompileMonoScript(IMonoScript monoScript)
		{
			return EmptyScript.GetContent(monoScript);//Placeholder until actual decompilation is implemented.
		}
	}
}
