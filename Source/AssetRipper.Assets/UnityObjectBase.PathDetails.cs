namespace AssetRipper.Assets;

public abstract partial class UnityObjectBase
{
	private sealed class PathDetails
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
				directory = value.NotEmpty();
				fullPath = null;
			}
		}

		public string? Name
		{
			get => name;
			set
			{
				name = value.NotEmpty();
				fullPath = null;
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
				extension = value.RemovePeriod().NotEmpty();
				fullPath = null;
			}
		}

		public string? FullPath
		{
			get => fullPath ??= CalculatePath();
			set
			{
				if (value != fullPath)
				{
					fullPath = value.NotEmpty();
					directory = Path.GetDirectoryName(value).NotEmpty();
					name = Path.GetFileNameWithoutExtension(value).NotEmpty();
					extension = Path.GetExtension(value).RemovePeriod().NotEmpty();
				}
			}
		}

		private string? NameWithExtension => Extension is null ? Name : $"{Name}.{Extension}";

		public override string? ToString() => FullPath;

		private string? CalculatePath()
		{
			return Directory is null
				? NameWithExtension
				: Path.Join(Directory, NameWithExtension);
		}
	}
}
