# AssetClassGenerator

A batch C# code generator for Unity type trees.

The generator reads a TPK class database, optionally reads IL2CPP metadata for custom MonoBehaviours, builds a model for every available built-in `AssetClassID`, and writes C# declarations for Unity object classes and the runtime classes nested inside them. It replaces the old AssetStudio text-file workflow and does not use Serde.

Generated declarations carry:

- `AssetManager.TypeTree.TypeTreeSource` on every Unity object class, recording the original type-tree source.
- `AssetManager.TypeTree.TypeTreeClass` on derived classes and on every member, recording the original type-tree class/type name.
- `PolyType.PropertyShape` on every member, recording the original type-tree member name for PolyType shapes.

## Usage

```
AssetClassGenerator.exe <path/to/options.json>
```

### options.json

```json
{
  "ClassTpkPath": "path/to/uncompressed.tpk",
  "BuiltinClassNamespace": "Sample.Builtin",
  "UnityVersion": "2022.3.62f3",
  "GameAssemblyPath": "path/to/GameAssembly.dll",
  "MetadataPath": "path/to/global-metadata.dat",
  "CustomClassNamespace": "Sample.Custom",
  "CustomClassSources": [
    {
      "AssemblyName": "Assembly-CSharp",
      "Namespace": "My.Game",
      "ClassName": "MyBehaviour"
    }
  ]
}
```

- `ClassTpkPath`, `BuiltinClassNamespace`, and `UnityVersion` are required.
- `GameAssemblyPath`, `MetadataPath`, and `CustomClassNamespace` are only required when `CustomClassSources` is non-empty.
- All paths are resolved relative to the JSON file when they are not absolute.
- `CustomClassSources` empty or omitted means only built-in classes are generated.

## Features

1. **Built-in Unity object classes**: every `AssetClassID` is tried; IDs without a usable type tree are skipped and reported. All generated Unity object classes are `public class` and inherit `UnityObject` directly or through another generated Unity object class.
2. **Custom MonoBehaviours**: `CustomClassSources` entries are resolved through IL2CPP and generated as `public class` types inheriting the built-in `MonoBehaviour` when its type tree is available (otherwise `UnityObject`).
3. **Shared `UnityObject` base**: an empty `public class UnityObject` is always generated in `BuiltinClassNamespace`.
4. **Known type handling**: `Info/KnownClassMap.TypeInfoMap` maps a type-tree class name and an expected `NodeDataType` to a fully qualified known type. The same class name may map to several structures; the first matching structure wins. PPtr is handled separately because its class name varies by target.
5. **Derived classes**: type-tree classes that are not known types become generated `record` or `record struct` types. A derived class is a `record struct` only when every member type is a value type.
6. **Deduplication and naming**: structurally identical derived classes share one model. Same-name classes with different structures, names that collide with known type names, names that collide with Unity object classes, and invalid C# identifiers get an `_<8 hex>` suffix derived from the structural hash. For example, a type-tree class named `string` with an `id` member is generated as `string_<hash>` and can be used as a `MultiDictionary` key instead of C# `string`.
7. **Inheritance**: `Info/UnityObjectInheritanceInfo.BaseTypeMap` defines the potential base class of a built-in Unity object. The parent type tree's members are matched against the start of the child's member list; matched members are removed from the generated class and the generated class inherits the parent. Otherwise it inherits `UnityObject`.
8. **Nested output folders**: `Generated` in the process current directory is recreated for every run. A namespace is written as one folder per segment, for example `Sample.Builtin` becomes `Generated/Sample/Builtin/`. Each Unity object class gets its own `.cs` file; all derived classes for that namespace are written to `classes.cs`.
9. **No `using` directives**: primitive types and `string` use C# keywords; every other type, base class, and attribute is written with a `global::` fully qualified name.

### Known type examples

