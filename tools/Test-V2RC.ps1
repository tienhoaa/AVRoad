$ErrorActionPreference='Stop'
$base=Split-Path $PSScriptRoot -Parent
$out=Join-Path $base 'release-v2'
$work=Join-Path $out 'tests'
New-Item -ItemType Directory -Force $work|Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$updater=Join-Path $out 'stage\AVUpdater.v2.exe'
$patch=Join-Path $out 'patch_2.0_rc1.zip'
$sha=(Get-FileHash $patch).Hash.ToLowerInvariant()
$results=[Collections.Generic.List[object]]::new()
function Run-Updater($arguments){
 $p=Start-Process -FilePath $updater -ArgumentList $arguments -Wait -PassThru
 return $p.ExitCode
}
function Old-Install($name){
 $dir=Join-Path $work $name;New-Item -ItemType Directory -Path $dir|Out-Null
 Copy-Item 'C:\AVRoad_2026\AVRoad.exe' (Join-Path $dir 'AVRoad.exe')
 Copy-Item 'C:\AVRoad_2026\AVUpdater.exe' (Join-Path $dir 'AVUpdater.exe')
 Copy-Item (Join-Path $base 'version.json') (Join-Path $dir 'version.json')
 return $dir
}
$dir=Old-Install 'upgrade'
$original=(Get-FileHash (Join-Path $dir 'AVRoad.exe')).Hash
$oldVersion=Get-Content -Raw (Join-Path $dir 'version.json')
$result=Join-Path $work 'upgrade-result.txt'
$code=Run-Updater @('/test-apply',$patch,$dir,$sha,$result)
if($code -ne 0){throw ('Upgrade test failed: '+(Get-Content -Raw $result))}
foreach($name in @('AVRoad.exe','AVUpdater.v2.exe','AVRoad.Deployment.dll','version.json')){
 if((Get-FileHash (Join-Path $dir $name)).Hash -ne (Get-FileHash (Join-Path $out ('stage\'+$name))).Hash){throw "Upgrade file mismatch: $name"}
}
$results.Add(@{Test='v1.9 file-copy upgrade through compiled updater transaction';Pass=$true})
$journal=(Get-Content -Raw $result).Trim()
$code=Run-Updater @('/rollback',$journal,(Join-Path $work 'rollback-result.txt'))
if($code -ne 0 -or (Get-FileHash (Join-Path $dir 'AVRoad.exe')).Hash -ne $original -or (Get-Content -Raw (Join-Path $dir 'version.json')) -ne $oldVersion -or (Test-Path (Join-Path $dir 'AVUpdater.v2.exe'))){throw 'Explicit rollback failed'}
$results.Add(@{Test='Explicit file rollback restores v1.9 and removes newly created files';Pass=$true})
$bad=Join-Path $work 'tampered.zip';Copy-Item $patch $bad
$f=[IO.File]::Open($bad,[IO.FileMode]::Append);try{$f.WriteByte(1)}finally{$f.Dispose()}
$code=Run-Updater @('/test-apply',$bad,$dir,$sha,(Join-Path $work 'checksum-result.txt'))
if($code -eq 0 -or (Get-FileHash (Join-Path $dir 'AVRoad.exe')).Hash -ne $original){throw 'Checksum rejection failed'}
$results.Add(@{Test='Corrupted ZIP rejected by SHA256 without upgrade';Pass=$true})
$unsafe=Join-Path $work 'traversal.zip';$s=[IO.File]::Create($unsafe);$z=[IO.Compression.ZipArchive]::new($s,[IO.Compression.ZipArchiveMode]::Create)
try{foreach($name in @('..\escape.txt','version.json')){$entry=$z.CreateEntry($name);$writer=[IO.StreamWriter]::new($entry.Open());try{$writer.Write('{}')}finally{$writer.Dispose()}}}finally{$z.Dispose();$s.Dispose()}
$code=Run-Updater @('/test-apply',$unsafe,$dir,(Get-FileHash $unsafe).Hash,(Join-Path $work 'traversal-result.txt'))
if($code -eq 0 -or (Test-Path (Join-Path $work 'escape.txt'))){throw 'Traversal rejection failed'}
$results.Add(@{Test='Archive traversal rejected before writes';Pass=$true})
$locked=Old-Install 'locked'
$lockPath=Join-Path $locked 'AVUpdater.v2.exe';[IO.File]::WriteAllText($lockPath,'old updater sentinel')
$handle=[IO.File]::Open($lockPath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
try{$code=Run-Updater @('/test-apply',$patch,$locked,$sha,(Join-Path $work 'locked-result.txt'))}finally{$handle.Dispose()}
if($code -eq 0 -or (Get-FileHash (Join-Path $locked 'AVRoad.exe')).Hash -ne $original -or (Get-Content -Raw (Join-Path $locked 'version.json')) -ne $oldVersion){throw 'Locked file rollback failed'}
$results.Add(@{Test='Mid-update locked file restores already overwritten launcher; version remains v1.9';Pass=$true})
$installer=Join-Path $out 'Setup_AVRoad_2.0.exe'
$extract=Join-Path $work 'installer-extracted'
$p=Start-Process -FilePath $installer -ArgumentList @('/extract-only',$extract) -Wait -PassThru
if($p.ExitCode -ne 0 -or !(Test-Path (Join-Path $extract 'verified-installer.txt'))){throw 'Embedded installer extraction failed'}
foreach($name in @('AVRoad.exe','AVUpdater.exe','AVUpdater.v2.exe','AVRoad.Deployment.dll','version.json','axdb.dll','acad.exe')){
 if((Get-FileHash (Join-Path $extract $name)).Hash -ne (Get-FileHash (Join-Path $out ('stage\'+$name))).Hash){throw "Embedded payload file mismatch: $name"}
}
$results.Add(@{Test='Standalone embedded Setup extracts expected files without invoking Registry configuration';Pass=$true})
$results|ConvertTo-Json -Depth 5|Set-Content -Encoding utf8 (Join-Path $out 'evidence\deployment-tests.json')
$results|ForEach-Object{'PASS: '+$_.Test}
