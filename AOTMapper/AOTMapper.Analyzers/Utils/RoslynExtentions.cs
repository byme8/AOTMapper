using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AOTMapper.Utils
{
    public static class RoslynExtentions
    {
        public static SyntaxTree ToSyntaxTree(this string code)
        {
            return SF.ParseSyntaxTree(code);
        }

        public static string Join(this IEnumerable<string> code, string separator = "")
        {
            return string.Join(separator, code);
        }

        public static BlockSyntax ToBlock(this string code)
        {
            return SF.Block().AddStatements(code.ToStatments());
        }

        public static StatementSyntax[] ToStatments(this string code)
        {
            return code
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => SF.ParseStatement(o))
                .ToArray();
        }

        public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
        {
            if (symbol.BaseType != null)
            {
                foreach (var member in symbol.BaseType.GetAllMembers())
                {
                    yield return member;
                }
            }

            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }
        }

        public static IEnumerable<IPropertySymbol> GetAllPublicProperties(this ITypeSymbol symbol)
        {
            return symbol
                .GetAllMembers()
                .Where(o => o.Kind == SymbolKind.Property && o.DeclaredAccessibility.HasFlag(Accessibility.Public))
                .OfType<IPropertySymbol>();
        }

        public static SourceText ToSourceText(this string source)
        {
            return SourceText.From(source, Encoding.UTF8);
        }

        public static string ToGlobalName(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string ToSafeGlobalName(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "").Replace(".", "");
        }

        public static string Wrap(this string text, string left = "", string right = "")
        {
            return $"{left}{text}{right}";
        }

        public static string JoinWithNewLine(this IEnumerable<string> values, string separator = "")
        {
            return string.Join($"{separator}{Environment.NewLine}", values);
        }

        public static AttributeSyntax GetAOTMapperMethodAttribute(this MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return methodDeclarationSyntax.AttributeLists
                .SelectMany(o => o.Attributes)
                .FirstOrDefault(o => 
                    o.Name.ToString().EndsWith("AOTMapperMethod") ||
                    o.Name.ToString().EndsWith("AOTMapperMethodAttribute"));
        }
    }
}