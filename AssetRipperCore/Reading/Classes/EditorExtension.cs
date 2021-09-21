using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Reading.Classes
{
	public abstract class EditorExtension : Classes.Object
	{
		protected EditorExtension(ObjectReader reader) : base(reader)
		{
			if (platform == Platform.NoTarget)
			{
				var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
				var m_PrefabInternal = new PPtr<Classes.Object>(reader); //PPtr<Prefab>
			}
		}
	}
}
