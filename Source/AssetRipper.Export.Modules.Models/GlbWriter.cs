using SharpGLTF.Scenes;

namespace AssetRipper.Export.Modules.Models;

public static class GlbWriter
{
	public static bool TryWrite(SceneBuilder sceneBuilder, Stream stream, [NotNullWhen(false)] out string? errorMessage)
	{
		// Setting MergeBuffers to false doesn't actually do anything because SharpGLTF changes it back to true.
		try
		{
			sceneBuilder.ToGltf2().WriteGLB(stream);
			errorMessage = null;
			return true;
		}
		catch (InvalidOperationException ex) when (ex.Message is "Can't merge a buffer larger than 2Gb")
		{
			errorMessage = "Model was too large to export as GLB.";
			return false;
		}
		catch (ArgumentException ex) when (ex.Message is "the combined size of all the meshes exceeds the maximum capacity of the buffers, try disabling Buffers merging")
		{
			errorMessage = "Model was too large to export as GLB.";
			return false;
		}
		catch (OutOfMemoryException)
		{
			errorMessage = "Could not allocate enough contiguous memory to export the model as GLB.";
			return false;
		}
	}
}
