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

                if (aotMapperAttribute is null)
                {
                    continue;
                }

                var isValidSignature = false;
                
                if (methodSymbol.Parameters.Length == 2 && 
                    methodSymbol.Parameters[0].Type.ToGlobalName() == "global::AOTMapper.IAOTMapper")
                {
                    isValidSignature = true; // Classic pattern
                }
                else if (methodSymbol.Parameters.Length == 1 && methodSymbol.IsExtensionMethod)
                {
                    isValidSignature = true; // Single-parameter instance extension pattern
                }
                else if (methodSymbol.Parameters.Length > 1 && methodSymbol.IsExtensionMethod)
                {
                    isValidSignature = true; // Multi-parameter instance extension pattern
                }

                if (!isValidSignature)
                {
                    continue;
                }

                verifiedMethods.Add(methodSymbol);
            }

            var assemblyName = compilation.Assembly.Identity.Name.Replace(".", "");
            
            var mapperRegistrations = verifiedMethods
                .Where(o => 
                {
                    // Only register methods that should be registered with the mapper
                    if (o.Parameters.Length == 2 && 
                        o.Parameters[0].Type.ToGlobalName() == "global::AOTMapper.IAOTMapper")
                    {
                        return true; // Classic pattern
                    }

                    if (o.Parameters.Length == 1 && o.IsExtensionMethod)
                    {
                        return true; // Single-parameter instance extension pattern
                    }
                    return false; // Multi-parameter instance extensions are not registered
                })
                .Select(o => 
                {
                    var sourceType = o.Parameters.Length == 2 ? o.Parameters[1].Type.ToGlobalName() : o.Parameters[0].Type.ToGlobalName();
                    var targetType = o.ReturnType.ToGlobalName();
                    var methodCall = o.Parameters.Length == 2 
                        ? $"{o.ContainingSymbol.ToGlobalName()}.{o.ToGlobalName()}" 
                        : $"(mapper, input) => {o.ContainingSymbol.ToGlobalName()}.{o.ToGlobalName()}(input)";
                    return $"            builder.AddMapper<{sourceType}, {targetType}>({methodCall});";
                })
                .JoinWithNewLine();
            
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
                    mapper = builder.Build();
                }}

                return mapper;
            }}
        }}

        public static AOTMapperBuilder Add{assemblyName}(this AOTMapperBuilder builder)
        {{
{mapperRegistrations}

            return builder;
        }}
    }}
}}
";
            context.AddSource($"AOTMapperModuleInitializerFor{assemblyName}.g.cs", source);
        }
    }

    public class AOTMapperModuleInitializerSyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> Methods = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                if (methodDeclarationSyntax.GetAOTMapperMethodAttribute() != null)
                {
                    Methods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}