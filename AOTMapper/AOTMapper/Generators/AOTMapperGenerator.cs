using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AOTMapper.Utils;
using Microsoft.CodeAnalysis;

namespace AOTMapper.Core.Generators
{
    public class AOTMapperGenerator
    {
        public AOTMapperGenerator(Project project, Compilation compilation)
        {
            this.Project = project;
            this.Compilation = compilation;
        }

        public Project Project { get; set; }
        public Compilation Compilation { get; set; }

        public void GenerateMappers(ImmutableDictionary<string, string> values, string[] outputNamespace)
        {
            foreach (var value in values)
            {
                var fromSymbol = this.Compilation.GetTypeByMetadataName(value.Key);
                var toSymbol = this.Compilation.GetTypeByMetadataName(value.Value);

                var source = this.GenerateMapper(fromSymbol, toSymbol);

                var fromSymbolName = fromSymbol.ToDisplayString().Replace(".", "");
                var toSymbolName = toSymbol.ToDisplayString().Replace(".", "");
                this.Project = this.Project
                    .AddDocument($"{fromSymbolName}_To_{toSymbolName}.cs", source)
                    .WithFolders(outputNamespace)
                    .Project;
            }
        }

        private string GenerateMapper(INamedTypeSymbol fromSymbol, INamedTypeSymbol toSymbol)
        {
            var fromProperties = fromSymbol.GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(o => (o.DeclaredAccessibility & Accessibility.Public) > 0)
                .ToDictionary(o => o.Name, o => o.Type);

            var toProperties = toSymbol.GetAllMembers()
                .OfType<IPropertySymbol>()
                .Where(o => (o.DeclaredAccessibility & Accessibility.Public) > 0)
                .ToDictionary(o => o.Name, o => o.Type);

            return $@"
public static class {fromSymbol.ToDisplayString().Replace(".", "")}Extentions 
{{
    public static {toSymbol.ToDisplayString()} MapTo{toSymbol.ToDisplayString().Split('.').Last()}(this {fromSymbol.ToDisplayString()} input)
    {{
        var output = new {toSymbol.ToDisplayString()}();
{ toProperties
    .Where(o => fromProperties.TryGetValue(o.Key, out var type) && type == o.Value)
    .Select(o => $"        output.{o.Key} = input.{o.Key};" )
    .JoinWithNewLine()
}
{ toProperties
    .Where(o => !fromProperties.TryGetValue(o.Key, out var type) || type != o.Value)
    .Select(o => $"        output.{o.Key} = ; // missing property")
    .JoinWithNewLine()
}
        return output;
    }}
}}
";
        }
    }
}
