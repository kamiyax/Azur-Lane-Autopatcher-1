# Azur-Lane-Autopatcher
Not even finished.

## System Requirements
- [NET Framework 4.6.1 or newer](https://www.microsoft.com/net/download/dotnet-framework-runtime)
- [Python 3.7 or newer](https://www.python.org/downloads/)
  - Make sure to enable `Add to PATH` option during the installation
  
## 3rdparty
This folder contains 3rd party tools which needed by the autopatcher. Put the said folder alongside autopatcher

If you are using the old version of autopatcher, move all contents within sub-directories to the root directory (3rdparty).

### Example of directory tree for old version of autopatcher
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