using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Mono;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	public sealed class SerializableStructure : UnityAssetBase
	{
		private readonly SerializableValue[] m_Fields;
		private readonly List<SerializableValue> m_ExtraFields; // 用于存储额外的Naninovel字段
		private readonly List<string> m_ExtraFieldNames; // 存储额外字段的名称

		public override int SerializedVersion => Type.Version;
		public override bool FlowMappedInYaml => Type.FlowMappedInYaml;

		internal SerializableStructure(SerializableType type, int depth)
		{
			Depth = depth;
			Type = type ?? throw new ArgumentNullException(nameof(type));
			m_Fields = new SerializableValue[type.FieldCount];
			m_ExtraFields = new List<SerializableValue>();
			m_ExtraFieldNames = new List<string>();
		}

		public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
		{
			for (int i = 0; i < m_Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					m_Fields[i].Read(ref reader, version, flags, Depth, etalon);
				}
			}
		}

		public void Write(AssetWriter writer)
		{
			// 写入标准字段
			for (int i = 0; i < m_Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					m_Fields[i].Write(writer, etalon);
				}
			}

			// 写入额外的Naninovel字段
			for (int i = 0; i < m_ExtraFields.Count; i++)
			{
				var extraField = new SerializableType.Field(
					SerializablePrimitiveType.GetOrCreate(PrimitiveType.String),
					0,
					m_ExtraFieldNames[i],
					true);
				m_ExtraFields[i].Write(writer, extraField);
			}
		}
		public override void WriteEditor(AssetWriter writer) => Write(writer);
		public override void WriteRelease(AssetWriter writer) => Write(writer);

		public override void WalkEditor(AssetWalker walker)
		{
			if (walker.EnterAsset(this))
			{
				bool hasEmittedFirstField = false;
				
				// 遍历标准字段
				for (int i = 0; i < m_Fields.Length; i++)
				{
					SerializableType.Field etalon = Type.GetField(i);
					if (IsAvailable(etalon))
					{
						if (hasEmittedFirstField)
						{
							walker.DivideAsset(this);
						}
						else
						{
							hasEmittedFirstField = true;
						}
						if (walker.EnterField(this, etalon.Name))
						{
							m_Fields[i].WalkEditor(walker, etalon);
							walker.ExitField(this, etalon.Name);
						}
					}
				}

				// 遍历额外字段
				for (int i = 0; i < m_ExtraFields.Count; i++)
				{
					if (hasEmittedFirstField)
					{
						walker.DivideAsset(this);
					}
					else
					{
						hasEmittedFirstField = true;
					}
					string fieldName = m_ExtraFieldNames[i];
					if (walker.EnterField(this, fieldName))
					{
						var extraField = new SerializableType.Field(
							SerializablePrimitiveType.GetOrCreate(PrimitiveType.String),
							0,
							fieldName,
							true);
						m_ExtraFields[i].WalkEditor(walker, extraField);
						walker.ExitField(this, fieldName);
					}
				}

				walker.ExitAsset(this);
			}
		}
		//For now, only the editor version is implemented.
		public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
		public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

		public override IEnumerable<(string, PPtr)> FetchDependencies()
		{
			for (int i = 0; i < m_Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.GetField(i);
				if (IsAvailable(etalon))
				{
					foreach ((string, PPtr) pair in m_Fields[i].FetchDependencies(etalon))
					{
						yield return pair;
					}
				}
			}
		}

		public override string ToString() => Type.FullName;

		private bool IsAvailable(in SerializableType.Field field)
		{
			// 对Naninovel类型特殊处理
			if (field.Type.Namespace?.Contains("Naninovel") == true)
			{
				return true;
			}

			if (Depth < GetMaxDepthLevel())
			{
				return true;
			}
			if (field.ArrayDepth > 0)
			{
				return false;
			}
			if (field.Type.Type == PrimitiveType.Complex)
			{
				return MonoUtils.IsEngineStruct(field.Type.Namespace, field.Type.Name);
			}
			return true;
		}

		public bool TryRead(ref EndianSpanReader reader, IMonoBehaviour monoBehaviour)
		{
			try
			{
				string? scriptNamespace = monoBehaviour.ScriptP?.Namespace?.String;
				bool isNaninovel = !string.IsNullOrEmpty(scriptNamespace) && scriptNamespace.Contains("Naninovel");

				// 读取基类字段
				if (Type.Base != null)
				{
					for (int i = 0; i < Type.BaseFieldCount; i++)
					{
						SerializableType.Field field = Type.Base.GetField(i);
						if (!IsAvailable(field))
							continue;

						try
						{
							// 对于基类字段，总是使用标准读取
							SerializableValue value = new();
							value.Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags, Depth, field);
							m_Fields[i] = value;
						}
						catch (Exception ex)
						{
							Logger.Warning(LogCategory.Import, $"Failed to read base field {field.Name}: {ex.Message}");
						}
					}
				}

				// 读取当前类型的字段
				for (int i = Type.BaseFieldCount; i < m_Fields.Length; i++)
				{
					SerializableType.Field field = Type.GetField(i);
					if (!IsAvailable(field))
						continue;

					try
					{
						int fieldStartPosition = reader.Position;
						bool fieldReadSuccess = false;

						// 首先尝试标准读取
						try
						{
							SerializableValue value = new();
							value.Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags, Depth, field);
							m_Fields[i] = value;
							fieldReadSuccess = true;
						}
						catch when (isNaninovel)
						{
							// 如果是Naninovel脚本且标准读取失败，尝试Naninovel特殊处理
							reader.Position = fieldStartPosition;
							
							if (field.Type.Type == PrimitiveType.String || 
								(field.Type.Type == PrimitiveType.Complex && field.Type.Namespace?.Contains("Naninovel") == true))
							{
								// 对于字符串和Naninovel类型，使用字符串读取
								SerializableValue value = new();
								value.Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags, Depth,
									new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, field.Name, true));
								m_Fields[i] = value;
								fieldReadSuccess = true;
							}
							else if (field.Type.Type == PrimitiveType.Complex)
							{
								try
								{
									// 尝试作为嵌套结构读取
									var nestedStructure = new SerializableStructure(field.Type, Depth + 1);
									if (nestedStructure.TryRead(ref reader, monoBehaviour))
									{
										m_Fields[i] = new SerializableValue { CValue = nestedStructure };
										fieldReadSuccess = true;
									}
								}
								catch
								{
									// 如果嵌套结构读取失败，尝试作为字符串读取
									reader.Position = fieldStartPosition;
									SerializableValue value = new();
									value.Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags, Depth,
										new SerializableType.Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, field.Name, true));
									m_Fields[i] = value;
									fieldReadSuccess = true;
								}
							}
						}

						if (!fieldReadSuccess)
						{
							Logger.Warning(LogCategory.Import, $"Failed to read field {field.Name}");
						}
					}
					catch (Exception ex)
					{
						Logger.Warning(LogCategory.Import, $"Failed to read field {field.Name}: {ex.Message}");
					}
				}

				// 处理剩余数据
				if (reader.Position < reader.Length && isNaninovel)
				{
					try
					{
						int remainingBytes = reader.Length - reader.Position;
						if (remainingBytes > 0)
						{
							List<SerializableValue> extraFields = new();
							List<string> extraFieldNames = new();
							
							while (reader.Position < reader.Length)
							{
								int extraFieldStartPosition = reader.Position;
								try
								{
									// 读取字符串长度
									int stringLength = reader.ReadInt32();
									if (stringLength > 0 && stringLength < remainingBytes)
									{
										// 读取实际的字符串内容
										byte[] stringBytes = reader.ReadBytes(stringLength);
										string? content = System.Text.Encoding.UTF8.GetString(stringBytes).TrimEnd('\0');
										
										if (!string.IsNullOrEmpty(content))
										{
											SerializableValue extraValue = new();
											string fieldName = $"NaninovelScript_{extraFields.Count}";
											var field = new SerializableType.Field(
												SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 
												0, 
												fieldName, 
												true);

											// 设置字符串值
											extraValue.AsString = content;
											extraFields.Add(extraValue);
											extraFieldNames.Add(fieldName);
										}
									}
									else
									{
										// 如果长度无效，跳过一个字节
										reader.Position = extraFieldStartPosition + 1;
									}
								}
								catch
								{
									// 如果读取失败，跳过一个字节继续尝试
									reader.Position = extraFieldStartPosition + 1;
								}

								// 检查是否已经读取到末尾
								if (reader.Position >= reader.Length)
								{
									break;
								}

								// 对齐到4字节边界
								int alignment = reader.Position % 4;
								if (alignment != 0)
								{
									reader.Position += 4 - alignment;
								}
							}

							if (extraFields.Count > 0)
							{
								// 清除现有的额外字段
								m_ExtraFields.Clear();
								m_ExtraFieldNames.Clear();

								// 添加新的字段
								for (int i = 0; i < extraFields.Count; i++)
								{
									if (!string.IsNullOrEmpty(extraFields[i].AsString))
									{
										m_ExtraFields.Add(extraFields[i]);
										m_ExtraFieldNames.Add(extraFieldNames[i]);
									}
								}
								Logger.Info(LogCategory.Import, $"Added {m_ExtraFields.Count} Naninovel script fields");
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Warning(LogCategory.Import, $"Failed to read remaining Naninovel data: {ex.Message}");
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				LogMonoBehaviorReadException(this, ex);
				return false;
			}
		}

		private static void LogMonoBehaviourMismatch(SerializableStructure structure, int actual, int expected)
		{
			Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content (read {actual} bytes, expected {expected} bytes).");
		}

		private static void LogMonoBehaviorReadException(SerializableStructure structure, Exception ex)
		{
			Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content ({ex.GetType().Name}).");
		}

		public int Depth { get; }
		public SerializableType Type { get; }
		public SerializableValue[] Fields => m_Fields;

		public ref SerializableValue this[string name]
		{
			get
			{
				if (TryGetIndex(name, out int index))
				{
					return ref m_Fields[index];
				}
				throw new KeyNotFoundException($"Field {name} wasn't found in {Type.Name}");
			}
		}

		public bool ContainsField(string name) => TryGetIndex(name, out _);

		public bool TryGetField(string name, out SerializableValue field)
		{
			if (TryGetIndex(name, out int index))
			{
				field = m_Fields[index];
				return true;
			}
			field = default;
			return false;
		}

		public SerializableValue? TryGetField(string name)
		{
			if (TryGetIndex(name, out int index))
			{
				return m_Fields[index];
			}
			return null;
		}

		private bool TryGetIndex(string name, out int index)
		{
			for (int i = 0; i < m_Fields.Length; i++)
			{
				if (Type.Fields[i].Name == name)
				{
					index = i;
					return true;
				}
			}
			index = -1;
			return false;
		}

		public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
		{
			if (source is null)
			{
				Reset();
			}
			else
			{
				CopyValues((SerializableStructure)source, converter);
			}
		}

		public void CopyValues(SerializableStructure source, PPtrConverter converter)
		{
			if (source.Depth != Depth)
			{
				throw new ArgumentException($"Depth {source.Depth} doesn't match with {Depth}", nameof(source));
			}

			// 复制标准字段
			if (source.Type == Type)
			{
				for (int i = 0; i < m_Fields.Length; i++)
				{
					SerializableValue sourceField = source.m_Fields[i];
					if (sourceField.CValue is null)
					{
						m_Fields[i] = sourceField;
					}
					else
					{
						m_Fields[i].CopyValues(sourceField, Depth, Type.Fields[i], converter);
					}
				}
			}
			else
			{
				for (int i = 0; i < m_Fields.Length; i++)
				{
					string fieldName = Type.Fields[i].Name;
					int index = -1;
					for (int j = 0; j < source.Type.Fields.Count; j++)
					{
						if (fieldName == source.Type.Fields[j].Name)
						{
							index = j;
							break;
						}
					}
					SerializableValue sourceField = index < 0 ? default : source.m_Fields[index];
					m_Fields[i].CopyValues(sourceField, Depth, Type.Fields[i], converter);
				}
			}

			// 复制额外字段
			m_ExtraFields.Clear();
			m_ExtraFieldNames.Clear();
			for (int i = 0; i < source.m_ExtraFields.Count; i++)
			{
				m_ExtraFields.Add(source.m_ExtraFields[i]);
				m_ExtraFieldNames.Add(source.m_ExtraFieldNames[i]);
			}
		}

		public SerializableStructure DeepClone(PPtrConverter converter)
		{
			SerializableStructure clone = new(Type, Depth);
			clone.CopyValues(this, converter);
			return clone;
		}

		public override void Reset()
		{
			foreach (SerializableValue field in m_Fields)
			{
				field.Reset();
			}
		}

		public void InitializeFields(UnityVersion version)
		{
			for (int i = 0; i < m_Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.Fields[i];
				if (IsAvailable(etalon))
				{
					m_Fields[i].Initialize(version, Depth, etalon);
				}
			}
		}

		/// <summary>
		/// 8 might have been an arbitrarily chosen number, but I think it's because the limit was supposedly 7 when mafaca made uTinyRipper.
		/// An official source is required, but forum posts suggest 7 and later 10 as the limits.
		/// It may be desirable to increase this number or do a Unity version check.
		/// </summary>
		/// <remarks>
		/// <see href="https://forum.unity.com/threads/serialization-depth-limit-and-recursive-serialization.1263599/"/><br/>
		/// <see href="https://forum.unity.com/threads/getting-a-serialization-depth-limit-7-error-for-no-reason.529850/"/><br/>
		/// <see href="https://forum.unity.com/threads/4-5-serialization-depth.248321/"/>
		/// </remarks>
		private static int GetMaxDepthLevel() => 8;

		private void HandleExtraFields(List<SerializableValue> extraFields, List<string> extraFieldNames)
		{
			if (extraFields.Count > 0)
			{
				// 直接添加到额外字段列表
				for (int i = 0; i < extraFields.Count; i++)
				{
					m_ExtraFields.Add(extraFields[i]);
					m_ExtraFieldNames.Add(extraFieldNames[i]);
				}
				Logger.Info(LogCategory.Import, $"Added {extraFields.Count} extra Naninovel fields");
			}
		}

		public IReadOnlyList<(string Name, SerializableValue Value)> GetExtraFields()
		{
			var result = new List<(string, SerializableValue)>();
			for (int i = 0; i < m_ExtraFields.Count; i++)
			{
				result.Add((m_ExtraFieldNames[i], m_ExtraFields[i]));
			}
			return result;
		}
	}
}
