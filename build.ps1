using namespace System.IO;
try {
    $nugetToken = [File]::ReadAllText("./nugetToken");
}
catch {
    echo "Nuget token is missing"
    exit
}

rm -r ./nugets
dotnet build -c release ./AOTMapper/AOTMapper/AOTMapper.csproj
dotnet build -c release ./AOTMapper.Core/AOTMapper.Core.csproj
dotnet pack  -c release -o ./../../nugets ./AOTMapper/AOTMapper/AOTMapper.csproj
dotnet pack  -c release -o ./../nugets ./AOTMapper.Core/AOTMapper.Core.csproj

if (($args.Count -gt 0) -and ($args.Contains("--publish"))) {
    $files = [Directory]::GetFiles("./nugets")
    foreach ($file in $files) {
        dotnet nuget push $file -k $nugetToken -s https://api.nuget.org/v3/index.json
    }
}
