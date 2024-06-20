@echo off

setlocal EnableDelayedExpansion

set batPath=%~dp0
cd /d %batPath%

for /f "delims=" %%i in ('dir /x /b *.?tf') do (
set "zt=%%~si"
mshta "javascript:new ActiveXObject('Shell.Application').NameSpace(20).CopyHere('!zt:\=\\!',0x0010);close()"
@echo 已安装字体：%%i
)
pause