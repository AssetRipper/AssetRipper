namespace AssetRipper.IO.Files.Streams.Smart
{
	public partial class SmartStream
	{
		private class SmartRefCount
		{
			public static SmartRefCount operator ++(SmartRefCount _this)
			{
				_this.RefCount++;
				return _this;
			}

			public static SmartRefCount operator --(SmartRefCount _this)
			{
				_this.RefCount--;
				return _this;
			}

			public void Increase()
			{
				RefCount++;
			}

			public void Decrease()
			{
				RefCount--;
			}

			public override string ToString()
			{
				return RefCount.ToString();
			}

			public bool IsZero => RefCount == 0;

			public int RefCount
			{
				get;
				private set
				{
					ArgumentOutOfRangeException.ThrowIfNegative(value);
					field = value;
				}
			} = 0;
		}
	}
}