| Type tree structure | DataType | Generated type |
|---|---|---|
| `ColorRGBA` with `r/g/b/a` floats | `ColorRGBA4f` | `UnityEngine.Color` |
| `ColorRGBA` with packed `rgba` uint | `ColorRGBA32` | `UnityEngine.Color32` |
| `Vector2f` with `x/y` floats | `Pos2f` | `UnityEngine.Vector2` |
| `Vector3f` or `float3` with `x/y/z` floats | `Pos3f` | `UnityEngine.Vector3` |
| `Vector4f` with `x/y/z/w` floats | `Pos4f` | `UnityEngine.Vector4` |
| `Quaternionf` with `x/y/z/w` floats | `Pos4f` | `UnityEngine.Quaternion` |
| `Matrix4x4f` with `e00..e33` floats | `Matrix4x4f` | `UnityEngine.Matrix4x4` |
| `GUID` | `Guid` | `AssetsTools.NET.GUID128` |
| `Hash128` | `Hash128` | `AssetsTools.NET.Hash128` |
| `PPtr<T>` | `PPtr` | `AssetsTools.NET.AssetPPtr` |
| `map` | `Map` | `AssetManager.Collections.MultiDictionary<K,V>` |
| `pair` | `Pair` | `AssetManager.Collections.MapPair<K,V>` |

`float4` has the same shape as `Vector4f`/`Quaternionf` but intentionally has no `TypeInfoMap` entry, so it remains a generated `record struct float4`.

## Example

The example is the built-in `AnimatorController` class in Unity `2022.3.62f3`, generated into `Sample.Builtin`. The generated files are:

```
Generated/Sample/Builtin/UnityObject.cs
Generated/Sample/Builtin/AnimatorController.cs
Generated/Sample/Builtin/classes.cs
```

### Type tree

