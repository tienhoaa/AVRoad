param([string]$AppDir = 'C:\AVRoad_2026')
$ErrorActionPreference = 'Stop'
$out = Join-Path $PSScriptRoot '..\diagnostics'
New-Item -ItemType Directory -Force $out | Out-Null
$report = Join-Path $out ('close-crash-' + (Get-Date -Format yyyyMMdd-HHmmss) + '.txt')
"AVRoad close crash diagnostics $(Get-Date -Format s)" | Set-Content $report
foreach ($dir in @($AppDir, (Join-Path $PSScriptRoot '..\AVROAD_PORTABLE'))) {
    "`nInstallation: $dir" | Add-Content $report
    foreach ($file in @('AVRoad.exe','acrender.arx','AlignmentDb17.dbx','TDTObjects17.dbx','version.json','Acad.rx')) {
        $path = Join-Path $dir $file
        if (Test-Path $path) { Get-FileHash $path | Format-List | Out-String | Add-Content $report }
    }
    Get-Content (Join-Path $dir 'acad.err') -Tail 60 -ErrorAction SilentlyContinue | Add-Content $report
}
foreach ($cad in Get-Process acad -ErrorAction SilentlyContinue) {
    "`nRunning CAD PID $($cad.Id)" | Add-Content $report
    try { $cad.Modules | Select-Object FileName,BaseAddress,ModuleMemorySize | Format-Table -AutoSize | Out-String -Width 240 | Add-Content $report }
    catch { $_.Exception.Message | Add-Content $report }
}
Write-Output $report
