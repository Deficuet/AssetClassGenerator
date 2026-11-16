using AssetManager.TypeTree;
using System;

namespace AssetClassGenerator.Info
{
    public sealed class TypeInfo
    {
        public NodeDataType ExpectedType { get; }

        public string FullQualifiedName { get; }

        public TypeInfo(NodeDataType expectedType, string fullQualifiedName)
        {
            ExpectedType = expectedType;
            FullQualifiedName = fullQualifiedName
                ?? throw new ArgumentNullException(nameof(fullQualifiedName));
        }
    }
}