```text
AnimatorController Base -1 False
	string m_Name -1 True
		Array Array -1 True
			int size 4 False
			char data 1 False
	unsigned int m_ControllerSize 4 False
	ControllerConstant m_Controller -1 False
		vector m_LayerArray -1 False
			Array Array -1 False
				int size 4 False
				OffsetPtr data -1 False
					LayerConstant data -1 False
						unsigned int m_StateMachineIndex 4 False
						unsigned int m_StateMachineSynchronizedLayerIndex 4 False
						HumanPoseMask m_BodyMask 12 False
							unsigned int word0 4 False
							unsigned int word1 4 False
							unsigned int word2 4 False
						OffsetPtr m_SkeletonMask -1 False
							SkeletonMask data -1 False
								vector m_Data -1 False
									Array Array -1 False
										int size 4 False
										SkeletonMaskElement data 8 False
											unsigned int m_PathHash 4 False
											float m_Weight 4 False
						unsigned int m_Binding 4 False
						int (int&)m_LayerBlendingMode 4 False
						float m_DefaultWeight 4 False
						bool m_IKPass 1 False
						bool m_SyncedLayerAffectsTiming 1 True
		vector m_StateMachineArray -1 False
			Array Array -1 False
				int size 4 False
				OffsetPtr data -1 False
					StateMachineConstant data -1 False
						vector m_StateConstantArray -1 False
							Array Array -1 False
								int size 4 False
								OffsetPtr data -1 False
									StateConstant data -1 False
										vector m_TransitionConstantArray -1 False
											Array Array -1 False
												int size 4 False
												OffsetPtr data -1 False
													TransitionConstant data -1 False
														vector m_ConditionConstantArray -1 False
															Array Array -1 False
																int size 4 False
																OffsetPtr data 16 False
																	ConditionConstant data 16 False
																		unsigned int m_ConditionMode 4 False
																		unsigned int m_EventID 4 False
																		float m_EventThreshold 4 False
																		float m_ExitTime 4 False
														unsigned int m_DestinationState 4 False
														unsigned int m_FullPathID 4 False
														unsigned int m_ID 4 False
														unsigned int m_UserID 4 False
														float m_TransitionDuration 4 False
														float m_TransitionOffset 4 False
														float m_ExitTime 4 False
														bool m_HasExitTime 1 False
														bool m_HasFixedDuration 1 True
														int m_InterruptionSource 4 False
														bool m_OrderedInterruption 1 False
														bool m_CanTransitionToSelf 1 True
										vector m_BlendTreeConstantIndexArray -1 False
											Array Array -1 False
												int size 4 False
												int data 4 False
										vector m_BlendTreeConstantArray -1 False
											Array Array -1 False
												int size 4 False
												OffsetPtr data -1 False
													BlendTreeConstant data -1 False
														vector m_NodeArray -1 False
															Array Array -1 False
																int size 4 False
																OffsetPtr data -1 False
																	BlendTreeNodeConstant data -1 False
																		unsigned int m_BlendType 4 False
																		unsigned int m_BlendEventID 4 False
																		unsigned int m_BlendEventYID 4 False
																		vector m_ChildIndices -1 False
																			Array Array -1 False
																				int size 4 False
																				unsigned int data 4 False
																		OffsetPtr m_Blend1dData -1 False
																			Blend1dDataConstant data -1 False
																				vector m_ChildThresholdArray -1 False
																					Array Array -1 False
																						int size 4 False
																						float data 4 False
																		OffsetPtr m_Blend2dData -1 False
																			Blend2dDataConstant data -1 False
																				vector m_ChildPositionArray -1 False
																					Array Array -1 False
																						int size 4 False
																						Vector2f data 8 False
																							float x 4 False
																							float y 4 False
																				vector m_ChildMagnitudeArray -1 False
																					Array Array -1 False
																						int size 4 False
																						float data 4 False
																				vector m_ChildPairVectorArray -1 False
																					Array Array -1 False
																						int size 4 False
																						Vector2f data 8 False
																							float x 4 False
																							float y 4 False
																				vector m_ChildPairAvgMagInvArray -1 False
																					Array Array -1 False
																						int size 4 False
																						float data 4 False
																				vector m_ChildNeighborListArray -1 False
																					Array Array -1 False
																						int size 4 False
																						MotionNeighborList data -1 False
																							vector m_NeighborArray -1 False
																								Array Array -1 False
																									int size 4 False
																									unsigned int data 4 False
																		OffsetPtr m_BlendDirectData -1 False
																			BlendDirectDataConstant data -1 False
																				vector m_ChildBlendEventIDArray -1 False
																					Array Array -1 False
																						int size 4 False
																						unsigned int data 4 False
																				bool m_NormalizedBlendValues 1 True
																		unsigned int m_ClipID 4 False
																		float m_Duration 4 False
																		float m_CycleOffset 4 False
																		bool m_Mirror 1 True
										unsigned int m_NameID 4 False
										unsigned int m_PathID 4 False
										unsigned int m_FullPathID 4 False
										unsigned int m_TagID 4 False
										unsigned int m_SpeedParamID 4 False
										unsigned int m_MirrorParamID 4 False
										unsigned int m_CycleOffsetParamID 4 False
										unsigned int m_TimeParamID 4 False
										float m_Speed 4 False
										float m_CycleOffset 4 False
										bool m_IKOnFeet 1 False
										bool m_WriteDefaultValues 1 False
										bool m_Loop 1 False
										bool m_Mirror 1 True
						vector m_AnyStateTransitionConstantArray -1 False
							Array Array -1 False
								int size 4 False
								OffsetPtr data -1 False
									TransitionConstant data -1 False
										vector m_ConditionConstantArray -1 False
											Array Array -1 False
												int size 4 False
												OffsetPtr data 16 False
													ConditionConstant data 16 False
														unsigned int m_ConditionMode 4 False
														unsigned int m_EventID 4 False
														float m_EventThreshold 4 False
														float m_ExitTime 4 False
										unsigned int m_DestinationState 4 False
										unsigned int m_FullPathID 4 False
										unsigned int m_ID 4 False
										unsigned int m_UserID 4 False
										float m_TransitionDuration 4 False
										float m_TransitionOffset 4 False
										float m_ExitTime 4 False
										bool m_HasExitTime 1 False
										bool m_HasFixedDuration 1 True
										int m_InterruptionSource 4 False
										bool m_OrderedInterruption 1 False
										bool m_CanTransitionToSelf 1 True
						vector m_SelectorStateConstantArray -1 False
							Array Array -1 False
								int size 4 False
								OffsetPtr data -1 False
									SelectorStateConstant data -1 False
										vector m_TransitionConstantArray -1 False
											Array Array -1 False
												int size 4 False
												OffsetPtr data -1 False
													SelectorTransitionConstant data -1 False
														unsigned int m_Destination 4 False
														vector m_ConditionConstantArray -1 False
															Array Array -1 False
																int size 4 False
																OffsetPtr data 16 False
																	ConditionConstant data 16 False
																		unsigned int m_ConditionMode 4 False
																		unsigned int m_EventID 4 False
																		float m_EventThreshold 4 False
																		float m_ExitTime 4 False
										unsigned int m_FullPathID 4 False
										bool m_IsEntry 1 True
						unsigned int m_DefaultState 4 False
						unsigned int m_SynchronizedLayerCount 4 False
		OffsetPtr m_Values -1 False
			ValueArrayConstant data -1 False
				vector m_ValueArray -1 False
					Array Array -1 False
						int size 4 False
						ValueConstant data 12 False
							unsigned int m_ID 4 False
							unsigned int m_Type 4 False
							unsigned int m_Index 4 False
		OffsetPtr m_DefaultValues -1 False
			ValueArray data -1 False
				vector m_PositionValues -1 False
					Array Array -1 False
						int size 4 False
						float3 data 12 False
							float x 4 False
							float y 4 False
							float z 4 False
				vector m_QuaternionValues -1 False
					Array Array -1 False
						int size 4 False
						float4 data 16 False
							float x 4 False
							float y 4 False
							float z 4 False
							float w 4 False
				vector m_ScaleValues -1 False
					Array Array -1 False
						int size 4 False
						float3 data 12 False
							float x 4 False
							float y 4 False
							float z 4 False
				vector m_FloatValues -1 False
					Array Array -1 False
						int size 4 False
						float data 4 False
				vector m_IntValues -1 False
					Array Array -1 False
						int size 4 False
						int data 4 False
				vector m_BoolValues -1 True
					Array Array -1 True
						int size 4 False
						bool data 1 False
	map m_TOS -1 False
		Array Array -1 False
			int size 4 False
			pair data -1 False
				unsigned int first 4 False
				string second -1 False
					Array Array -1 True
						int size 4 False
						char data 1 False
	vector m_AnimationClips -1 False
		Array Array -1 True
			int size 4 False
			PPtr<AnimationClip> data 12 False
				int m_FileID 4 False
				SInt64 m_PathID 8 False
	StateMachineBehaviourVectorDescription m_StateMachineBehaviourVectorDescription -1 False
		map m_StateMachineBehaviourRanges -1 False
			Array Array -1 False
				int size 4 False
				pair data 16 False
					StateKey first 8 False
						unsigned int m_StateID 4 False
						int m_LayerIndex 4 False
					StateRange second 8 False
						unsigned int m_StartIndex 4 False
						unsigned int m_Count 4 False
		vector m_StateMachineBehaviourIndices -1 False
			Array Array -1 True
				int size 4 False
				unsigned int data 4 False
	vector m_StateMachineBehaviours -1 False
		Array Array -1 True
			int size 4 False
			PPtr<MonoBehaviour> data 12 False
				int m_FileID 4 False
				SInt64 m_PathID 8 False
	bool m_MultiThreadedStateMachine 1 True
```

