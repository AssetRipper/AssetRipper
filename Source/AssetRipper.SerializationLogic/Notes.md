# Serialization Notes

https://docs.unity3d.com/Manual/script-Serialization.html

## `[SerializeReference]`

The biggest indicator of `[SerializeReference]` in type trees is that the last node is `ReferencedObjectData data`.

> Compiled with 6000.0.0b15

```
// classID{114}: MonoBehaviour
MonoBehaviour Base // ByteSize{ffffffff}, Index{0}, Version{1}, IsArray{0}, MetaFlag{8000}
	PPtr<GameObject> m_GameObject // ByteSize{c}, Index{1}, Version{1}, IsArray{0}, MetaFlag{41}
		int m_FileID // ByteSize{4}, Index{2}, Version{1}, IsArray{0}, MetaFlag{41}
		SInt64 m_PathID // ByteSize{8}, Index{3}, Version{1}, IsArray{0}, MetaFlag{41}
	UInt8 m_Enabled // ByteSize{1}, Index{4}, Version{1}, IsArray{0}, MetaFlag{4101}
	PPtr<MonoScript> m_Script // ByteSize{c}, Index{5}, Version{1}, IsArray{0}, MetaFlag{0}
		int m_FileID // ByteSize{4}, Index{6}, Version{1}, IsArray{0}, MetaFlag{800001}
		SInt64 m_PathID // ByteSize{8}, Index{7}, Version{1}, IsArray{0}, MetaFlag{800001}
	string m_Name // ByteSize{ffffffff}, Index{8}, Version{1}, IsArray{0}, MetaFlag{88001}
		Array Array // ByteSize{ffffffff}, Index{9}, Version{1}, IsArray{1}, MetaFlag{84001}
			int size // ByteSize{4}, Index{a}, Version{1}, IsArray{0}, MetaFlag{80001}
			char data // ByteSize{1}, Index{b}, Version{1}, IsArray{0}, MetaFlag{80001}
	SerializableClass fieldWithoutAttribute // ByteSize{ffffffff}, Index{c}, Version{1}, IsArray{0}, MetaFlag{8000}
		string key // ByteSize{ffffffff}, Index{d}, Version{1}, IsArray{0}, MetaFlag{8000}
			Array Array // ByteSize{ffffffff}, Index{e}, Version{1}, IsArray{1}, MetaFlag{4001}
				int size // ByteSize{4}, Index{f}, Version{1}, IsArray{0}, MetaFlag{1}
				char data // ByteSize{1}, Index{10}, Version{1}, IsArray{0}, MetaFlag{1}
		int value // ByteSize{4}, Index{11}, Version{1}, IsArray{0}, MetaFlag{0}
	managedReference fieldWithAttribute // ByteSize{8}, Index{12}, Version{1}, IsArray{2}, MetaFlag{0}
		SInt64 rid // ByteSize{8}, Index{13}, Version{1}, IsArray{0}, MetaFlag{0}
	SerializableClass listWithoutAttribute // ByteSize{ffffffff}, Index{14}, Version{1}, IsArray{0}, MetaFlag{8000}
		Array Array // ByteSize{ffffffff}, Index{15}, Version{1}, IsArray{1}, MetaFlag{8000}
			int size // ByteSize{4}, Index{16}, Version{1}, IsArray{0}, MetaFlag{0}
			SerializableClass data // ByteSize{ffffffff}, Index{17}, Version{1}, IsArray{0}, MetaFlag{8000}
				string key // ByteSize{ffffffff}, Index{18}, Version{1}, IsArray{0}, MetaFlag{8000}
					Array Array // ByteSize{ffffffff}, Index{19}, Version{1}, IsArray{1}, MetaFlag{4001}
						int size // ByteSize{4}, Index{1a}, Version{1}, IsArray{0}, MetaFlag{1}
						char data // ByteSize{1}, Index{1b}, Version{1}, IsArray{0}, MetaFlag{1}
				int value // ByteSize{4}, Index{1c}, Version{1}, IsArray{0}, MetaFlag{0}
	SerializableClass listWithAttribute // ByteSize{ffffffff}, Index{1d}, Version{1}, IsArray{8}, MetaFlag{0}
		Array Array // ByteSize{ffffffff}, Index{1e}, Version{1}, IsArray{1}, MetaFlag{0}
			int size // ByteSize{4}, Index{1f}, Version{1}, IsArray{0}, MetaFlag{0}
			managedRefArrayItem data // ByteSize{8}, Index{20}, Version{1}, IsArray{2}, MetaFlag{0}
				SInt64 rid // ByteSize{8}, Index{21}, Version{1}, IsArray{0}, MetaFlag{0}
	ManagedReferencesRegistry references // ByteSize{ffffffff}, Index{22}, Version{1}, IsArray{4}, MetaFlag{8001}
		int version // ByteSize{4}, Index{23}, Version{1}, IsArray{0}, MetaFlag{1}
		vector RefIds // ByteSize{ffffffff}, Index{24}, Version{1}, IsArray{0}, MetaFlag{8001}
			Array Array // ByteSize{ffffffff}, Index{25}, Version{1}, IsArray{1}, MetaFlag{c001}
				int size // ByteSize{4}, Index{26}, Version{1}, IsArray{0}, MetaFlag{1}
				ReferencedObject data // ByteSize{ffffffff}, Index{27}, Version{1}, IsArray{0}, MetaFlag{8001}
					SInt64 rid // ByteSize{8}, Index{28}, Version{1}, IsArray{0}, MetaFlag{1}
					ReferencedManagedType type // ByteSize{ffffffff}, Index{29}, Version{1}, IsArray{0}, MetaFlag{208001}
						string class // ByteSize{ffffffff}, Index{2a}, Version{1}, IsArray{0}, MetaFlag{208001}
							Array Array // ByteSize{ffffffff}, Index{2b}, Version{1}, IsArray{1}, MetaFlag{204001}
								int size // ByteSize{4}, Index{2c}, Version{1}, IsArray{0}, MetaFlag{200001}
								char data // ByteSize{1}, Index{2d}, Version{1}, IsArray{0}, MetaFlag{200001}
						string ns // ByteSize{ffffffff}, Index{2e}, Version{1}, IsArray{0}, MetaFlag{208001}
							Array Array // ByteSize{ffffffff}, Index{2f}, Version{1}, IsArray{1}, MetaFlag{204001}
								int size // ByteSize{4}, Index{30}, Version{1}, IsArray{0}, MetaFlag{200001}
								char data // ByteSize{1}, Index{31}, Version{1}, IsArray{0}, MetaFlag{200001}
						string asm // ByteSize{ffffffff}, Index{32}, Version{1}, IsArray{0}, MetaFlag{208001}
							Array Array // ByteSize{ffffffff}, Index{33}, Version{1}, IsArray{1}, MetaFlag{204001}
								int size // ByteSize{4}, Index{34}, Version{1}, IsArray{0}, MetaFlag{200001}
								char data // ByteSize{1}, Index{35}, Version{1}, IsArray{0}, MetaFlag{200001}
					ReferencedObjectData data // ByteSize{0}, Index{36}, Version{1}, IsArray{0}, MetaFlag{1}
```

