namespace AssetRipper.Assets;

public abstract partial class UnityObjectBase
{
	private sealed class OriginalPathDetails
	{
		private string? directory;
		private string? name;
		private string? extension;
		private string? fullPath;

		public string? Directory
		{
			get => directory;
			set
			{
				directory = value;
				fullPath = CalculatePath();
			}
		}

		public string? Name
		{
			get => name;
			set
			{
				name = value;
				fullPath = CalculatePath();
			}
		}

		/// <summary>
		/// Not including the period
		/// </summary>
		public string? Extension
		{
			get => extension;
			set
			{
				extension = RemovePeriod(value);
				fullPath = CalculatePath();
			}
		}

		public string? FullPath
		{
			get => fullPath;
			set
			{
				if (value != fullPath)
				{
					fullPath = value;
					Directory = Path.GetDirectoryName(value);
					Name = Path.GetFileNameWithoutExtension(value);
					Extension = RemovePeriod(Path.GetExtension(value));
				}
			}
		}

		private string NameWithExtension => string.IsNullOrEmpty(Extension) ? Name ?? "" : $"{Name}.{Extension}";

		public override string? ToString() => FullPath;

		private string CalculatePath()
		{
			return Directory is null
				? NameWithExtension
				: Path.Combine(Directory, NameWithExtension);
		}

		[return: NotNullIfNotNull(nameof(str))]
		private static string? RemovePeriod(string? str)
		{
			return string.IsNullOrEmpty(str) || str[0] != '.' ? str : str.Substring(1);
		}
	}
}
