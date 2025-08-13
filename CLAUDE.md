# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Test
- **Build solution**: `dotnet build AOTMapper.sln` 
- **Build specific projects**: `dotnet build AOTMapper.Core/AOTMapper.Core.csproj` or `dotnet build AOTMapper/AOTMapper/AOTMapper.csproj`
- **Run tests**: `dotnet test AOTMapper.Tests/AOTMapper.Tests.csproj`
- **Run benchmarks**: `dotnet run --project AOTMapper.Benchmark/AOTMapper.Benchmark.csproj`

### Package Management
- **Build packages**: `./build.ps1` (PowerShell script that builds and packs both Core and main projects)
- **Publish packages**: `./build.ps1 --publish` (requires nugetToken file)

## Architecture Overview

AOTMapper is a compile-time object mapper library that uses Roslyn source generators and analyzers to generate mapping code at build time, eliminating runtime reflection overhead.

### Core Components

#### AOTMapper.Core (Runtime Library)
- **AOTMapper/IAOTMapper**: Main mapper interface and implementation with type-safe mapping methods
- **AOTMapperBuilder**: Fluent builder for configuring mapper instances with descriptor registration
- **AOTMapperDescriptor**: Type descriptors that define source/target types and mapping functions
- **AOTMapperMethodAttribute**: Attribute to mark methods as mapper methods for source generation

#### AOTMapper (Analyzer/Generator Package)
- **AOTMapperModuleInitializerSourceGenerator**: Generates extension methods like `Add{AssemblyName}()` that register all mapper methods found in an assembly
- **MissingPropertiesAnalyzer**: Roslyn analyzer that detects unmapped properties in mapper methods
- **AddMissingPropertiesCodeFix**: Code fix provider that automatically adds missing property mappings
- **AOTMapperGenerator**: Utility for programmatically generating mapper code (used by tools)

### Key Design Patterns

1. **Source Generation Workflow**: 
   - Methods marked with `[AOTMapperMethod]` are discovered by `AOTMapperModuleInitializerSourceGenerator`
   - Generator creates `Add{AssemblyName}()` extension methods that register mappers with the builder
   - Users call `new AOTMapperBuilder().Add{AssemblyName}().Build()` to get a configured mapper

2. **Mapper Method Contract** (supports two patterns):
   - **Classic Pattern**: Static extension methods on `IAOTMapper` with signature `(IAOTMapper mapper, TSource input) -> TTarget`
   - **Instance Extension Pattern**: Static extension methods on source type with signature `(this TSource input) -> TTarget`
   - Must return the target type
   - Should create `output` variable and assign properties, then `return output`

3. **Analyzer Integration**:
   - `MissingPropertiesAnalyzer` validates that all public properties of the target type are assigned
   - Can be disabled per method with `[AOTMapperMethod(disableMissingPropertiesDetection: true)]`
   - Code fixes automatically generate missing property assignments

### Project Structure
- **Library/**: Contains the runtime components (AOTMapper.Core, AOTMapper analyzer package)
- **Tests/**: Contains unit tests, benchmarks, and test projects
- Test projects use xUnit with FluentAssertions and test the source generators using Buildalyzer

### Package Dependencies
- Main package targets .NET Standard 2.0 for broad compatibility
- Uses Microsoft.CodeAnalysis 4.1.0 for Roslyn integration
- Test projects target .NET 6.0 and include BenchmarkDotNet for performance testing