## Empty Structs

Empty types can still be serializable even though they have no content.

> Compiled with 6000.0.0b15

```
// classID{114}: MonoBehaviour
MonoBehaviour Base // ByteSize{ffffffff}, Index{0}, Version{1}, IsArray{0}, MetaFlag{8000}
	PPtr<GameObject> m_GameObject // ByteSize{c}, Index{1}, Version{1}, IsArray{0}, MetaFlag{41}
		int m_FileID // ByteSize{4}, Index{2}, Version{1}, IsArray{0}, MetaFlag{41}
		SInt64 m_PathID // ByteSize{8}, Index{3}, Version{1}, IsArray{0}, MetaFlag{41}
	UInt8 m_Enabled // ByteSize{1}, Index{4}, Version{1}, IsArray{0}, MetaFlag{4101}
	PPtr<MonoScript> m_Script // ByteSize{c}, Index{5}, Version{1}, IsArray{0}, MetaFlag{0}
		int m_FileID // ByteSize{4}, Index{6}, Version{1}, IsArray{0}, MetaFlag{800001}
		SInt64 m_PathID // ByteSize{8}, Index{7}, Version{1}, IsArray{0}, MetaFlag{800001}
	string m_Name // ByteSize{ffffffff}, Index{8}, Version{1}, IsArray{0}, MetaFlag{88001}
		Array Array // ByteSize{ffffffff}, Index{9}, Version{1}, IsArray{1}, MetaFlag{84001}
			int size // ByteSize{4}, Index{a}, Version{1}, IsArray{0}, MetaFlag{80001}
			char data // ByteSize{1}, Index{b}, Version{1}, IsArray{0}, MetaFlag{80001}
	StructWithFields listOfFields // ByteSize{ffffffff}, Index{c}, Version{1}, IsArray{0}, MetaFlag{8000}
		Array Array // ByteSize{ffffffff}, Index{d}, Version{1}, IsArray{1}, MetaFlag{8000}
			int size // ByteSize{4}, Index{e}, Version{1}, IsArray{0}, MetaFlag{0}
			StructWithFields data // ByteSize{ffffffff}, Index{f}, Version{1}, IsArray{0}, MetaFlag{8000}
				string key // ByteSize{ffffffff}, Index{10}, Version{1}, IsArray{0}, MetaFlag{8000}
					Array Array // ByteSize{ffffffff}, Index{11}, Version{1}, IsArray{1}, MetaFlag{4001}
						int size // ByteSize{4}, Index{12}, Version{1}, IsArray{0}, MetaFlag{1}
						char data // ByteSize{1}, Index{13}, Version{1}, IsArray{0}, MetaFlag{1}
				int value // ByteSize{4}, Index{14}, Version{1}, IsArray{0}, MetaFlag{0}
	EmptyStruct listOfNothing // ByteSize{ffffffff}, Index{15}, Version{1}, IsArray{0}, MetaFlag{0}
		Array Array // ByteSize{ffffffff}, Index{16}, Version{1}, IsArray{1}, MetaFlag{0}
			int size // ByteSize{4}, Index{17}, Version{1}, IsArray{0}, MetaFlag{0}
			EmptyStruct data // ByteSize{0}, Index{18}, Version{1}, IsArray{0}, MetaFlag{0}
	StructWithFields fields // ByteSize{ffffffff}, Index{19}, Version{1}, IsArray{0}, MetaFlag{8000}
		string key // ByteSize{ffffffff}, Index{1a}, Version{1}, IsArray{0}, MetaFlag{8000}
			Array Array // ByteSize{ffffffff}, Index{1b}, Version{1}, IsArray{1}, MetaFlag{4001}
				int size // ByteSize{4}, Index{1c}, Version{1}, IsArray{0}, MetaFlag{1}
				char data // ByteSize{1}, Index{1d}, Version{1}, IsArray{0}, MetaFlag{1}
		int value // ByteSize{4}, Index{1e}, Version{1}, IsArray{0}, MetaFlag{0}
	EmptyStruct nothing // ByteSize{0}, Index{1f}, Version{1}, IsArray{0}, MetaFlag{0}
```