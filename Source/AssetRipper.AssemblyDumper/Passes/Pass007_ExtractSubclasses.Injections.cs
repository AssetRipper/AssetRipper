using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.IO.Files.SerializedFiles;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass007_ExtractSubclasses
{
	private static void DoCustomInjections()
	{
		InjectLayerMask();
		InjectRectInt();
		InjectRectOffset();
		InjectAABBInt();
		InjectGUIStyleState();
		InjectGUIStyle();
		InjectStreamedCurveKey();
		InjectStreamedFrame();
	}

	private static void InjectLayerMask()
	{
		const string ClassName = "LayerMask";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);
		UniversalNode releaseNode = MakeRootNode();
		UniversalNode editorNode = MakeRootNode();
		UniversalClass @class = new UniversalClass(releaseNode, editorNode);
		classList.Add(SharedState.Instance.MinVersion, @class);

		static UniversalNode MakeRootNode()
		{
			UniversalNode rootNode = new()
			{
				Name = "Base",
				TypeName = ClassName,
				Version = 2,//1 before 2.0.0
				MetaFlag = TransferMetaFlags.NoTransferFlags,
			};
			rootNode.SubNodes.Add(new()
			{
				Name = "m_Bits",
				TypeName = "UInt32",//UInt16 before 2.0.0
				Version = 1,
				MetaFlag = TransferMetaFlags.NoTransferFlags,
			});
			return rootNode;
		}
	}

	private static void InjectRectInt()
	{
		const string ClassName = "RectInt";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);
		UniversalNode releaseNode = MakeRootNode();
		UniversalNode editorNode = MakeRootNode();
		UniversalClass @class = new UniversalClass(releaseNode, editorNode);
		classList.Add(SharedState.Instance.MinVersion, @class);

		static UniversalNode MakeRootNode()
		{
			UniversalNode rootNode = CreateRootNode(ClassName, 2);
			rootNode.SubNodes.Add(CreateInt32Node("m_X", "x"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Y", "y"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Width", "width"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Height", "height"));
			return rootNode;
		}
	}

	private static void InjectRectOffset()
	{
		const string ClassName = "RectOffset";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);
		UniversalNode releaseNode = MakeRootNode();
		UniversalNode editorNode = MakeRootNode();
		UniversalClass @class = new UniversalClass(releaseNode, editorNode);
		classList.Add(SharedState.Instance.MinVersion, @class);

		static UniversalNode MakeRootNode()
		{
			UniversalNode rootNode = CreateRootNode(ClassName);
			rootNode.SubNodes.Add(CreateInt32Node("m_Left"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Right"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Top"));
			rootNode.SubNodes.Add(CreateInt32Node("m_Bottom"));
			return rootNode;
		}
	}

	private static void InjectAABBInt()
	{
		VersionedList<UniversalClass> vector3IntList = SharedState.Instance.SubclassInformation["Vector3Int"];
		VersionedList<UniversalClass> classList = new();
		const string ClassName = "AABBInt";
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);
		foreach ((UnityVersion version, UniversalClass? vectorClass) in vector3IntList)
		{
			if (vectorClass is null)
			{
				classList.Add(version, null);
			}
			else
			{
				UniversalNode releaseNode;
				{
					releaseNode = new()
					{
						Name = "Base",
						TypeName = ClassName,
						Version = 1,
						MetaFlag = TransferMetaFlags.NoTransferFlags,
					};
					UniversalNode centerNode = vectorClass.ReleaseRootNode!.DeepClone();
					centerNode.Name = centerNode.OriginalName = "m_Center";
					releaseNode.SubNodes.Add(centerNode);

					UniversalNode extentNode = vectorClass.ReleaseRootNode!.DeepClone();
					extentNode.Name = extentNode.OriginalName = "m_Extent";
					releaseNode.SubNodes.Add(extentNode);
				}
				UniversalNode editorNode;
				{
					editorNode = new()
					{
						Name = "Base",
						TypeName = ClassName,
						Version = 1,
						MetaFlag = TransferMetaFlags.NoTransferFlags,
					};
					UniversalNode centerNode = vectorClass.EditorRootNode!.DeepClone();
					centerNode.Name = centerNode.OriginalName = "m_Center";
					editorNode.SubNodes.Add(centerNode);

					UniversalNode extentNode = vectorClass.EditorRootNode!.DeepClone();
					extentNode.Name = extentNode.OriginalName = "m_Extent";
					editorNode.SubNodes.Add(extentNode);
				}
				UniversalClass @class = new UniversalClass(releaseNode, editorNode);
				classList.Add(version, @class);
			}
		}
	}

	private static void InjectGUIStyleState()
	{
		VersionedList<UniversalClass> pptrTexture2DList = SharedState.Instance.SubclassInformation["PPtr_Texture2D"];
		VersionedList<UniversalClass> colorList = SharedState.Instance.SubclassInformation["ColorRGBAf"];
		Debug.Assert(pptrTexture2DList.Count == 2);
		Debug.Assert(colorList.Count == 1);

		const string ClassName = "GUIStyleState";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);

		(UnityVersion versionC, UniversalClass? colorClass) = colorList[0];
		(UnityVersion versionP1, UniversalClass? pptrClass1) = pptrTexture2DList[0];
		(UnityVersion versionP2, UniversalClass? pptrClass2) = pptrTexture2DList[1];
		Debug.Assert(versionC == SharedState.Instance.MinVersion);
		Debug.Assert(versionC == versionP1);
		Debug.Assert(versionC < versionP2);

		UniversalNode releaseRoot1 = CreateRootNode(ClassName);
		releaseRoot1.SubNodes.Add(pptrClass1!.ReleaseRootNode!.DeepCloneAndChangeName("m_Background"));
		releaseRoot1.SubNodes.Add(colorClass!.ReleaseRootNode!.DeepCloneAndChangeName("m_TextColor"));
		UniversalNode editorRoot1 = CreateRootNode(ClassName);
		editorRoot1.SubNodes.Add(pptrClass1!.EditorRootNode!.DeepCloneAndChangeName("m_Background"));
		editorRoot1.SubNodes.Add(colorClass!.EditorRootNode!.DeepCloneAndChangeName("m_TextColor"));
		classList.Add(versionC, new UniversalClass(releaseRoot1, editorRoot1));

		UniversalNode releaseRoot2 = CreateRootNode(ClassName);
		releaseRoot2.SubNodes.Add(pptrClass2!.ReleaseRootNode!.DeepCloneAndChangeName("m_Background"));
		releaseRoot2.SubNodes.Add(colorClass!.ReleaseRootNode!.DeepCloneAndChangeName("m_TextColor"));
		UniversalNode editorRoot2 = CreateRootNode(ClassName);
		editorRoot2.SubNodes.Add(pptrClass2!.EditorRootNode!.DeepCloneAndChangeName("m_Background"));
		editorRoot2.SubNodes.Add(colorClass!.EditorRootNode!.DeepCloneAndChangeName("m_TextColor"));
		classList.Add(versionP2, new UniversalClass(releaseRoot2, editorRoot2));

		UniversalNode releaseRoot3 = CreateRootNode(ClassName);
		releaseRoot3.SubNodes.Add(pptrClass2!.ReleaseRootNode!.DeepCloneAndChangeName("m_Background"));
		releaseRoot3.SubNodes.Add(colorClass!.ReleaseRootNode!.DeepCloneAndChangeName("m_TextColor"));
		UniversalNode editorRoot3 = CreateRootNode(ClassName);
		editorRoot3.SubNodes.Add(pptrClass2!.EditorRootNode!.DeepCloneAndChangeName("m_Background"));
		editorRoot3.SubNodes.Add(CreateArrayNode("m_ScaledBackgrounds", pptrClass2!.EditorRootNode!.DeepClone(), false));
		editorRoot3.SubNodes.Add(colorClass!.EditorRootNode!.DeepCloneAndChangeName("m_TextColor"));
		classList.Add(new UnityVersion(5, 4, 0), new UniversalClass(releaseRoot3, editorRoot3));

		UniversalNode releaseRoot4 = CreateRootNode(ClassName);
		releaseRoot4.SubNodes.Add(pptrClass2!.ReleaseRootNode!.DeepCloneAndChangeName("m_Background"));
		releaseRoot4.SubNodes.Add(colorClass!.ReleaseRootNode!.DeepCloneAndChangeName("m_TextColor"));
		UniversalNode editorRoot4 = CreateRootNode(ClassName);
		editorRoot4.SubNodes.Add(pptrClass2!.EditorRootNode!.DeepCloneAndChangeName("m_Background"));
		editorRoot4.SubNodes.Add(CreateArrayNode("m_ScaledBackgrounds", pptrClass2!.EditorRootNode!.DeepClone(), true));
		editorRoot4.SubNodes.Add(colorClass!.EditorRootNode!.DeepCloneAndChangeName("m_TextColor"));
		classList.Add(ArrayAlignmentStartVersion, new UniversalClass(releaseRoot4, editorRoot4));
	}

	private static void InjectGUIStyle()
	{
		UnityVersion builtInVersion = new UnityVersion(4, 0, 0);
		VersionedList<UniversalClass> pptrFontList = SharedState.Instance.SubclassInformation["PPtr_Font"];
		VersionedList<UniversalClass> stateList = SharedState.Instance.SubclassInformation["GUIStyleState"];
		UniversalNode stringTemplate = SharedState.Instance.ClassInformation[1][^1].Value!.ReleaseRootNode!.GetSubNodeByName("m_Name");
		VersionedList<UniversalClass> rectOffsetList = SharedState.Instance.SubclassInformation["RectOffset"];
		VersionedList<UniversalClass> vectorList = SharedState.Instance.SubclassInformation["Vector2f"];
		Debug.Assert(pptrFontList.Count == 2);
		Debug.Assert(stateList.Count == 4);

		const string ClassName = "GUIStyle";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);

		UnityVersion versionS1 = stateList[0].Key;
		UnityVersion versionP1 = pptrFontList[0].Key;
		Debug.Assert(versionS1 == versionP1);
		Debug.Assert(versionS1 == SharedState.Instance.MinVersion);
		Debug.Assert(versionS1 < builtInVersion);

		UnityVersion versionP2 = pptrFontList[1].Key;
		UnityVersion versionS2 = stateList[1].Key;
		UnityVersion versionS3 = stateList[2].Key;
		UnityVersion versionS4 = stateList[3].Key;
		Debug.Assert(versionS2 == versionP2);
		Debug.Assert(versionS2 > builtInVersion);

		List<UnityVersion> versions = new()
		{
			versionS1,
			builtInVersion,
			versionS2,
			versionS3,
			versionS4,
		};

		foreach (UnityVersion version in versions)
		{
			UniversalNode releaseRoot = CreateRootNode(ClassName);
			UniversalNode editorRoot = CreateRootNode(ClassName);

			AddSubNodeClone(releaseRoot, editorRoot, stringTemplate, "m_Name");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_Normal");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_Hover");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_Active");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_Focused");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_OnNormal");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_OnHover");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_OnActive");
			AddSubNode(releaseRoot, editorRoot, stateList, version, "m_OnFocused");
			AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Border");
			if (IsBuiltinFormat(version))
			{
				AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Margin");
				AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Padding");
			}
			else
			{
				AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Padding");
				AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Margin");
			}
			AddSubNode(releaseRoot, editorRoot, rectOffsetList, version, "m_Overflow");
			AddSubNode(releaseRoot, editorRoot, pptrFontList, version, "m_Font");
			if (IsBuiltinFormat(version))
			{
				AddInt32SubNode(releaseRoot, editorRoot, "m_FontSize");
				AddInt32SubNode(releaseRoot, editorRoot, "m_FontStyle");
				AddInt32SubNode(releaseRoot, editorRoot, "m_Alignment");
				AddBooleanSubNode(releaseRoot, editorRoot, "m_WordWrap", false);
				AddBooleanSubNode(releaseRoot, editorRoot, "m_RichText", true);

				AddInt32SubNode(releaseRoot, editorRoot, "m_TextClipping");
				AddInt32SubNode(releaseRoot, editorRoot, "m_ImagePosition");
				AddSubNode(releaseRoot, editorRoot, vectorList, version, "m_ContentOffset");
				AddSingleSubNode(releaseRoot, editorRoot, "m_FixedWidth");
				AddSingleSubNode(releaseRoot, editorRoot, "m_FixedHeight");
				AddBooleanSubNode(releaseRoot, editorRoot, "m_StretchWidth", false);
				AddBooleanSubNode(releaseRoot, editorRoot, "m_StretchHeight", true);
			}
			else
			{
				AddInt32SubNode(releaseRoot, editorRoot, "m_ImagePosition");
				AddInt32SubNode(releaseRoot, editorRoot, "m_Alignment");
				AddBooleanSubNode(releaseRoot, editorRoot, "m_WordWrap", true);

				AddInt32SubNode(releaseRoot, editorRoot, "m_TextClipping");
				AddSubNode(releaseRoot, editorRoot, vectorList, version, "m_ContentOffset");
				AddSubNode(releaseRoot, editorRoot, vectorList, version, "m_ClipOffset");
				AddSingleSubNode(releaseRoot, editorRoot, "m_FixedWidth");
				AddSingleSubNode(releaseRoot, editorRoot, "m_FixedHeight");
				if (HasFontSize(version))
				{
					AddInt32SubNode(releaseRoot, editorRoot, "m_FontSize");
					AddInt32SubNode(releaseRoot, editorRoot, "m_FontStyle");
				}
				AddBooleanSubNode(releaseRoot, editorRoot, "m_StretchWidth", true);
				AddBooleanSubNode(releaseRoot, editorRoot, "m_StretchHeight", true);
			}

			classList.Add(version, new UniversalClass(releaseRoot, editorRoot));
		}

		static void AddSubNodeClone(UniversalNode releaseRoot, UniversalNode editorRoot, UniversalNode sourceNode, string name)
		{
			releaseRoot.SubNodes.Add(sourceNode.DeepCloneAndChangeName(name));
			editorRoot.SubNodes.Add(sourceNode.DeepCloneAndChangeName(name));
		}
	}

	private static void InjectStreamedCurveKey()
	{
		VersionedList<UniversalClass> vectorList = SharedState.Instance.SubclassInformation["Vector3f"];

		UnityVersion startVersion = vectorList[0].Key;

		const string ClassName = "StreamedCurveKey";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);

		UniversalNode releaseRoot = CreateRootNode(ClassName);
		UniversalNode editorRoot = CreateRootNode(ClassName);

		AddInt32SubNode(releaseRoot, editorRoot, "m_Index");
		AddSubNode(releaseRoot, editorRoot, vectorList, startVersion, "m_Coefficient");
		AddSingleSubNode(releaseRoot, editorRoot, "m_Value");

		classList.Add(startVersion, new UniversalClass(releaseRoot, editorRoot));
	}

	private static void InjectStreamedFrame()
	{
		VersionedList<UniversalClass> curveKeyList = SharedState.Instance.SubclassInformation["StreamedCurveKey"];

		const string ClassName = "StreamedFrame";
		VersionedList<UniversalClass> classList = new();
		SharedState.Instance.SubclassInformation.Add(ClassName, classList);

		List<UnityVersion> versions = new()
		{
			curveKeyList[0].Key,
			ArrayAlignmentStartVersion,
		};

		foreach (UnityVersion version in versions)
		{
			UniversalNode releaseRoot = CreateRootNode(ClassName);
			UniversalNode editorRoot = CreateRootNode(ClassName);

			AddSingleSubNode(releaseRoot, editorRoot, "m_Time");

			releaseRoot.SubNodes.Add(CreateArrayNode("m_Curves", curveKeyList[0].Value!.ReleaseRootNode!.DeepClone(), version));
			editorRoot.SubNodes.Add(CreateArrayNode("m_Curves", curveKeyList[0].Value!.EditorRootNode!.DeepClone(), version));

			classList.Add(version, new UniversalClass(releaseRoot, editorRoot));
		}
	}

	private static UniversalNode DeepCloneAndChangeName(this UniversalNode node, string name)
	{
		UniversalNode result = node.DeepClone();
		result.OriginalName = result.Name = name;
		return result;
	}

	private static UniversalNode CreateRootNode(string className, short version = 1)
	{
		return new()
		{
			Name = "Base",
			TypeName = className,
			Version = version,
			MetaFlag = TransferMetaFlags.NoTransferFlags,
		};
	}

	private static UniversalNode CreateInt32Node(string name)
	{
		return new()
		{
			Name = name,
			TypeName = "SInt32",
			Version = 1,
			MetaFlag = TransferMetaFlags.NoTransferFlags,
		};
	}

	private static UniversalNode CreateInt32Node(string name, string originalName)
	{
		UniversalNode result = CreateInt32Node(name);
		result.OriginalName = originalName;
		return result;
	}

	private static UniversalNode CreateSingleNode(string name)
	{
		return new()
		{
			Name = name,
			TypeName = "float",
			Version = 1,
			MetaFlag = TransferMetaFlags.NoTransferFlags,
		};
	}

	private static UniversalNode CreateBooleanNode(string name, bool align)
	{
		return new()
		{
			Name = name,
			TypeName = "bool",
			Version = 1,
			MetaFlag = align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags,
		};
	}

	private static UniversalNode CreateArrayNode(string name, UniversalNode dataNode, bool align)
	{
		UniversalNode arrayNode = new()
		{
			Name = name,
			TypeName = "Array",
			Version = 1,
			MetaFlag = align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags,
		};
		arrayNode.SubNodes.Add(new()
		{
			Name = "size",
			TypeName = "SInt32",
			Version = 1,
			MetaFlag = TransferMetaFlags.NoTransferFlags,
		});
		dataNode.Name = "m_Data";
		dataNode.OriginalName = "data";
		arrayNode.SubNodes.Add(dataNode);
		return arrayNode;
	}

	private static UniversalNode CreateArrayNode(string name, UniversalNode dataNode, UnityVersion version)
	{
		return CreateArrayNode(name, dataNode, version >= ArrayAlignmentStartVersion);
	}

	private static void AddSubNode(UniversalNode releaseRoot, UniversalNode editorRoot, VersionedList<UniversalClass> sourceList, UnityVersion version, string name)
	{
		releaseRoot.SubNodes.Add(sourceList.GetItemForVersion(version)!.ReleaseRootNode!.DeepCloneAndChangeName(name));
		editorRoot.SubNodes.Add(sourceList.GetItemForVersion(version)!.EditorRootNode!.DeepCloneAndChangeName(name));
	}

	private static void AddInt32SubNode(UniversalNode releaseRoot, UniversalNode editorRoot, string name)
	{
		releaseRoot.SubNodes.Add(CreateInt32Node(name));
		editorRoot.SubNodes.Add(CreateInt32Node(name));
	}

	private static void AddSingleSubNode(UniversalNode releaseRoot, UniversalNode editorRoot, string name)
	{
		releaseRoot.SubNodes.Add(CreateSingleNode(name));
		editorRoot.SubNodes.Add(CreateSingleNode(name));
	}

	private static void AddBooleanSubNode(UniversalNode releaseRoot, UniversalNode editorRoot, string name, bool align)
	{
		releaseRoot.SubNodes.Add(CreateBooleanNode(name, align));
		editorRoot.SubNodes.Add(CreateBooleanNode(name, align));
	}

	/// <summary>
	/// 4.0.0 and greater
	/// GUIStyle became builtin serializable only in v4.0.0
	/// </summary>
	private static bool IsBuiltinFormat(UnityVersion version) => version.GreaterThanOrEquals(4);
	/// <summary>
	/// 3.0.0 and greater
	/// </summary>
	private static bool HasFontSize(UnityVersion version) => version.GreaterThanOrEquals(3);
	private static UnityVersion ArrayAlignmentStartVersion { get; } = new UnityVersion(2017, 1);
}
