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
				return m_refCount.ToString();
			}

			public bool IsZero => RefCount == 0;

			private int RefCount
			{
				get => m_refCount;
				set
				{
					if (value < 0)
					{
						throw new ArgumentOutOfRangeException(nameof(value));
					}
					m_refCount = value;
				}
			}

			private int m_refCount = 0;
		}
	}
}
