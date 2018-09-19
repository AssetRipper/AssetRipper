using System;

namespace UtinyRipper.AssetExporters
{
	public struct ScriptInfo
	{
		public ScriptInfo(string assembly, string @namespace, string name)
		{
			if (string.IsNullOrEmpty(assembly))
			{
				throw new ArgumentNullException(nameof(assembly));
			}
			if (@namespace == null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}
			Assembly = assembly;
			Namespace = @namespace;
			Name = name;
		}

		public static bool operator ==(ScriptInfo left, ScriptInfo right)
		{
			if(left.Assembly != right.Assembly)
			{
				return false;
			}
			if (left.Namespace != right.Namespace)
			{
				return false;
			}
			if (left.Name != right.Name)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(ScriptInfo left, ScriptInfo right)
		{
			if (left.Assembly != right.Assembly)
			{
				return true;
			}
			if (left.Namespace != right.Namespace)
			{
				return true;
			}
			if (left.Name != right.Name)
			{
				return true;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(ScriptInfo))
			{
				return false;
			}
			return this == (ScriptInfo)obj;
		}

		public override int GetHashCode()
		{
			int hash = 317;
			unchecked
			{
				hash = hash + 89 * Assembly.GetHashCode();
				hash = hash * 79 + Namespace.GetHashCode();
				hash = hash * 37 + Name.GetHashCode();
			}
			return hash;
		}

		public string Assembly { get; }
		public string Namespace { get; }
		public string Name { get; }
	}
}
