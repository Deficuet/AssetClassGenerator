using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AssetClassGenerator.Roslyn
{
    public static class RoslynUtils
    {
        public static TypeDeclarationSyntax InsertStructKeyword(this TypeDeclarationSyntax syntax)
        {
            if (syntax is not RecordDeclarationSyntax recordSyntax)
            {
                return syntax;
            }
            return recordSyntax.WithClassOrStructKeyword(
                SyntaxFactory.Token(SyntaxKind.StructKeyword));
        }
    }
}
