using AssetManager.TypeTree;

namespace AssetClassGenerator.Info
{
    public static class KnownClassMap
    {
        public static readonly Dictionary<NodeDataType, string> NodeTypeMap = new()
        {
            [NodeDataType.Pair] = "UObjects.Collections.MapPair",
            [NodeDataType.Map] = "UObjects.Collections.MultiDictionary"
        };

        public static readonly Dictionary<string, string> TypeNameMap = new()
        {
            ["ColorRGBA"] = "UnityEngine.Color",
            ["Matrix4x4f"] = "UnityEngine.Matrix4x4",
            ["Quaternionf"] = "UnityEngine.Quaternion",
            ["Vector2f"] = "UnityEngine.Vector2",
            ["Vector3f"] = "UnityEngine.Vector3",
            ["Vector4f"] = "UnityEngine.Vector4",
            ["float3"] = "UnityEngine.Vector3",
            ["GUID"] = "AssetsTools.NET.GUID128",
            ["Hash128"] = "AssetsTools.NET.Hash128",
        };
    }
}
