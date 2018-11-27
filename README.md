# Azur-Lane-Autopatcher
Not even finished.

## System Requirements
- [NET Framework 4.6.1 or newer](https://www.microsoft.com/net/download/dotnet-framework-runtime)
- [Python 3.7 or newer](https://www.python.org/downloads/)
  - Make sure to enable `Add to PATH` option during the installation
  
## 3rdparty
This folder contains the 3rd party tool which needed by the autopatcher. Copy the said folder and put it in the same directory alongside the autopatcher binary.

If you are using the old version of autopatcher, move all the contents inside the subdirectories and put it in one same directory.

### Example of directory tree
```
| 3rdparty
|- jit
|--- bc.lua
|--- ...
|- ljd
|--- ast
|--- ...
|- test
|--- breaks.lua
|--- ...
|- .gitignore
|- LICENSE
|- lua51.dll
|- luajit.exe
|- main.py
|- README.md
|- UnityEx.exe
```
