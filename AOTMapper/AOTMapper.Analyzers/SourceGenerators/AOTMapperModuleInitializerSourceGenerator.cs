using System;
using System.Collections.Generic;
using System.Linq;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AOTMapper.SourceGenerators
{
    [Generator]
    public class AOTMapperModuleInitializerSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AOTMapperModuleInitializerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AOTMapperModuleInitializerSyntaxReceiver syntaxReceiver))
            {
                return;
            }

            var compilation = context.Compilation;
            var verifiedMethods = new List<IMethodSymbol>();
            foreach (var methodSyntax in syntaxReceiver.Methods)
            {
                var symbol = compilation
                    .GetSemanticModel(methodSyntax.SyntaxTree)
                    .GetDeclaredSymbol(methodSyntax);

                if (!(symbol is IMethodSymbol methodSymbol))
                {
                    continue;
                }

                var aotMapperAttribute = methodSymbol
                    .GetAttributes()
                    .FirstOrDefault(o => o.AttributeClass
                        .ToGlobalName().EndsWith("AOTMapper.Core.AOTMapperMethodAttribute"));

                if (aotMapperAttribute is null || methodSymbol.Parameters.Length != 2)
                {
                    continue;
                }

                verifiedMethods.Add(methodSymbol);
            }

            var assemblyName = compilation.Assembly.Identity.Name.Replace(".", "");
            var source =
                $@"
using System.Runtime.CompilerServices;
using AOTMapper.Core;

namespace AOTMapper.Core
{{
    public static class AOTMapperFor{assemblyName}
    {{
        private static IAOTMapper mapper;

        public static IAOTMapper Mapper 
        {{ 
            get
            {{
                if(mapper is null)
                {{
                    var builder = new AOTMapperBuilder();
                    builder.Add{assemblyName}();
                    return builder.Build();
                }}

                return mapper;
            }}
        }}

        public static AOTMapperBuilder Add{assemblyName}(this AOTMapperBuilder builder)
        {{
            {verifiedMethods
                .Select(o => $"builder.AddMapper<{o.Parameters[1].Type.ToGlobalName()}, {o.ReturnType.ToGlobalName()}>({o.ContainingSymbol.ToGlobalName()}.{o.ToGlobalName()});")
                .JoinWithNewLine()}

            return builder;
        }}
    }}
}}
";
            context.AddSource($"AOTMapperModuleInitializerFor{assemblyName}", source);
        }
    }

    public class AOTMapperModuleInitializerSyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> Methods = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                if (methodDeclarationSyntax.CanBeAOTMapperMethod())
                {
                    Methods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}