dotnet restore -r=linux-arm64
dotnet msbuild /t:CreateDeb /p:RuntimeIdentifier=linux-arm64 /p:Configuration=Release

dotnet restore -r=linux-x64
dotnet msbuild /t:CreateDeb /p:RuntimeIdentifier=linux-x64 /p:Configuration=Release