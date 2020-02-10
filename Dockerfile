FROM mcr.microsoft.com/dotnet/framework/runtime:4.8

RUN mkdir C:/Apophysis
WORKDIR C:/Apophysis
COPY target/apoc-release .

ENTRYPOINT [ "apophysis.exe" ]
