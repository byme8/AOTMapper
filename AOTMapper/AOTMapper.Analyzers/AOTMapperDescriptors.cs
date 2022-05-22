using Microsoft.CodeAnalysis;

namespace AOTMapper
{
    public static class AOTMapperDescriptors
    {
        public static DiagnosticDescriptor NotAllOutputValuesAreMapped = new DiagnosticDescriptor(
            nameof(NotAllOutputValuesAreMapped),
            "Not all output properties are mapped",
            "Next properties are not mapped: {0}",
            "AOTMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "You need to fill all properties before return the value.");
        
        public static DiagnosticDescriptor ReturnOfOutputIsMissing = new DiagnosticDescriptor(
            nameof(ReturnOfOutputIsMissing),
            "Return the output variable",
            "The return statement has to return variable with name 'output'",
            "AOTMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "The return statement has to return variable with name 'output'.");
        
        public static DiagnosticDescriptor AOTMapperMethodWrongDeclaration = new DiagnosticDescriptor(
            nameof(AOTMapperMethodWrongDeclaration),
            "The AOTMapper method has wrong declaration",
            "The AOTMapper method have to accept two parameters: IAOTMapper mapper and source object, and return the mapped object",
            "AOTMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            "The AOTMapper method have to accept two parameters: IAOTMapper mapper and source object, and return the mapped object.");
        

    }
}