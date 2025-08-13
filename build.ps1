using namespace System.IO;

rm -r ./nugets
dotnet build -c release ./AOTMapper/AOTMapper/AOTMapper.csproj
dotnet build -c release ./AOTMapper.Core/AOTMapper.Core.csproj
dotnet pack  -c release -o ./nugets ./AOTMapper/AOTMapper/AOTMapper.csproj
dotnet pack  -c release -o ./nugets ./AOTMapper.Core/AOTMapper.Core.csproj
