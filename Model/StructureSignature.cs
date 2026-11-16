using System.Collections.Generic;
using System.Text;

namespace AssetClassGenerator.Model;

internal static class StructureSignature
{
    public static string Build(string name, IEnumerable<MemberModel> members)
    {
        var builder = new StringBuilder();
        AppendString(builder, name);
        builder.Append('{');
        foreach (MemberModel member in members)
        {
            AppendString(builder, member.OriginalName);
            builder.Append(':');
            AppendString(builder, member.Type.Signature);
            builder.Append(';');
        }
        builder.Append('}');
        return builder.ToString();
    }

    private static void AppendString(StringBuilder builder, string value)
    {
        builder.Append(value.Length);
        builder.Append(':');
        builder.Append(value);
    }
}
