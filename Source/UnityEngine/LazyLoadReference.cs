namespace UnityEngine;

public struct LazyLoadReference<T> where T : Object
{
	private long m_InstanceID;
}
