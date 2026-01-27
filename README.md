# AssetClassGenerator

A code generator that parses exported type tree text file from [AssetStudio](https://github.com/Perfare/AssetStudio) to C# class declarations with `[GenerateSerde]` applied. See [Serde.NET](https://github.com/serdedotnet/serde) and [my fork](https://github.com/Deficuet/Serde.NET). 

## Usage
```
AssetClassGenerator.exe </path/to/typeTree1.txt> [/path/to/typeTree2.txt] ...
```

## Features
1. Classes with the same name but different fields are automatically detected and renamed with a suffix `_1`, `_2`, etc.
2. Invalid field names are automatically renamed in camelCase with attribute `[SerdeMemberOptions(Rename = "...")]` applied for storing the original field name. (Beta)
3. Some "well known" value types are skipped: `Colorf`, `Matrix4x4f`, `Quaternionf`, `Vector2f`, `Vector3f`, `Vector4f`, `GUID`, `Hash128`
4. If the **first** field of the root `UnityObject` class is `string m_Name`, then the class will inherit `NamedObject` instead of default `UnityObject` and the first field `m_Name` will be removed.

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
public partial record AnimationClip : NamedObject
{
    public required bool m_Legacy;
    public required bool m_Compressed;
    public required bool m_UseHighQualityCurve;
    public required QuaternionCurve[] m_RotationCurves;
    public required CompressedAnimationCurve[] m_CompressedRotationCurves;
    public required Vector3Curve[] m_EulerCurves;
    public required Vector3Curve[] m_PositionCurves;
    public required Vector3Curve[] m_ScaleCurves;
    public required FloatCurve[] m_FloatCurves;
    public required PPtrCurve[] m_PPtrCurves;
    public required float m_SampleRate;
    public required int m_WrapMode;
    public required AABB m_Bounds;
    public required uint m_MuscleClipSize;
    public required ClipMuscleConstant m_MuscleClip;
    public required AnimationClipBindingConstant m_ClipBindingConstant;
    public required bool m_HasGenericRootTransform;
    public required bool m_HasMotionFloatCurves;
    public required AnimationEvent[] m_Events;
}

[GenerateSerde]
public partial record Keyframe_1
{
    public required float time;
    [SerdeMemberOptions(Rename = "value")]
    public required Quaternionf value_;
    public required Quaternionf inSlope;
    public required Quaternionf outSlope;
    public required int weightedMode;
    public required Quaternionf inWeight;
    public required Quaternionf outWeight;
}

[GenerateSerde]
public partial record Keyframe_2
{
    public required float time;
    [SerdeMemberOptions(Rename = "value")]
    public required Vector3f value_;
    public required Vector3f inSlope;
    public required Vector3f outSlope;
    public required int weightedMode;
    public required Vector3f inWeight;
    public required Vector3f outWeight;
}

[GenerateSerde]
public partial record Keyframe_3
{
    public required float time;
    [SerdeMemberOptions(Rename = "value")]
    public required float value_;
    public required float inSlope;
    public required float outSlope;
    public required int weightedMode;
    public required float inWeight;
    public required float outWeight;
}

[GenerateSerde]
public partial record PackedBitVector_1
{
    public required uint m_NumItems;
    public required byte[] m_Data;
    public required byte m_BitSize;
}

[GenerateSerde]
public partial record PackedBitVector_2
{
    public required uint m_NumItems;
    public required byte[] m_Data;
}

[GenerateSerde]
public partial record PackedBitVector_3
{
    public required uint m_NumItems;
    public required float m_Range;
    public required float m_Start;
    public required byte[] m_Data;
    public required byte m_BitSize;
}

[GenerateSerde]
public partial record PPtrKeyframe
{
    public required float time;
    [SerdeMemberOptions(Rename = "value")]
    public required PPtr<UnityObject> value_;
}

[GenerateSerde]
public partial record AABB
{
    public required Vector3f m_Center;
    public required Vector3f m_Extent;
}

[GenerateSerde]
public partial record float3
{
    public required float x;
    public required float y;
    public required float z;
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
public partial record StreamedClip
{
    public required uint[] data;
    public required ushort curveCount;
    public required ushort discreteCurveCount;
}

[GenerateSerde]
public partial record DenseClip
{
    public required int m_FrameCount;
    public required uint m_CurveCount;
    public required float m_SampleRate;
    public required float m_BeginTime;
    public required float[] m_SampleArray;
}

[GenerateSerde]
public partial record ConstantClip
{
    public required float[] data;
}

[GenerateSerde]
public partial record ValueDelta
{
    public required float m_Start;
    public required float m_Stop;
}

[GenerateSerde]
public partial record GenericBinding
{
    public required uint path;
    public required uint attribute;
    public required PPtr<UnityObject> script;
    public required int typeID;
    public required byte customType;
    public required byte isPPtrCurve;
    public required byte isIntCurve;
    public required byte isSerializeReferenceCurve;
}

[GenerateSerde]
public partial record AnimationEvent
{
    public required float time;
    public required string functionName;
    public required string data;
    public required PPtr<UnityObject> objectReferenceParameter;
    public required float floatParameter;
    public required int intParameter;
    public required int messageOptions;
}

[GenerateSerde]
public partial record AnimationCurve_1
{
    public required Keyframe_1[] m_Curve;
    public required int m_PreInfinity;
    public required int m_PostInfinity;
    public required int m_RotationOrder;
}

[GenerateSerde]
public partial record AnimationCurve_2
{
    public required Keyframe_2[] m_Curve;
    public required int m_PreInfinity;
    public required int m_PostInfinity;
    public required int m_RotationOrder;
}

[GenerateSerde]
public partial record AnimationCurve_3
{
    public required Keyframe_3[] m_Curve;
    public required int m_PreInfinity;
    public required int m_PostInfinity;
    public required int m_RotationOrder;
}

[GenerateSerde]
public partial record CompressedAnimationCurve
{
    public required string m_Path;
    public required PackedBitVector_1 m_Times;
    public required PackedBitVector_2 m_Values;
    public required PackedBitVector_3 m_Slopes;
    public required int m_PreInfinity;
    public required int m_PostInfinity;
}

[GenerateSerde]
public partial record PPtrCurve
{
    public required PPtrKeyframe[] curve;
    public required string attribute;
    public required string path;
    public required uint classID;
    public required PPtr<MonoScript> script;
    public required int flags;
}

[GenerateSerde]
public partial record xform
{
    public required float3 t;
    public required float4 q;
    public required float3 s;
}

[GenerateSerde]
public partial record Clip
{
    public required StreamedClip m_StreamedClip;
    public required DenseClip m_DenseClip;
    public required ConstantClip m_ConstantClip;
}

[GenerateSerde]
public partial record AnimationClipBindingConstant
{
    public required GenericBinding[] genericBindings;
    public required PPtr<UnityObject>[] pptrCurveMapping;
}

[GenerateSerde]
public partial record QuaternionCurve
{
    public required AnimationCurve_1 curve;
    public required string path;
}

[GenerateSerde]
public partial record Vector3Curve
{
    public required AnimationCurve_2 curve;
    public required string path;
}

[GenerateSerde]
public partial record FloatCurve
{
    public required AnimationCurve_3 curve;
    public required string attribute;
    public required string path;
    public required uint classID;
    public required PPtr<MonoScript> script;
    public required int flags;
}

[GenerateSerde]
public partial record HumanGoal
{
    public required xform m_X;
    public required float m_WeightT;
    public required float m_WeightR;
    public required float3 m_HintT;
    public required float m_HintWeightT;
}

[GenerateSerde]
public partial record HandPose
{
    public required xform m_GrabX;
    public required float[] m_DoFArray;
    public required float m_Override;
    public required float m_CloseOpen;
    public required float m_InOut;
    public required float m_Grab;
}

[GenerateSerde]
public partial record OffsetPtr
{
    public required Clip data;
}

[GenerateSerde]
public partial record HumanPose
{
    public required xform m_RootX;
    public required float3 m_LookAtPosition;
    public required float4 m_LookAtWeight;
    public required HumanGoal[] m_GoalArray;
    public required HandPose m_LeftHandPose;
    public required HandPose m_RightHandPose;
    public required float[] m_DoFArray;
    public required float3[] m_TDoFArray;
}

[GenerateSerde]
public partial record ClipMuscleConstant
{
    public required HumanPose m_DeltaPose;
    public required xform m_StartX;
    public required xform m_StopX;
    public required xform m_LeftFootStartX;
    public required xform m_RightFootStartX;
    public required float3 m_AverageSpeed;
    public required OffsetPtr m_Clip;
    public required float m_StartTime;
    public required float m_StopTime;
    public required float m_OrientationOffsetY;
    public required float m_Level;
    public required float m_CycleOffset;
    public required float m_AverageAngularSpeed;
    public required int[] m_IndexArray;
    public required ValueDelta[] m_ValueArrayDelta;
    public required float[] m_ValueArrayReferencePose;
    public required bool m_Mirror;
    public required bool m_LoopTime;
    public required bool m_LoopBlend;
    public required bool m_LoopBlendOrientation;
    public required bool m_LoopBlendPositionY;
    public required bool m_LoopBlendPositionXZ;
    public required bool m_StartAtOrigin;
    public required bool m_KeepOriginalOrientation;
    public required bool m_KeepOriginalPositionY;
    public required bool m_KeepOriginalPositionXZ;
    public required bool m_HeightFromFeet;
}
```
