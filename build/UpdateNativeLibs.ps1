
Push-Location "$PSScriptRoot\.."

if (-not (Test-Path -PathType Leaf -Path ".\native\build.cmd")) {
	git submodule update --init --recursive
}

Push-Location "native"
&cmd /c "build.cmd"
Pop-Location

if (Test-Path -PathType Leaf -Path ".\native\target\x86\aporender.dll") {
	Copy-Item -Force ".\native\target\x86\aporender.dll" "lib\aporender.x86.dll"
}

if (Test-Path -PathType Leaf -Path ".\native\target\x64\aporender.dll") {
	Copy-Item -Force ".\native\target\x64\aporender.dll" "lib\aporender.x64.dll"
}

Pop-Location