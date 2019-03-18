using FMOD;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace uTinyRipperGUI.Exporters
{
	public static class AudioConverter
	{
		public static bool ConvertToWav(byte[] data, Stream exportStream)
		{
			RESULT result = Factory.System_Create(out FMOD.System system);
			if (result != RESULT.OK)
			{
				return false;
			}

			try
			{
				result = system.init(1, INITFLAGS.NORMAL, IntPtr.Zero);
				if (result != RESULT.OK)
				{
					return false;
				}

				CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
				exinfo.cbsize = Marshal.SizeOf(exinfo);
				exinfo.length = (uint)data.Length;
				result = system.createSound(data, MODE.OPENMEMORY, ref exinfo, out Sound sound);
				if (result != RESULT.OK)
				{
					return false;
				}

				try
				{
					result = sound.getSubSound(0, out Sound subsound);
					if (result != RESULT.OK)
					{
						return false;
					}

					try
					{
						result = subsound.getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out int numChannels, out int bitsPerSample);
						if (result != RESULT.OK)
						{
							return false;
						}

						result = subsound.getDefaults(out float frequency, out int priority);
						if (result != RESULT.OK)
						{
							return false;
						}

						int sampleRate = (int)frequency;
						result = subsound.getLength(out uint length, TIMEUNIT.PCMBYTES);
						if (result != RESULT.OK)
						{
							return false;
						}

						result = subsound.@lock(0, length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2);
						if (result != RESULT.OK)
						{
							return false;
						}

						using (BinaryWriter writer = new BinaryWriter(exportStream))
						{
							writer.Write(Encoding.UTF8.GetBytes("RIFF"));
							writer.Write(len1 + 36);
							writer.Write(Encoding.UTF8.GetBytes("WAVEfmt "));
							writer.Write(16);
							writer.Write((short)1);
							writer.Write((short)numChannels);
							writer.Write(sampleRate);
							writer.Write(sampleRate * numChannels * bitsPerSample / 8);
							writer.Write((short)(numChannels * bitsPerSample / 8));
							writer.Write((short)bitsPerSample);
							writer.Write(Encoding.UTF8.GetBytes("data"));
							writer.Write(len1);

							for (int i = 0; i < len1; i++)
							{
								byte value = Marshal.ReadByte(ptr1, i);
								writer.Write(value);
							}
						}

						result = subsound.unlock(ptr1, ptr2, len1, len2);
						return result == RESULT.OK;
					}
					finally
					{
						subsound.release();
					}
				}
				finally
				{
					sound.release();
				}
			}
			finally
			{
				system.release();
			}
		}
	}
}
