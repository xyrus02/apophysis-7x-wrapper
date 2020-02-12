# Apophysis 7x Wrapper Library
This repository contains a .NET Standard wrapper library around the native DLL found here: https://github.com/xyrus02/apophysis-7x-dll. Its purpose is to allow a relatively easy way to easily utilize the *unique(tm)* Apophysis interpretation of fractal flames in other projects.

## Contents
There are two .NET projects in this repository:
- aporender: The actual wrapper library which provides a class named `ApophysisNative`
- apophysis: An example CLI rendering application based on .NET Core

The wrapper library depends on two native DLL files named `aporender.x86.dll` and `aporender.x64.dll`. This dependency is the reason why the target platform of the .NET Standard library and .NET Core application is limited to Microsoft Windows.

## Nuget
The library is available on NuGet.org: https://www.nuget.org/packages/XyrusWorx.Apophysis7x/
This allows you to easily add the functionality of the Apophysis 7x rendering engine to your own .NET application using:

  dotnet add package XyrusWorx.Apophysis7x --version 2.15.3
  
Likewise, the CLI tool is provided as a "tool" here: https://www.nuget.org/packages/XyrusWorx.Apophysis7x.Cli/
It's probably not what you want if you have no business with programming and just want to use the command line renderer. In this case, you may check the "Releases" section here on Github for a binary build or just compile the source code yourself.

## How to build
Generally, you don't need to build Delphi code as the native DLLs are included as binary files in this repository. So it is enough to build the solution in `src` using your favorite IDE or build tool. It should work perfectly fine with:

  `dotnet build apophysis.sln`
  
given you have the latest and proper .NET Core SDK (at least 3.1) installed on your computer. NuGet metadata is included in both projects, so the `dotnet pack` command will create NuGet packages for you, if you need a snapshot build to integrate 

## Update the native library
If you made changes in the native library, you can run the script in `build/UpdateNativeLibs.ps1` to call the build script in the Git submodule `native` and copy the freshly built DLLs into the `lib` folder. If the submodule isn't initialized yet, the script will do that for you.
