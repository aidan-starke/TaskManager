.PHONY: api cli test build clean format

api:
	dotnet run --project TaskManager.API/TaskManager.API.csproj --launch-profile http

cli:
	dotnet run --project TaskManager.CLI/TaskManager.CLI.csproj

test:
	dotnet test

build:
	dotnet build

clean:
	dotnet clean

format:
	csharpier format ./
