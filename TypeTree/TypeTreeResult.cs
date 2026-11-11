using AssetsTools.NET;

namespace AssetClassGenerator.TypeTree
{
    public sealed class TypeTreeResult(
        TypeTreeType definition,
        byte[] commonStringBufferBytes = null)
    {
        public TypeTreeType Definition { get; } = definition
                ?? throw new ArgumentNullException(nameof(definition));

        public List<TypeTreeNode> Nodes => Definition.Nodes;

        public byte[] StringBufferBytes => Definition.StringBufferBytes;

        public byte[] CommonStringBufferBytes { get; } =
                commonStringBufferBytes ?? [];

        public string GetNameString(TypeTreeNode node)
        {
            return node.GetNameString(
                StringBufferBytes,
                CommonStringBufferBytes
            );
        }

        public string GetTypeString(TypeTreeNode node)
        {
            return node.GetTypeString(
                StringBufferBytes,
                CommonStringBufferBytes
            );
        }
    }
}
