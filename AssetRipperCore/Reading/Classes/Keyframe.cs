using AssetRipper.Core.IO;
using System;

namespace AssetRipper.Core.Reading.Classes
{
	public class Keyframe<T>
	{
		public float time;
		public T value;
		public T inSlope;
		public T outSlope;
		public int weightedMode;
		public T inWeight;
		public T outWeight;


		public Keyframe(ObjectReader reader, Func<T> readerFunc)
		{
			time = reader.ReadSingle();
			value = readerFunc();
			inSlope = readerFunc();
			outSlope = readerFunc();
			if (reader.version[0] >= 2018) //2018 and up
			{
				weightedMode = reader.ReadInt32();
				inWeight = readerFunc();
				outWeight = readerFunc();
			}
		}
	}
}
