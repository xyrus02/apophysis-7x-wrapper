FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ADD . .
RUN dotnet build src\\apophysis.sln -c Release

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
RUN mkdir C:\\Apophysis
WORKDIR C:\\Apophysis
COPY --from=build target\\Release\\netcoreapp3.1 .
ENTRYPOINT [ "apophysis.exe" ]
