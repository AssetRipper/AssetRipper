from __future__ import annotations

import argparse
import json
import re
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

import pefile


GENERIC_VERSIONS = {"0.0.0", "0.0.0.0", "4.0.30319"}
LOW_SIGNAL_VERSIONS = {"1.0.0", "1.0.0.0"}
PACKAGE_TAG_RE = re.compile(
    r"(?i)(?:packages[/\\])?(?P<pkg>(?:com|io)\.[a-z0-9][a-z0-9._-]+)"
    r"@(?P<ver>[0-9]+(?:\.[0-9]+){1,3}(?:[-+][A-Za-z0-9_.-]+)?)"
)
PACKAGE_REF_RE = re.compile(
    r"(?i)(?:PackageCache[/\\])?(?P<pkg>(?:com|io|meta)\.[a-z0-9][a-z0-9._-]+)"
    r"(?:@(?P<ref>[A-Za-z0-9._-]+))?"
)
SEMVER_RE = re.compile(r"(?<![0-9])(?P<ver>[0-9]+(?:\.[0-9]+){1,3}(?:[-+][A-Za-z0-9_.-]+)?)(?![0-9])")
OPENXR_PLUGIN_VERSION_RE = re.compile(r"OpenXR Plug-?in version (?P<ver>[0-9]+(?:\.[0-9]+){1,3})", re.IGNORECASE)
ASCII_STRING_RE = re.compile(rb"[ -~]{4,}")
UTF16_STRING_RE = re.compile(rb"(?:[ -~]\x00){4,}")
OFFICIAL_PACKAGE_RE = re.compile(r"^(?:com|io)\.[a-z0-9][a-z0-9._-]+$")
IL_OPERAND_SIZES = {0x1F: 1, 0x20: 4, 0x28: 4, 0x6F: 4, 0x72: 4, 0x73: 4, 0x7E: 4, 0x7F: 4, 0x80: 4}


@dataclass
class FamilyHint:
    family: str
    display: str
    manifest_key: Optional[str] = None
    tokens: Tuple[str, ...] = ()


@dataclass(order=True)
class Candidate:
    sort_key: Tuple[int, int, int] = field(init=False, repr=False)
    confidence: int
    package: str
    version: str
    source: str
    exact: bool = True
    note: str = ""

    def __post_init__(self) -> None:
        self.sort_key = (self.confidence, self.version.count("."), len(self.version))


@dataclass
class ScanResult:
    file: str
    family: str
    display: str
    version: str
    source: str
    confidence: int
    exact: bool
    notes: List[str]
    assembly_name: str = ""
    assembly_version: str = ""
    explicit_packages: List[Tuple[str, str]] = field(default_factory=list)
    unresolved_reason: str = ""


