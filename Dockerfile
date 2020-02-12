FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN mkdir C:\\Apophysis
WORKDIR C:\\Apophysis
COPY target\\Release\\netcoreapp3.1 .

ENTRYPOINT [ "apophysis.exe" ]
