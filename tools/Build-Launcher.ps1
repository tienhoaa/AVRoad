param()
$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & $csc /nologo /target:winexe /platform:x86 /win32icon:AVRoad.ico /out:AVROAD_PORTABLE\AVRoad.exe AVRoad_Launcher.cs UpdateToastForm.cs
    if ($LASTEXITCODE -ne 0) { throw 'Launcher compilation failed' }
    & $csc /nologo /target:winexe /platform:x86 /win32icon:AVRoad.ico /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /out:AVROAD_PORTABLE\AVUpdater.exe AVUpdater.cs
    if ($LASTEXITCODE -ne 0) { throw 'Updater compilation failed' }
} finally { Pop-Location }
