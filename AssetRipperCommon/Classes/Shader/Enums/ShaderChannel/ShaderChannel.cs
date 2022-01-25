﻿using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using System;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.Shader.Enums.ShaderChannel
{
	/// <summary>
	/// Changed several times. Also called VertexAttribute<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ShaderChannel
	{
		/// <summary>
		/// Also called Position
		/// </summary>
		Vertex,
		Normal,
		Tangent,
		/// <summary>
		/// Vertex Color
		/// </summary>
		Color,
		UV0,
		UV1,
		UV2,
		UV3,
		UV4,
		UV5,
		UV6,
		UV7,
		/// <summary>
		/// Also called BlendWeight
		/// </summary>
		SkinWeight,
		/// <summary>
		/// Also called BlendIndices
		/// </summary>
		SkinBoneIndex,
	}

	public static class ShaderChannelExtensions
	{
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool ShaderChannel2018Relevant(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool ShaderChannel5Relevant(UnityVersion version) => version.IsGreaterEqual(5);

		public static int GetChannelCount(UnityVersion version)
		{
			if (ShaderChannel2018Relevant(version))
			{
				return 14;
			}
			else if (ShaderChannel5Relevant(version))
			{
				return 8;
			}
			else
			{
				return 6;
			}
		}

		public static VertexFormat.VertexFormat GetVertexFormat(this ShaderChannel _this, UnityVersion version)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return VertexFormat.VertexFormat.Float;
				case ShaderChannel.Normal:
					return VertexFormat.VertexFormat.Float;
				case ShaderChannel.Tangent:
					return VertexFormat.VertexFormat.Float;
				case ShaderChannel.Color:
					return VertexFormatExtensions.VertexFormat2019Relevant(version) ? VertexFormat.VertexFormat.Byte : VertexFormat.VertexFormat.Color;

				case ShaderChannel.UV0:
				case ShaderChannel.UV1:
				case ShaderChannel.UV2:
				case ShaderChannel.UV3:
				case ShaderChannel.UV4:
				case ShaderChannel.UV5:
				case ShaderChannel.UV6:
				case ShaderChannel.UV7:
					return VertexFormat.VertexFormat.Float;

				case ShaderChannel.SkinWeight:
					return VertexFormat.VertexFormat.Float;
				case ShaderChannel.SkinBoneIndex:
					return VertexFormat.VertexFormat.Int;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ShaderChannel _this, UnityVersion version)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return 3;
				case ShaderChannel.Normal:
					return 3;
				case ShaderChannel.Tangent:
					return 4;
				case ShaderChannel.Color:
					return ShaderChannel5Relevant(version) ? (byte)4 : (byte)1;

				case ShaderChannel.UV0:
				case ShaderChannel.UV1:
				case ShaderChannel.UV2:
				case ShaderChannel.UV3:
				case ShaderChannel.UV4:
				case ShaderChannel.UV5:
				case ShaderChannel.UV6:
				case ShaderChannel.UV7:
					return 2;

				case ShaderChannel.SkinWeight:
				case ShaderChannel.SkinBoneIndex:
					throw new Exception($"Skin's dimention is varying");

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ShaderChannel _this, UnityVersion version)
		{
			VertexFormat.VertexFormat format = _this.GetVertexFormat(version);
			int dimention = _this.GetDimention(version);
			return format.CalculateStride(version, dimention);
		}

		public static bool HasChannel(this ShaderChannel _this, UnityVersion version)
		{
			if (ShaderChannel2018Relevant(version))
			{
				return true;
			}
			else if (ShaderChannel5Relevant(version))
			{
				return _this <= ShaderChannel.UV3;
			}
			else
			{
				return _this <= ShaderChannel.UV1;
			}
		}

		public static int ToChannel(this ShaderChannel _this, UnityVersion version)
		{
			if (ShaderChannel2018Relevant(version))
			{
				return (int)ToShaderChannel2018(_this);
			}
			else if (ShaderChannel5Relevant(version))
			{
				return (int)ToShaderChannel5(_this);
			}
			else
			{
				return (int)ToShaderChannel4(_this);
			}
		}

		public static ShaderChannel4 ToShaderChannel4(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannel4.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannel4.Normal;
				case ShaderChannel.Color:
					return ShaderChannel4.Color;
				case ShaderChannel.UV0:
					return ShaderChannel4.UV0;
				case ShaderChannel.UV1:
					return ShaderChannel4.UV1;
				case ShaderChannel.Tangent:
					return ShaderChannel4.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ShaderChannel5 ToShaderChannel5(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannel5.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannel5.Normal;
				case ShaderChannel.Color:
					return ShaderChannel5.Color;
				case ShaderChannel.UV0:
					return ShaderChannel5.UV0;
				case ShaderChannel.UV1:
					return ShaderChannel5.UV1;
				case ShaderChannel.UV2:
					return ShaderChannel5.UV2;
				case ShaderChannel.UV3:
					return ShaderChannel5.UV3;
				case ShaderChannel.Tangent:
					return ShaderChannel5.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ShaderChannel2018 ToShaderChannel2018(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannel2018.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannel2018.Normal;
				case ShaderChannel.Tangent:
					return ShaderChannel2018.Tangent;
				case ShaderChannel.Color:
					return ShaderChannel2018.Color;
				case ShaderChannel.UV0:
					return ShaderChannel2018.UV0;
				case ShaderChannel.UV1:
					return ShaderChannel2018.UV1;
				case ShaderChannel.UV2:
					return ShaderChannel2018.UV2;
				case ShaderChannel.UV3:
					return ShaderChannel2018.UV3;
				case ShaderChannel.UV4:
					return ShaderChannel2018.UV4;
				case ShaderChannel.UV5:
					return ShaderChannel2018.UV5;
				case ShaderChannel.UV6:
					return ShaderChannel2018.UV6;
				case ShaderChannel.UV7:
					return ShaderChannel2018.UV7;
				case ShaderChannel.SkinWeight:
					return ShaderChannel2018.SkinWeight;
				case ShaderChannel.SkinBoneIndex:
					return ShaderChannel2018.SkinBoneIndex;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
