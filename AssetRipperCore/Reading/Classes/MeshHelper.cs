using System;

namespace AssetRipper.Reading.Classes
{
	public static class MeshHelper
    {
        public enum VertexChannelFormat
        {
            kChannelFormatFloat,
            kChannelFormatFloat16,
            kChannelFormatColor,
            kChannelFormatByte,
            kChannelFormatUInt32
        }

        public enum VertexFormat2017
        {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatColor,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        public enum VertexFormat
        {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        public static VertexFormat ToVertexFormat(int format, int[] version)
        {
            if (version[0] < 2017)
            {
                switch ((VertexChannelFormat)format)
                {
                    case VertexChannelFormat.kChannelFormatFloat:
                        return VertexFormat.kVertexFormatFloat;
                    case VertexChannelFormat.kChannelFormatFloat16:
                        return VertexFormat.kVertexFormatFloat16;
                    case VertexChannelFormat.kChannelFormatColor: //in 4.x is size 4
                        return VertexFormat.kVertexFormatUNorm8;
                    case VertexChannelFormat.kChannelFormatByte:
                        return VertexFormat.kVertexFormatUInt8;
                    case VertexChannelFormat.kChannelFormatUInt32: //in 5.x
                        return VertexFormat.kVertexFormatUInt32;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else if (version[0] < 2019)
            {
                switch ((VertexFormat2017)format)
                {
                    case VertexFormat2017.kVertexFormatFloat:
                        return VertexFormat.kVertexFormatFloat;
                    case VertexFormat2017.kVertexFormatFloat16:
                        return VertexFormat.kVertexFormatFloat16;
                    case VertexFormat2017.kVertexFormatColor:
                    case VertexFormat2017.kVertexFormatUNorm8:
                        return VertexFormat.kVertexFormatUNorm8;
                    case VertexFormat2017.kVertexFormatSNorm8:
                        return VertexFormat.kVertexFormatSNorm8;
                    case VertexFormat2017.kVertexFormatUNorm16:
                        return VertexFormat.kVertexFormatUNorm16;
                    case VertexFormat2017.kVertexFormatSNorm16:
                        return VertexFormat.kVertexFormatSNorm16;
                    case VertexFormat2017.kVertexFormatUInt8:
                        return VertexFormat.kVertexFormatUInt8;
                    case VertexFormat2017.kVertexFormatSInt8:
                        return VertexFormat.kVertexFormatSInt8;
                    case VertexFormat2017.kVertexFormatUInt16:
                        return VertexFormat.kVertexFormatUInt16;
                    case VertexFormat2017.kVertexFormatSInt16:
                        return VertexFormat.kVertexFormatSInt16;
                    case VertexFormat2017.kVertexFormatUInt32:
                        return VertexFormat.kVertexFormatUInt32;
                    case VertexFormat2017.kVertexFormatSInt32:
                        return VertexFormat.kVertexFormatSInt32;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else
            {
                return (VertexFormat)format;
            }
        }


        public static uint GetFormatSize(VertexFormat format)
        {
            switch (format)
            {
                case VertexFormat.kVertexFormatFloat:
                case VertexFormat.kVertexFormatUInt32:
                case VertexFormat.kVertexFormatSInt32:
                    return 4u;
                case VertexFormat.kVertexFormatFloat16:
                case VertexFormat.kVertexFormatUNorm16:
                case VertexFormat.kVertexFormatSNorm16:
                case VertexFormat.kVertexFormatUInt16:
                case VertexFormat.kVertexFormatSInt16:
                    return 2u;
                case VertexFormat.kVertexFormatUNorm8:
                case VertexFormat.kVertexFormatSNorm8:
                case VertexFormat.kVertexFormatUInt8:
                case VertexFormat.kVertexFormatSInt8:
                    return 1u;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static bool IsIntFormat(VertexFormat format)
        {
            return format >= VertexFormat.kVertexFormatUInt8;
        }

        public static float[] BytesToFloatArray(byte[] inputBytes, VertexFormat format)
        {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new float[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.kVertexFormatFloat:
                        result[i] = BitConverter.ToSingle(inputBytes, i * 4);
                        break;
                    case VertexFormat.kVertexFormatFloat16:
                        result[i] = (float)AssetRipper.Utils.HalfUtils.ToHalf(inputBytes, i * 2);
                        break;
                    case VertexFormat.kVertexFormatUNorm8:
                        result[i] = inputBytes[i] / 255f;
                        break;
                    case VertexFormat.kVertexFormatSNorm8:
                        result[i] = System.Math.Max((sbyte)inputBytes[i] / 127f, -1f);
                        break;
                    case VertexFormat.kVertexFormatUNorm16:
                        result[i] = BitConverter.ToUInt16(inputBytes, i * 2) / 65535f;
                        break;
                    case VertexFormat.kVertexFormatSNorm16:
                        result[i] = System.Math.Max(BitConverter.ToInt16(inputBytes, i * 2) / 32767f, -1f);
                        break;
                }
            }
            return result;
        }

        public static int[] BytesToIntArray(byte[] inputBytes, VertexFormat format)
        {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new int[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.kVertexFormatUInt8:
                    case VertexFormat.kVertexFormatSInt8:
                        result[i] = inputBytes[i];
                        break;
                    case VertexFormat.kVertexFormatUInt16:
                    case VertexFormat.kVertexFormatSInt16:
                        result[i] = BitConverter.ToInt16(inputBytes, i * 2);
                        break;
                    case VertexFormat.kVertexFormatUInt32:
                    case VertexFormat.kVertexFormatSInt32:
                        result[i] = BitConverter.ToInt32(inputBytes, i * 4);
                        break;
                }
            }
            return result;
        }
    }
}