FAMILY_HINTS: Dict[str, FamilyHint] = {
    "Cinemachine.dll": FamilyHint("com.unity.cinemachine", "Unity Cinemachine", "com.unity.cinemachine", ("cinemachine",)),
    "Unity.Addressables.dll": FamilyHint("com.unity.addressables", "Unity Addressables", "com.unity.addressables", ("addressables",)),
    "Unity.AI.Navigation.dll": FamilyHint("com.unity.ai.navigation", "Unity AI Navigation", "com.unity.ai.navigation", ("navigation",)),
    "Unity.Animation.Rigging.dll": FamilyHint("com.unity.animation.rigging", "Unity Animation Rigging", "com.unity.animation.rigging", ("rigging",)),
    "Unity.Animation.Rigging.DocCodeExamples.dll": FamilyHint("com.unity.animation.rigging", "Unity Animation Rigging", "com.unity.animation.rigging", ("rigging",)),
    "Unity.Burst.dll": FamilyHint("com.unity.burst", "Unity Burst", "com.unity.burst", ("burst",)),
    "Unity.Burst.Unsafe.dll": FamilyHint("com.unity.burst", "Unity Burst", "com.unity.burst", ("burst",)),
    "Unity.Collections.dll": FamilyHint("com.unity.collections", "Unity Collections", "com.unity.collections", ("collections",)),
    "Unity.Collections.LowLevel.ILSupport.dll": FamilyHint("com.unity.collections", "Unity Collections", "com.unity.collections", ("collections",)),
    "Autodesk.Fbx.dll": FamilyHint("com.autodesk.fbx", "Autodesk FBX", "com.autodesk.fbx", ("fbx", "autodesk")),
    "Unity.Formats.Fbx.Runtime.dll": FamilyHint("com.autodesk.fbx", "Autodesk FBX", "com.autodesk.fbx", ("fbx", "autodesk")),
    "Unity.InputSystem.dll": FamilyHint("com.unity.inputsystem", "Unity Input System", "com.unity.inputsystem", ("inputsystem",)),
    "Unity.InputSystem.ForUI.dll": FamilyHint("com.unity.inputsystem", "Unity Input System", "com.unity.inputsystem", ("inputsystem",)),
    "Unity.Mathematics.dll": FamilyHint("com.unity.mathematics", "Unity Mathematics", "com.unity.mathematics", ("mathematics",)),
    "Unity.ProBuilder.dll": FamilyHint("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ("probuilder",)),
    "Unity.ProBuilder.Csg.dll": FamilyHint("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ("probuilder",)),
    "Unity.ProBuilder.KdTree.dll": FamilyHint("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ("probuilder",)),
    "Unity.ProBuilder.Poly2Tri.dll": FamilyHint("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ("probuilder",)),
    "Unity.ProBuilder.Stl.dll": FamilyHint("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ("probuilder",)),
    "Unity.Profiling.Core.dll": FamilyHint("com.unity.profiling.core", "Unity Profiling Core", "com.unity.profiling.core", ("profiling",)),
    "Unity.RenderPipelines.Core.Runtime.dll": FamilyHint("com.unity.render-pipelines.core", "Unity RP Core", "com.unity.render-pipelines.core", ("render-pipelines", "srp")),
    "Unity.RenderPipelines.Core.ShaderLibrary.dll": FamilyHint("com.unity.render-pipelines.core", "Unity RP Core", "com.unity.render-pipelines.core", ("render-pipelines", "srp")),
    "Unity.RenderPipeline.Universal.ShaderLibrary.dll": FamilyHint("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ("urp", "universal")),
    "Unity.RenderPipelines.Universal.Runtime.dll": FamilyHint("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ("urp", "universal")),
    "Unity.RenderPipelines.Universal.Shaders.dll": FamilyHint("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ("urp", "universal")),
    "Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary.dll": FamilyHint("com.unity.shadergraph", "Unity Shader Graph", "com.unity.shadergraph", ("shadergraph",)),
    "Unity.ResourceManager.dll": FamilyHint("com.unity.addressables", "Unity Addressables", "com.unity.addressables", ("addressables", "resourcemanager")),
    "Unity.ScriptableBuildPipeline.dll": FamilyHint("com.unity.scriptablebuildpipeline", "Unity Scriptable Build Pipeline", "com.unity.scriptablebuildpipeline", ("scriptablebuildpipeline",)),
    "Unity.Splines.dll": FamilyHint("com.unity.splines", "Unity Splines", "com.unity.splines", ("splines",)),
    "Unity.TextMeshPro.dll": FamilyHint("com.unity.textmeshpro", "TextMeshPro", "com.unity.textmeshpro", ("textmeshpro",)),
    "Unity.Timeline.dll": FamilyHint("com.unity.timeline", "Unity Timeline", "com.unity.timeline", ("timeline",)),
    "Unity.XR.CoreUtils.dll": FamilyHint("com.unity.xr.core-utils", "XR Core Utils", "com.unity.xr.core-utils", ("xr", "coreutils")),
    "Unity.XR.Interaction.Toolkit.dll": FamilyHint("com.unity.xr.interaction.toolkit", "XR Interaction Toolkit", "com.unity.xr.interaction.toolkit", ("xr", "interaction toolkit")),
    "Unity.XR.Management.dll": FamilyHint("com.unity.xr.management", "XR Management", "com.unity.xr.management", ("xr", "management")),
    "Unity.XR.Oculus.dll": FamilyHint("com.unity.xr.oculus", "Unity XR Oculus", "com.unity.xr.oculus", ("xr", "oculus")),
    "Unity.XR.OpenXR.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "Unity.XR.OpenXR.Features.ConformanceAutomation.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "Unity.XR.OpenXR.Features.MetaQuestSupport.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "Unity.XR.OpenXR.Features.MockRuntime.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "Unity.XR.OpenXR.Features.OculusQuestSupport.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "Unity.XR.OpenXR.Features.RuntimeDebugger.dll": FamilyHint("com.unity.xr.openxr", "Unity XR OpenXR", "com.unity.xr.openxr", ("openxr", "unity.xr.openxr")),
    "UnityEngine.SpatialTracking.dll": FamilyHint("com.unity.xr.legacyinputhelpers", "XR Legacy Input Helpers", "com.unity.xr.legacyinputhelpers", ("legacyinputhelpers", "spatialtracking")),
    "UnityEngine.XR.LegacyInputHelpers.dll": FamilyHint("com.unity.xr.legacyinputhelpers", "XR Legacy Input Helpers", "com.unity.xr.legacyinputhelpers", ("legacyinputhelpers",)),
    "UnityEngine.UI.dll": FamilyHint("com.unity.ugui", "Unity UI", "com.unity.ugui", ("ugui", "ui")),