### AnimatorController.cs

```csharp
namespace Sample.Builtin
{
    [global::AssetManager.TypeTree.TypeTreeSource("UnityEngine", "UnityEngine", "AnimatorController")]
    public class AnimatorController : global::Sample.Builtin.UnityObject
    {
        [global::AssetManager.TypeTree.TypeTreeClass("string")]
        [global::PolyType.PropertyShape(Name = "m_Name")]
        public string m_Name { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_ControllerSize")]
        public uint m_ControllerSize { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("ControllerConstant")]
        [global::PolyType.PropertyShape(Name = "m_Controller")]
        public global::Sample.Builtin.ControllerConstant m_Controller { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("map<unsigned int,string>")]
        [global::PolyType.PropertyShape(Name = "m_TOS")]
        public global::UObjects.Collections.MultiDictionary<uint, string> m_TOS { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<PPtr<AnimationClip>>")]
        [global::PolyType.PropertyShape(Name = "m_AnimationClips")]
        public global::AssetsTools.NET.AssetPPtr[] m_AnimationClips { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("StateMachineBehaviourVectorDescription")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineBehaviourVectorDescription")]
        public global::Sample.Builtin.StateMachineBehaviourVectorDescription m_StateMachineBehaviourVectorDescription { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<PPtr<MonoBehaviour>>")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineBehaviours")]
        public global::AssetsTools.NET.AssetPPtr[] m_StateMachineBehaviours { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_MultiThreadedStateMachine")]
        public bool m_MultiThreadedStateMachine { get; set; }
    }
}
```

### classes.cs

