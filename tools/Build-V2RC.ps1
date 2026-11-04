param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'release-v2'
$stage=Join-Path $out 'stage'
$app='C:\AVRoad_2026'
$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
if(Test-Path (Join-Path $out 'build-started.txt')){throw 'One-pass build has already started. Do not rebuild without reviewing evidence.'}
Set-Content (Join-Path $out 'build-started.txt') (Get-Date -Format o)
Push-Location $root
try{
 & $csc /nologo /target:library /platform:x86 /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /out:release-v2\stage\AVRoad.Deployment.dll Cad7ComRegistration.cs PatchTransaction.cs
 if($LASTEXITCODE){throw 'Deployment library build failed'}
 & $csc /nologo /target:winexe /platform:x86 /win32icon:AVRoad.ico /r:release-v2\stage\AVRoad.Deployment.dll /out:release-v2\stage\AVRoad.exe AVRoad_Launcher.cs CADRegistryConfig.cs UpdateToastForm.cs
 if($LASTEXITCODE){throw 'Launcher build failed'}
 & $csc /nologo /target:winexe /platform:x86 /win32icon:AVRoad.ico /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /r:release-v2\stage\AVRoad.Deployment.dll /out:release-v2\stage\AVUpdater.v2.exe AVUpdater.cs
 if($LASTEXITCODE){throw 'Updater build failed'}
 Copy-Item (Join-Path $stage 'AVUpdater.v2.exe') (Join-Path $stage 'AVUpdater.exe')
 Add-Type -AssemblyName System.IO.Compression.FileSystem
 $patch=Join-Path $out 'patch_2.0_rc1.zip'
 $stream=[IO.File]::Open($patch,[IO.FileMode]::CreateNew)
 $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
 try{foreach($name in @('AVRoad.exe','AVUpdater.v2.exe','AVRoad.Deployment.dll','version.json')){
  [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $stage $name),$name,[IO.Compression.CompressionLevel]::Optimal)|Out-Null
 }}finally{$zip.Dispose();$stream.Dispose()}
 $manifest=Get-Content -Raw (Join-Path $stage 'version.json')|ConvertFrom-Json
 $manifest|Add-Member -NotePropertyName patch_sha256 -NotePropertyValue ((Get-FileHash $patch).Hash.ToLowerInvariant())
 $manifest|ConvertTo-Json -Depth 6|Set-Content -Encoding utf8 (Join-Path $root 'version-2.0-rc.json')
 # Copy the inspected runtime byte-for-byte. Never patch Autodesk binaries.
 $excluded='(^|\\)(\.avroad-rollback|\.git|diagnostics|locked|backup[^\\]*)(\\|$)'
 $copied=0
 foreach($file in Get-ChildItem -LiteralPath $app -File -Recurse){
  $rel=$file.FullName.Substring($app.Length+1)
  if($rel -match $excluded -or $file.Extension -in @('.cs','.py','.ps1','.scr','.log','.err','.dmp','.bak','.tmp','.zip') -or $file.Name -match '^(Setup.*\.exe|Drawing.*\.(dwg|dwl|dwl2|sv\$)|acad_backup.*|avroad\.old\.exe)$'){continue}
  if($rel -in @('AVRoad.exe','AVUpdater.exe','AVUpdater.v2.exe','AVRoad.Deployment.dll','version.json')){continue}
  $dest=Join-Path $stage $rel
  New-Item -ItemType Directory -Force -Path (Split-Path $dest -Parent)|Out-Null
  Copy-Item -LiteralPath $file.FullName -Destination $dest
  $copied++
 }
 [IO.Compression.ZipFile]::CreateFromDirectory($stage,(Join-Path $out 'payload.zip'),[IO.Compression.CompressionLevel]::Optimal,$false)
 & $csc /nologo /target:winexe /platform:x86 /win32icon:AVRoad.ico /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /resource:release-v2\payload.zip,payload.zip /out:release-v2\Setup_AVRoad_2.0.exe Setup_AVRoad_2026.cs CADRegistryConfig.cs Cad7ComRegistration.cs PatchTransaction.cs
 if($LASTEXITCODE){throw 'Installer build failed'}
 $outputs=@('Setup_AVRoad_2.0.exe','patch_2.0_rc1.zip')
 $lines=@(foreach($file in $outputs){$h=Get-FileHash (Join-Path $out $file);$h.Hash.ToLowerInvariant()+'  '+$file})
 $lines|Set-Content -Encoding ascii (Join-Path $out 'SHA256SUMS.txt')
 [pscustomobject]@{BuildPasses=1;RuntimeFilesCopied=$copied;Version=$manifest.version;Prerelease=$true;Built=(Get-Date -Format o)}|ConvertTo-Json|Set-Content -Encoding utf8 (Join-Path $out 'build-result.json')
 Get-Item ($outputs|ForEach-Object{Join-Path $out $_})|Select-Object Name,Length
}finally{Pop-Location}
