echo "new console proj"
dotnet new console -lang "F#" -o $1
cd $1
dotnet add package EventStore.Client
dotnet add package FsUnit.xUnit
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