```csharp
namespace Sample.Builtin
{
    [global::AssetManager.TypeTree.TypeTreeClass("Blend1dDataConstant")]
    public record Blend1dDataConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<float>")]
        [global::PolyType.PropertyShape(Name = "m_ChildThresholdArray")]
        public float[] m_ChildThresholdArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("Blend2dDataConstant")]
    public record Blend2dDataConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<Vector2f>")]
        [global::PolyType.PropertyShape(Name = "m_ChildPositionArray")]
        public global::UnityEngine.Vector2[] m_ChildPositionArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<float>")]
        [global::PolyType.PropertyShape(Name = "m_ChildMagnitudeArray")]
        public float[] m_ChildMagnitudeArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<Vector2f>")]
        [global::PolyType.PropertyShape(Name = "m_ChildPairVectorArray")]
        public global::UnityEngine.Vector2[] m_ChildPairVectorArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<float>")]
        [global::PolyType.PropertyShape(Name = "m_ChildPairAvgMagInvArray")]
        public float[] m_ChildPairAvgMagInvArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<MotionNeighborList>")]
        [global::PolyType.PropertyShape(Name = "m_ChildNeighborListArray")]
        public global::Sample.Builtin.MotionNeighborList[] m_ChildNeighborListArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("BlendDirectDataConstant")]
    public record BlendDirectDataConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<unsigned int>")]
        [global::PolyType.PropertyShape(Name = "m_ChildBlendEventIDArray")]
        public uint[] m_ChildBlendEventIDArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_NormalizedBlendValues")]
        public bool m_NormalizedBlendValues { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("BlendTreeConstant")]
    public record BlendTreeConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_NodeArray")]
        public global::Sample.Builtin.OffsetPtr_18a1a5da[] m_NodeArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("BlendTreeNodeConstant")]
    public record BlendTreeNodeConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_BlendType")]
        public uint m_BlendType { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_BlendEventID")]
        public uint m_BlendEventID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_BlendEventYID")]
        public uint m_BlendEventYID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<unsigned int>")]
        [global::PolyType.PropertyShape(Name = "m_ChildIndices")]
        public uint[] m_ChildIndices { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_Blend1dData")]
        public global::Sample.Builtin.OffsetPtr_aa29e699 m_Blend1dData { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_Blend2dData")]
        public global::Sample.Builtin.OffsetPtr_9966b365 m_Blend2dData { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_BlendDirectData")]
        public global::Sample.Builtin.OffsetPtr_9e2fe326 m_BlendDirectData { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_ClipID")]
        public uint m_ClipID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_Duration")]
        public float m_Duration { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_CycleOffset")]
        public float m_CycleOffset { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_Mirror")]
        public bool m_Mirror { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("ConditionConstant")]
    public record struct ConditionConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_ConditionMode")]
        public uint m_ConditionMode { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_EventID")]
        public uint m_EventID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_EventThreshold")]
        public float m_EventThreshold { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_ExitTime")]
        public float m_ExitTime { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("ControllerConstant")]
    public record ControllerConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_LayerArray")]
        public global::Sample.Builtin.OffsetPtr_22caeee8[] m_LayerArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineArray")]
        public global::Sample.Builtin.OffsetPtr_39db6cdc[] m_StateMachineArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_Values")]
        public global::Sample.Builtin.OffsetPtr_8cc9997b m_Values { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_DefaultValues")]
        public global::Sample.Builtin.OffsetPtr_e794aa8d m_DefaultValues { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("HumanPoseMask")]
    public record struct HumanPoseMask
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "word0")]
        public uint word0 { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "word1")]
        public uint word1 { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "word2")]
        public uint word2 { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("LayerConstant")]
    public record LayerConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineIndex")]
        public uint m_StateMachineIndex { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineSynchronizedLayerIndex")]
        public uint m_StateMachineSynchronizedLayerIndex { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("HumanPoseMask")]
        [global::PolyType.PropertyShape(Name = "m_BodyMask")]
        public global::Sample.Builtin.HumanPoseMask m_BodyMask { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
        [global::PolyType.PropertyShape(Name = "m_SkeletonMask")]
        public global::Sample.Builtin.OffsetPtr_b3eda9fa m_SkeletonMask { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_Binding")]
        public uint m_Binding { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("int")]
        [global::PolyType.PropertyShape(Name = "(int&)m_LayerBlendingMode")]
        public int intMlayerblendingmode { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_DefaultWeight")]
        public float m_DefaultWeight { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_IKPass")]
        public bool m_IKPass { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_SyncedLayerAffectsTiming")]
        public bool m_SyncedLayerAffectsTiming { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("MotionNeighborList")]
    public record MotionNeighborList
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<unsigned int>")]
        [global::PolyType.PropertyShape(Name = "m_NeighborArray")]
        public uint[] m_NeighborArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_18a1a5da
    {
        [global::AssetManager.TypeTree.TypeTreeClass("BlendTreeNodeConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.BlendTreeNodeConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_22caeee8
    {
        [global::AssetManager.TypeTree.TypeTreeClass("LayerConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.LayerConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_39db6cdc
    {
        [global::AssetManager.TypeTree.TypeTreeClass("StateMachineConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.StateMachineConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_3cb05302
    {
        [global::AssetManager.TypeTree.TypeTreeClass("BlendTreeConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.BlendTreeConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_6ca3c339
    {
        [global::AssetManager.TypeTree.TypeTreeClass("TransitionConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.TransitionConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_8cc9997b
    {
        [global::AssetManager.TypeTree.TypeTreeClass("ValueArrayConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.ValueArrayConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record struct OffsetPtr_91ef57d5
    {
        [global::AssetManager.TypeTree.TypeTreeClass("ConditionConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.ConditionConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_982ee56f
    {
        [global::AssetManager.TypeTree.TypeTreeClass("SelectorStateConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.SelectorStateConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_9966b365
    {
        [global::AssetManager.TypeTree.TypeTreeClass("Blend2dDataConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.Blend2dDataConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_9e2fe326
    {
        [global::AssetManager.TypeTree.TypeTreeClass("BlendDirectDataConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.BlendDirectDataConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_aa29e699
    {
        [global::AssetManager.TypeTree.TypeTreeClass("Blend1dDataConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.Blend1dDataConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_b3eda9fa
    {
        [global::AssetManager.TypeTree.TypeTreeClass("SkeletonMask")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.SkeletonMask data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_c102ff05
    {
        [global::AssetManager.TypeTree.TypeTreeClass("StateConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.StateConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_d3581342
    {
        [global::AssetManager.TypeTree.TypeTreeClass("SelectorTransitionConstant")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.SelectorTransitionConstant data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("OffsetPtr")]
    public record OffsetPtr_e794aa8d
    {
        [global::AssetManager.TypeTree.TypeTreeClass("ValueArray")]
        [global::PolyType.PropertyShape(Name = "data")]
        public global::Sample.Builtin.ValueArray data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("SelectorStateConstant")]
    public record SelectorStateConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_TransitionConstantArray")]
        public global::Sample.Builtin.OffsetPtr_d3581342[] m_TransitionConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_FullPathID")]
        public uint m_FullPathID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_IsEntry")]
        public bool m_IsEntry { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("SelectorTransitionConstant")]
    public record SelectorTransitionConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_Destination")]
        public uint m_Destination { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_ConditionConstantArray")]
        public global::Sample.Builtin.OffsetPtr_91ef57d5[] m_ConditionConstantArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("SkeletonMask")]
    public record SkeletonMask
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<SkeletonMaskElement>")]
        [global::PolyType.PropertyShape(Name = "m_Data")]
        public global::Sample.Builtin.SkeletonMaskElement[] m_Data { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("SkeletonMaskElement")]
    public record struct SkeletonMaskElement
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_PathHash")]
        public uint m_PathHash { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_Weight")]
        public float m_Weight { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("StateConstant")]
    public record StateConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_TransitionConstantArray")]
        public global::Sample.Builtin.OffsetPtr_6ca3c339[] m_TransitionConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<int>")]
        [global::PolyType.PropertyShape(Name = "m_BlendTreeConstantIndexArray")]
        public int[] m_BlendTreeConstantIndexArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_BlendTreeConstantArray")]
        public global::Sample.Builtin.OffsetPtr_3cb05302[] m_BlendTreeConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_NameID")]
        public uint m_NameID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_PathID")]
        public uint m_PathID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_FullPathID")]
        public uint m_FullPathID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_TagID")]
        public uint m_TagID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_SpeedParamID")]
        public uint m_SpeedParamID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_MirrorParamID")]
        public uint m_MirrorParamID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_CycleOffsetParamID")]
        public uint m_CycleOffsetParamID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_TimeParamID")]
        public uint m_TimeParamID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_Speed")]
        public float m_Speed { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_CycleOffset")]
        public float m_CycleOffset { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_IKOnFeet")]
        public bool m_IKOnFeet { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_WriteDefaultValues")]
        public bool m_WriteDefaultValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_Loop")]
        public bool m_Loop { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_Mirror")]
        public bool m_Mirror { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("StateKey")]
    public record struct StateKey
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_StateID")]
        public uint m_StateID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("int")]
        [global::PolyType.PropertyShape(Name = "m_LayerIndex")]
        public int m_LayerIndex { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("StateMachineBehaviourVectorDescription")]
    public record StateMachineBehaviourVectorDescription
    {
        [global::AssetManager.TypeTree.TypeTreeClass("map<StateKey,StateRange>")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineBehaviourRanges")]
        public global::UObjects.Collections.MultiDictionary<global::Sample.Builtin.StateKey, global::Sample.Builtin.StateRange> m_StateMachineBehaviourRanges { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<unsigned int>")]
        [global::PolyType.PropertyShape(Name = "m_StateMachineBehaviourIndices")]
        public uint[] m_StateMachineBehaviourIndices { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("StateMachineConstant")]
    public record StateMachineConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_StateConstantArray")]
        public global::Sample.Builtin.OffsetPtr_c102ff05[] m_StateConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_AnyStateTransitionConstantArray")]
        public global::Sample.Builtin.OffsetPtr_6ca3c339[] m_AnyStateTransitionConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_SelectorStateConstantArray")]
        public global::Sample.Builtin.OffsetPtr_982ee56f[] m_SelectorStateConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_DefaultState")]
        public uint m_DefaultState { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_SynchronizedLayerCount")]
        public uint m_SynchronizedLayerCount { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("StateRange")]
    public record struct StateRange
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_StartIndex")]
        public uint m_StartIndex { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_Count")]
        public uint m_Count { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("TransitionConstant")]
    public record TransitionConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<OffsetPtr>")]
        [global::PolyType.PropertyShape(Name = "m_ConditionConstantArray")]
        public global::Sample.Builtin.OffsetPtr_91ef57d5[] m_ConditionConstantArray { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_DestinationState")]
        public uint m_DestinationState { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_FullPathID")]
        public uint m_FullPathID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_ID")]
        public uint m_ID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_UserID")]
        public uint m_UserID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_TransitionDuration")]
        public float m_TransitionDuration { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_TransitionOffset")]
        public float m_TransitionOffset { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "m_ExitTime")]
        public float m_ExitTime { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_HasExitTime")]
        public bool m_HasExitTime { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_HasFixedDuration")]
        public bool m_HasFixedDuration { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("int")]
        [global::PolyType.PropertyShape(Name = "m_InterruptionSource")]
        public int m_InterruptionSource { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_OrderedInterruption")]
        public bool m_OrderedInterruption { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("bool")]
        [global::PolyType.PropertyShape(Name = "m_CanTransitionToSelf")]
        public bool m_CanTransitionToSelf { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("ValueArray")]
    public record ValueArray
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<float3>")]
        [global::PolyType.PropertyShape(Name = "m_PositionValues")]
        public global::UnityEngine.Vector3[] m_PositionValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<float4>")]
        [global::PolyType.PropertyShape(Name = "m_QuaternionValues")]
        public global::Sample.Builtin.float4[] m_QuaternionValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<float3>")]
        [global::PolyType.PropertyShape(Name = "m_ScaleValues")]
        public global::UnityEngine.Vector3[] m_ScaleValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<float>")]
        [global::PolyType.PropertyShape(Name = "m_FloatValues")]
        public float[] m_FloatValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<int>")]
        [global::PolyType.PropertyShape(Name = "m_IntValues")]
        public int[] m_IntValues { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("vector<bool>")]
        [global::PolyType.PropertyShape(Name = "m_BoolValues")]
        public bool[] m_BoolValues { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("ValueArrayConstant")]
    public record ValueArrayConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("vector<ValueConstant>")]
        [global::PolyType.PropertyShape(Name = "m_ValueArray")]
        public global::Sample.Builtin.ValueConstant[] m_ValueArray { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("ValueConstant")]
    public record struct ValueConstant
    {
        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_ID")]
        public uint m_ID { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_Type")]
        public uint m_Type { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("unsigned int")]
        [global::PolyType.PropertyShape(Name = "m_Index")]
        public uint m_Index { get; set; }
    }

    [global::AssetManager.TypeTree.TypeTreeClass("float4")]
    public record struct float4
    {
        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "x")]
        public float x { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "y")]
        public float y { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "z")]
        public float z { get; set; }

        [global::AssetManager.TypeTree.TypeTreeClass("float")]
        [global::PolyType.PropertyShape(Name = "w")]
        public float w { get; set; }
    }
}
```
