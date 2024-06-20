@echo off 
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c %~s0 ::","","runas",1)(window.close)&&exit 
cd /d "%~dp0"

"%CommonProgramFiles(x86)%\ArcGIS\bin\ESRIRegAsm.exe" /u TrueCMYK.dll /p:desktop

