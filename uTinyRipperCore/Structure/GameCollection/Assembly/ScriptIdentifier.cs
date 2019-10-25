using System;

namespace uTinyRipper.Assembly
{
	public struct ScriptIdentifier
	{
		public ScriptIdentifier(string assembly, string @namespace, string name)
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

		public static string ToUniqueName(string assembly, string @namespace, string name)
		{
			return @namespace == string.Empty ? $"[{assembly}]{name}" : $"[{assembly}]{@namespace}.{name}";
		}

		public static string ToUniqueName(string assembly, string fullName)
		{
			return $"[{assembly}]{fullName}";
		}

		public static bool operator ==(ScriptIdentifier left, ScriptIdentifier right)
		{
			if (left.Assembly != right.Assembly)
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

		public static bool operator !=(ScriptIdentifier left, ScriptIdentifier right)
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
			if (obj.GetType() != typeof(ScriptIdentifier))
			{
				return false;
			}
			return this == (ScriptIdentifier)obj;
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

		public override string ToString()
		{
			return IsDefault ? base.ToString() : (Namespace == string.Empty ? $"{Name}" : $"{Namespace}.{Name}");
		}

		public bool IsDefault => Name == null;
		public string UniqueName => ToUniqueName(Assembly, Namespace, Name);

		public string Assembly { get; }
		public string Namespace { get; }
		public string Name { get; }
	}
}
