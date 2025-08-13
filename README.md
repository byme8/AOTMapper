# AOTMapper

AOTMapper is a high-performance, compile-time object mapping library that eliminates runtime reflection overhead by generating mapping code at build time using Roslyn source generators.

## Features

- **Zero Runtime Reflection**: All mapping code is generated at compile-time
- **High Performance**: Benchmark results show comparable or better performance than manual mapping
- **Compile-Time Safety**: Type mismatches and missing properties are caught at build time
- **Multiple Mapping Patterns**: Support for classic, instance extension, and multi-parameter patterns
- **IDE Integration**: Full IntelliSense support and code analysis
- **Missing Property Detection**: Automatic warnings for unmapped properties with code fixes

## Installation

Install the AOTMapper NuGet package to your project:

```bash
dotnet add package AOTMapper
```

## Quick Start

### 1. Define Your Models

```csharp
public class User 
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

### 2. Create Mapper Methods

AOTMapper supports three different mapping patterns:

#### Classic Pattern
```csharp
public static class UserMappers
{
    [AOTMapperMethod]
    public static UserDto MapToDto(this IAOTMapper mapper, User input)
    {
        var output = new UserDto();
        output.Name = input.Name;
        output.Age = input.Age;
        output.Email = input.Email;
        return output;
    }
}
```

#### Instance Extension Pattern (Single Parameter)
```csharp
public static class UserMappers
{
    [AOTMapperMethod]
    public static UserDto MapToDto(this User input)
    {
        var output = new UserDto();
        output.Name = input.Name;
        output.Age = input.Age;
        output.Email = input.Email;
        return output;
    }
}
```

#### Multi-Parameter Instance Extensions
For scenarios requiring additional parameters (these are not registered with the mapper but work as direct extension methods):

```csharp
public static class UserMappers
{
    [AOTMapperMethod]
    public static UserDto MapToDto(this User input, string defaultEmail)
    {
        var output = new UserDto();
        output.Name = input.Name;
        output.Age = input.Age;
        output.Email = input.Email ?? defaultEmail;
        return output;
    }
}
```

### 3. Build and Use the Mapper

```csharp
// Build the mapper (the extension method is auto-generated)
var mapper = new AOTMapperBuilder()
    .AddYourProjectName() // Auto-generated based on your assembly name
    .Build();

// Use the mapper
var user = new User { Name = "John", Age = 30, Email = "john@example.com" };

// Classic and single-parameter patterns work through the mapper
var dto = mapper.Map<UserDto>(user);

// Multi-parameter extensions work as direct extension methods
var dtoWithDefault = user.MapToDto("default@example.com");

Console.WriteLine(dto.Name); // John
```

## Advanced Features

### Missing Property Detection

AOTMapper automatically detects unmapped properties and provides warnings:

```csharp
[AOTMapperMethod]
public static UserDto MapToDto(this User input)
{
    var output = new UserDto();
    output.Name = input.Name;
    // Warning: Property 'Age' is not mapped
    // Warning: Property 'Email' is not mapped
    return output;
}
```

### Disabling Missing Property Validation

```csharp
[AOTMapperMethod(disableMissingPropertiesDetection: true)]
public static UserDto MapToDto(this User input)
{
    var output = new UserDto();
    output.Name = input.Name;
    // No warnings will be generated
    return output;
}
```

### Code Fixes

AOTMapper provides automatic code fixes for missing properties. When the analyzer detects unmapped properties, you can use the Quick Fix feature in your IDE to automatically add the missing property assignments.

## How It Works

### Source Generation

When you build your project, AOTMapper's source generator:

1. Scans for methods marked with `[AOTMapperMethod]`
2. Validates method signatures and property mappings
3. Generates registration extension methods like `AddYourAssemblyName()`
4. Creates optimized mapping code with zero runtime reflection

### Generated Code Example

For a method like:
```csharp
[AOTMapperMethod]
public static UserDto MapToDto(this User input) { /* ... */ }
```

AOTMapper generates:
```csharp
public static AOTMapperBuilder AddYourProject(this AOTMapperBuilder builder)
{
    builder.AddMapper<User, UserDto>((mapper, input) => UserMappers.MapToDto(input));
    return builder;
}
```

## Pattern Comparison

| Pattern | Registration | Usage | Best For |
|---------|-------------|--------|----------|
| Classic | ✅ Auto-registered | `mapper.Map<T>(input)` | Standard scenarios |
| Instance Extension | ✅ Auto-registered | `mapper.Map<T>(input)` or `input.MapToDto()` | Fluent syntax preference |
| Multi-Parameter | ❌ Not registered | `input.MapToDto(param1, param2)` | Complex scenarios with context |

## Performance

AOTMapper is designed for maximum performance:
- **Zero Reflection**: All mapping logic is generated at compile-time
- **Direct Method Calls**: Generated code uses direct method invocations
- **Minimal Overhead**: Registration and lookup optimized for speed
- **Memory Efficient**: No runtime code generation or dynamic assemblies

## IDE Integration

AOTMapper provides rich IDE support:
- **IntelliSense**: Full autocomplete for generated extension methods
- **Error Squiggles**: Real-time validation of mapping methods
- **Quick Fixes**: Automatic generation of missing property assignments
- **Go to Definition**: Navigate from usage to mapper method definitions