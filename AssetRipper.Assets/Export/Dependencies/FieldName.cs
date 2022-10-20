using System.Text;

namespace AssetRipper.Assets.Export.Dependencies
{
	public sealed class FieldName
	{
		public static FieldName Empty { get; } = new FieldName("");

		public FieldName(string name)
		{
			Name = name;
		}

		public FieldName(string name, FieldName? parent)
		{
			Name = name;
			Parent = parent;
		}

		public string Name { get; }
		public FieldName? Parent { get; }

		public override string ToString()
		{
			if (Parent is null)
			{
				return Name;
			}

			StringBuilder sb = new();
			FieldName? current = this;
			while (current != null)
			{
				if (sb.Length > 0)
				{
					sb.Append('.');
				}
				string name = current.Name;
				for (int i = name.Length - 1; i >= 0; i--)
				{
					sb.Append(name[i]);
				}
				current = current.Parent;
			}

			ReverseCharacterOrder(sb);

			return sb.ToString();
		}

		private static void ReverseCharacterOrder(StringBuilder sb)
		{
			int lastIndex = sb.Length - 1;
			for (int i = sb.Length / 2; i >= 0; i--)
			{
				char first = sb[i];
				char second = sb[lastIndex - i];
				sb[i] = second;
				sb[lastIndex - i] = first;
			}
		}
	}
}
