using System.Text;
using System.Text.RegularExpressions;

namespace AssetClassGenerator;

public static partial class IdentifierUtils
{
    // A conservative set of C# keywords and contextual keywords (keeps common ones).
    // If you need absolute correctness for a particular C# version, extend this set.
    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const",
        "continue","decimal","default","delegate","do","double","else","enum","event","explicit",
        "extern","false","finally","fixed","float","for","foreach","goto","if","implicit","in","int",
        "interface","internal","is","lock","long","namespace","new","null","object","operator","out",
        "override","params","private","protected","public","readonly","ref","return","sbyte","sealed",
        "short","sizeof","stackalloc","static","string","struct","switch","this","throw","true","try",
        "typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while",
        // contextual and query keywords
        "add","remove","alias","ascending","async","await","by","descending","dynamic","equals","from",
        "get","global","group","into","join","let","nameof","on","orderby","partial","select","set",
        "value","var","when","where","yield"
    };

    /// <summary>
    /// Tests whether <paramref name="s"/> is a valid C# identifier (not a keyword).
    /// Accepts Unicode letters/digits/underscore per standard identifier rules.
    /// </summary>
    public static bool IsValidIdentifier(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;

        // First character: letter or underscore
        char first = s[0];
        if (!(first == '_' || char.IsLetter(first)))
            return false;

        // Remaining chars: letter, digit or underscore
        for (int i = 1; i < s.Length; ++i)
        {
            char c = s[i];
            if (!(c == '_' || char.IsLetterOrDigit(c)))
                return false;
        }

        // Must not be a keyword
        if (CSharpKeywords.Contains(s))
            return false;

        return true;
    }

    /// <summary>
    /// Convert input string (ASCII letters, digits, spaces, and bracket characters) into a valid camelCase variable name.
    /// Rules applied:
    ///  - Split on spaces and bracket characters: [ ] ( ) { } &lt; &gt;
    ///  - Lowercase tokens, then produce camelCase (first token lower, subsequent tokens capitalized)
    ///  - Remove any non-alphanumeric characters leftover
    ///  - If result starts with digit, prefix with '_'
    ///  - If result is a C# keyword, append '_'
    ///  - If result becomes empty, return "_"
    /// </summary>
    public static string ToCamelCaseIdentifier(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Characters considered separators for tokenization (spaces + common brackets).
        char[] separators = [' ', '\t', '\r', '\n', '[', ']', '(', ')', '{', '}', '<', '>'];

        // Split into tokens removing empty entries.
        var rawTokens = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        // Keep only alphanumeric characters in tokens (defensive).
        var tokens = rawTokens
            .Select(t => new string(t.Where(char.IsLetterOrDigit).ToArray()))
            .Where(t => t.Length > 0)
            .ToArray();

        string result;
        if (tokens.Length == 0)
        {
            result = "_";
        }
        else
        {
            // Lowercase all tokens so camel-casing is consistent.
            for (int i = 0; i < tokens.Length; i++)
                tokens[i] = tokens[i].ToLowerInvariant();

            var sb = new StringBuilder();
            sb.Append(tokens[0]); // first token lowercased

            for (int i = 1; i < tokens.Length; ++i)
            {
                var tk = tokens[i];
                // Capitalize first letter of token, keep rest as-is (all lowercased already).
                sb.Append(char.ToUpperInvariant(tk[0]));
                if (tk.Length > 1) sb.Append(tk.AsSpan(1));
            }

            result = sb.ToString();
            if (string.IsNullOrEmpty(result)) result = "_";
        }

        // If starts with digit, prefix underscore
        if (char.IsDigit(result[0])) result = "_" + result;

        // If it's a keyword, make it non-keyword by appending underscore
        if (CSharpKeywords.Contains(result))
            result += "_";

        // Finally, ensure the result is a valid identifier (defensive).
        if (!IsValidIdentifier(result))
        {
            // As a last resort, prefix '_' until valid (won't normally be necessary)
            while (!IsValidIdentifier(result))
                result = "_" + result;
        }

        return result;
    }

    [GeneratedRegex(@"PPtr<\$(?<clsName>.+)>")]
    private static partial Regex PPtrEscapeRegex();

    public static string EscapeTypeName(string typeName)
    {
        Match m = PPtrEscapeRegex().Match(typeName);
        if (m.Success)
        {
            return $"PPtr<{m.Groups["clsName"].Value}>";
        }
        return typeName;
    }

    private static readonly Dictionary<string, string> s_typeNameRemap = new()
    {
        { "PPtr<Object>", "PPtr<UnityObject>" },
        { "PPtr<Component>", "PPtr<UnityComponent>" },
        { "Matrix4x4f", "Matrix4x4" },
        { "Quaternionf", "Quaternion" }, 
        { "Vector2f", "Vector2" }, 
        { "Vector3f", "Vector3" }, 
        { "Vector4f", "Vector4" },
        { "float3", "Vector3" },
        { "GUID", "Guid" }
    };

    public static string RemapTypeName(string typeName)
    {
        if (s_typeNameRemap.TryGetValue(typeName, out var remapped))
        {
            return remapped;
        }
        return typeName;
    }

    public static string ProcessTypeName(string typeName)
    {
        var tn = EscapeTypeName(typeName);
        tn = RemapTypeName(tn);
        return tn;
    }
}
