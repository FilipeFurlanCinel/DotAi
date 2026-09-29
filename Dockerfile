FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DotAi.sln ./
COPY Domain/Domain.csproj Domain/
COPY Usecases/Usecases.csproj Usecases/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY CrossCutting/CrossCutting.csproj CrossCutting/
COPY Presentation/Presentation.csproj Presentation/

RUN dotnet restore Presentation/Presentation.csproj

COPY Domain/ Domain/
COPY Usecases/ Usecases/
COPY Infrastructure/ Infrastructure/
COPY CrossCutting/ CrossCutting/
COPY Presentation/ Presentation/

RUN dotnet publish Presentation/Presentation.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends git ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

WORKDIR /workspace
# Secondary runtime path — prefer installing the local `dot-ai` tool (see Readme.md).
ENTRYPOINT ["dotnet", "/app/dot-ai.dll"]
