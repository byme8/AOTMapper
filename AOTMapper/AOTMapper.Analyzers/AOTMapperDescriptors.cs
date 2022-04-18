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

    }
}