# AssetClassGenerator

A code generator that parses exported type tree text file from [AssetStudio](https://github.com/Perfare/AssetStudio) to C# class declarations with `[GenerateSerde]` applied. See [Serde.NET](https://github.com/serdedotnet/serde) and [my fork](https://github.com/Deficuet/Serde.NET). 

## Usage
```
AssetClassGenerator.exe </path/to/typeTree1.txt> [/path/to/typeTree2.txt] ...
```

## Features
1. Classes with the same name but different fields will be renamed with a suffix `_1`, `_2`, etc.
2. Invalid field names will be renamed in camelCase with attribute `[SerdeMemberOptions(Rename = "...")]` applied for storing the original field name. (Beta)
3. Some classes are renamed to avoid name conflicts when copy-paste, include class names inside PPtr e.g. `PPtr<Object>`
   - `Object` to `UnityObject`
   - `Component` to `UnityComponent`
4. Some "well known" value types are skipped: `ColorRGBA`, `Matrix4x4f`, `Quaternionf`, `Vector2f`, `Vector3f`, `Vector4f`, `GUID`, `Hash128`, `float3`
   - `float3` is automatically remapped as `Vector3f`
   - `float4` can be `Vector4f` or `Quaternion` so it is leaved as-is. 
5. Some parent classes will be detected: `NamedObject`, `UnityComponent`, `Behaviour`, `MonoBehaviour`. And the fields will be removed in the generated code. By default an object (root class) will inherit `UnityObject`. 

## Example
- [Type tree](#type-tree)
- [Generated C# classes](#generated-classes)

### Type tree
`AnimationController` in `2022.3.51f1`
```
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
### Generated classes
```C#
[GenerateSerde]
public partial record AnimatorController : NamedObject
{
    public required uint m_ControllerSize;
    public required ControllerConstant m_Controller;
    public required MultiDictionary<uint, string> m_TOS;
    public required PPtr<AnimationClip>[] m_AnimationClips;
    public required StateMachineBehaviourVectorDescription m_StateMachineBehaviourVectorDescription;
    public required PPtr<MonoBehaviour>[] m_StateMachineBehaviours;
    public required bool m_MultiThreadedStateMachine;
}

[GenerateSerde]
public partial record HumanPoseMask
{
    public required uint word0;
    public required uint word1;
    public required uint word2;
}

[GenerateSerde]
public partial record SkeletonMaskElement
{
    public required uint m_PathHash;
    public required float m_Weight;
}

[GenerateSerde]
public partial record ConditionConstant
{
    public required uint m_ConditionMode;
    public required uint m_EventID;
    public required float m_EventThreshold;
    public required float m_ExitTime;
}

[GenerateSerde]
public partial record Blend1dDataConstant
{
    public required float[] m_ChildThresholdArray;
}

[GenerateSerde]
public partial record MotionNeighborList
{
    public required uint[] m_NeighborArray;
}

[GenerateSerde]
public partial record BlendDirectDataConstant
{
    public required uint[] m_ChildBlendEventIDArray;
    public required bool m_NormalizedBlendValues;
}

[GenerateSerde]
public partial record ValueConstant
{
    public required uint m_ID;
    public required uint m_Type;
    public required uint m_Index;
}

[GenerateSerde]
public partial record float4
{
    public required float x;
    public required float y;
    public required float z;
    public required float w;
}

[GenerateSerde]
public partial record StateKey
{
    public required uint m_StateID;
    public required int m_LayerIndex;
}

[GenerateSerde]
public partial record StateRange
{
    public required uint m_StartIndex;
    public required uint m_Count;
}

[GenerateSerde]
public partial record SkeletonMask
{
    public required SkeletonMaskElement[] m_Data;
}

[GenerateSerde]
public partial record OffsetPtr_ConditionConstant
{
    public required ConditionConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_Blend1dDataConstant
{
    public required Blend1dDataConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_BlendDirectDataConstant
{
    public required BlendDirectDataConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_SkeletonMask
{
    public required SkeletonMask data;
}

[GenerateSerde]
public partial record OffsetPtr_Blend2dDataConstant
{
    public required Blend2dDataConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_ValueArrayConstant
{
    public required ValueArrayConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_ValueArray
{
    public required ValueArray data;
}

[GenerateSerde]
public partial record OffsetPtr_TransitionConstant
{
    public required TransitionConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_SelectorTransitionConstant
{
    public required SelectorTransitionConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_LayerConstant
{
    public required LayerConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_BlendTreeNodeConstant
{
    public required BlendTreeNodeConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_SelectorStateConstant
{
    public required SelectorStateConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_BlendTreeConstant
{
    public required BlendTreeConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_StateConstant
{
    public required StateConstant data;
}

[GenerateSerde]
public partial record OffsetPtr_StateMachineConstant
{
    public required StateMachineConstant data;
}

[GenerateSerde]
[UseProxy(ForType = typeof(Vector2), Proxy = typeof(Vector2Proxy))]
public partial record Blend2dDataConstant
{
    public required Vector2[] m_ChildPositionArray;
    public required float[] m_ChildMagnitudeArray;
    public required Vector2[] m_ChildPairVectorArray;
    public required float[] m_ChildPairAvgMagInvArray;
    public required MotionNeighborList[] m_ChildNeighborListArray;
}

[GenerateSerde]
public partial record ValueArrayConstant
{
    public required ValueConstant[] m_ValueArray;
}

[GenerateSerde]
[UseProxy(ForType = typeof(Vector3), Proxy = typeof(Vector3Proxy))]
public partial record ValueArray
{
    public required Vector3[] m_PositionValues;
    public required float4[] m_QuaternionValues;
    public required Vector3[] m_ScaleValues;
    public required float[] m_FloatValues;
    public required int[] m_IntValues;
    public required bool[] m_BoolValues;
}

[GenerateSerde]
public partial record StateMachineBehaviourVectorDescription
{
    public required MultiDictionary<StateKey, StateRange> m_StateMachineBehaviourRanges;
    public required uint[] m_StateMachineBehaviourIndices;
}

[GenerateSerde]
public partial record TransitionConstant
{
    public required OffsetPtr_ConditionConstant[] m_ConditionConstantArray;
    public required uint m_DestinationState;
    public required uint m_FullPathID;
    public required uint m_ID;
    public required uint m_UserID;
    public required float m_TransitionDuration;
    public required float m_TransitionOffset;
    public required float m_ExitTime;
    public required bool m_HasExitTime;
    public required bool m_HasFixedDuration;
    public required int m_InterruptionSource;
    public required bool m_OrderedInterruption;
    public required bool m_CanTransitionToSelf;
}

[GenerateSerde]
public partial record SelectorTransitionConstant
{
    public required uint m_Destination;
    public required OffsetPtr_ConditionConstant[] m_ConditionConstantArray;
}

[GenerateSerde]
public partial record LayerConstant
{
    public required uint m_StateMachineIndex;
    public required uint m_StateMachineSynchronizedLayerIndex;
    public required HumanPoseMask m_BodyMask;
    public required OffsetPtr_SkeletonMask m_SkeletonMask;
    public required uint m_Binding;
    [SerdeMemberOptions(Rename = "(int&)m_LayerBlendingMode")]
    public required int intMlayerblendingmode;
    public required float m_DefaultWeight;
    public required bool m_IKPass;
    public required bool m_SyncedLayerAffectsTiming;
}

[GenerateSerde]
public partial record BlendTreeNodeConstant
{
    public required uint m_BlendType;
    public required uint m_BlendEventID;
    public required uint m_BlendEventYID;
    public required uint[] m_ChildIndices;
    public required OffsetPtr_Blend1dDataConstant m_Blend1dData;
    public required OffsetPtr_Blend2dDataConstant m_Blend2dData;
    public required OffsetPtr_BlendDirectDataConstant m_BlendDirectData;
    public required uint m_ClipID;
    public required float m_Duration;
    public required float m_CycleOffset;
    public required bool m_Mirror;
}

[GenerateSerde]
public partial record SelectorStateConstant
{
    public required OffsetPtr_SelectorTransitionConstant[] m_TransitionConstantArray;
    public required uint m_FullPathID;
    public required bool m_IsEntry;
}

[GenerateSerde]
public partial record BlendTreeConstant
{
    public required OffsetPtr_BlendTreeNodeConstant[] m_NodeArray;
}

[GenerateSerde]
public partial record StateConstant
{
    public required OffsetPtr_TransitionConstant[] m_TransitionConstantArray;
    public required int[] m_BlendTreeConstantIndexArray;
    public required OffsetPtr_BlendTreeConstant[] m_BlendTreeConstantArray;
    public required uint m_NameID;
    public required uint m_PathID;
    public required uint m_FullPathID;
    public required uint m_TagID;
    public required uint m_SpeedParamID;
    public required uint m_MirrorParamID;
    public required uint m_CycleOffsetParamID;
    public required uint m_TimeParamID;
    public required float m_Speed;
    public required float m_CycleOffset;
    public required bool m_IKOnFeet;
    public required bool m_WriteDefaultValues;
    public required bool m_Loop;
    public required bool m_Mirror;
}

[GenerateSerde]
public partial record StateMachineConstant
{
    public required OffsetPtr_StateConstant[] m_StateConstantArray;
    public required OffsetPtr_TransitionConstant[] m_AnyStateTransitionConstantArray;
    public required OffsetPtr_SelectorStateConstant[] m_SelectorStateConstantArray;
    public required uint m_DefaultState;
    public required uint m_SynchronizedLayerCount;
}

[GenerateSerde]
public partial record ControllerConstant
{
    public required OffsetPtr_LayerConstant[] m_LayerArray;
    public required OffsetPtr_StateMachineConstant[] m_StateMachineArray;
    public required OffsetPtr_ValueArrayConstant m_Values;
    public required OffsetPtr_ValueArray m_DefaultValues;
}
```
