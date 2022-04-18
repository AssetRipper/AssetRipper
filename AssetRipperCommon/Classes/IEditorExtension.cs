using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IEditorExtension : IUnityObjectBase
	{
		/// <summary>
		/// Added in 2018.2 as m_PrefabInternal<br/>
		/// Changed in 2018.3 to m_PrefabInstance
		/// </summary>
		PPtr<IPrefabInstance> PrefabInstance { get; set; }
	}
}