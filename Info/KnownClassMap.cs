using AssetManager.TypeTree;
using System.Collections.Generic;

namespace AssetClassGenerator.Info
{
    public static class KnownClassMap
    {
        public static readonly Dictionary<NodeDataType, string> NodeTypeMap = new()
        {
            [NodeDataType.Pair] = "AssetManager.Collections.MapPair",
            [NodeDataType.Map] = "AssetManager.Collections.MultiDictionary"
        };

        public static readonly Dictionary<string, TypeInfo[]> TypeInfoMap = new()
        {
            ["ColorRGBA"] = new[]
            {
                new TypeInfo(NodeDataType.ColorRGBA4f, "UnityEngine.Color"),
                new TypeInfo(NodeDataType.ColorRGBA32, "UnityEngine.Color32")
            },
            ["Matrix4x4f"] = new[]
            {
                new TypeInfo(NodeDataType.Matrix4x4f, "UnityEngine.Matrix4x4")
            },
            ["Quaternionf"] = new[]
            {
                new TypeInfo(NodeDataType.Pos4f, "UnityEngine.Quaternion")
            },
            ["Vector2f"] = new[]
            {
                new TypeInfo(NodeDataType.Pos2f, "UnityEngine.Vector2")
            },
            ["Vector3f"] = new[]
            {
                new TypeInfo(NodeDataType.Pos3f, "UnityEngine.Vector3")
            },
            ["Vector4f"] = new[]
            {
                new TypeInfo(NodeDataType.Pos4f, "UnityEngine.Vector4")
            },
            ["float3"] = new[]
            {
                new TypeInfo(NodeDataType.Pos3f, "UnityEngine.Vector3")
            },
            ["GUID"] = new[]
            {
                new TypeInfo(NodeDataType.Guid, "AssetsTools.NET.GUID128")
            },
            ["Hash128"] = new[]
            {
                new TypeInfo(NodeDataType.Hash128, "AssetsTools.NET.Hash128")
            }
        };
    }
}
