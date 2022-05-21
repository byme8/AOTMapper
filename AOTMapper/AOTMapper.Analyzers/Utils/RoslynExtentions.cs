using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static string JoinWithNewLine(this IEnumerable<string> code)
        {
            return string.Join(Environment.NewLine, code);
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

        public static TValue Cast<TValue>(this object value)
        {
            return (TValue)value;
        }

        public static TValue As<TValue>(this object value)
            where TValue : class
        {
            return value as TValue;
        }
    }
}
