using System;
using System.Collections.Generic;

namespace UtinyRipper.SerializedFiles
{
	internal sealed class RTTIClassHierarchyDescriptor
	{
		public RTTIClassHierarchyDescriptor(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}
			Name = name;
		}

		/// <summary>
		/// 3.0.0b and greater
		/// </summary>
		public static bool IsReadSignature(FileGeneration generation)
		{
			return generation > FileGeneration.FG_300b;
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadAttributes(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_300_342;
		}
		/// <summary>
		/// 5.0.0Unk2 and greater
		/// </summary>
		public static bool IsReadSerializeTypeTrees(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500aunk2;
		}
		/// <summary>
		/// 3.0.0b to 4.x.x
		/// </summary>
		public static bool IsReadUnknown(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_300b && generation < FileGeneration.FG_500;
		}

		public void Read(SerializedFileStream stream)
		{
			if (IsReadSignature(stream.Generation))
			{
				string signature = stream.ReadStringZeroTerm();
				Version.Parse(signature);
			}
			if (IsReadAttributes(stream.Generation))
			{
				Platform = (Platform)stream.ReadUInt32();
				if (!Enum.IsDefined(typeof(Platform), Platform))
				{
					throw new Exception($"Unsuported platform {Platform} for asset file '{Name}'");
				}
			}
			if (IsReadSerializeTypeTrees(stream.Generation))
			{
				SerializeTypeTrees = stream.ReadBoolean();
			}
			else
			{
				SerializeTypeTrees = true;
			}
			m_types = stream.ReadArray(() => new RTTIBaseClassDescriptor(SerializeTypeTrees));

			if (IsReadUnknown(stream.Generation))
			{
				Unknown = stream.ReadInt32();
			}
		}

		/// <summary>
		/// Signature
		/// </summary>
		public Version Version { get; } = new Version();
		/// <summary>
		/// Attributes
		/// </summary>
		public Platform Platform { get; private set; }
		public bool SerializeTypeTrees { get; private set; }
		public IReadOnlyList<RTTIBaseClassDescriptor> Types => m_types;
		public int Unknown { get; private set; }
		
		private string Name { get; }

		private RTTIBaseClassDescriptor[] m_types;
	}
}
