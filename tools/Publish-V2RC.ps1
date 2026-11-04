param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'release-v2'
$tests=@(Get-Content -Raw (Join-Path $out 'evidence\deployment-tests.json')|ConvertFrom-Json)
if($tests.Count -ne 6 -or @($tests|Where-Object {!$_.Pass}).Count){throw 'Deployment verification incomplete'}
if(!(Test-Path (Join-Path $out 'evidence\com-helper-test.txt'))){throw 'Compiled COM helper verification missing'}
$manifest=Get-Content -Raw (Join-Path $root 'version-2.0-rc.json')|ConvertFrom-Json
if(!$manifest.prerelease -or $manifest.patch_sha256 -ne (Get-FileHash (Join-Path $out 'patch_2.0_rc1.zip')).Hash.ToLowerInvariant()){throw 'RC manifest mismatch'}
foreach($line in Get-Content (Join-Path $out 'SHA256SUMS.txt')){
 $parts=$line -split '  ',2
 if((Get-FileHash (Join-Path $out $parts[1])).Hash.ToLowerInvariant() -ne $parts[0]){throw 'Artifact checksum mismatch'}
}
# Existing Git Credential Manager credential; never print or persist the secret.
$pi=[Diagnostics.ProcessStartInfo]::new('git','-C C:\TDT_RESEARCH credential fill')
$pi.UseShellExecute=$false;$pi.CreateNoWindow=$true
$pi.RedirectStandardInput=$true;$pi.RedirectStandardOutput=$true;$pi.RedirectStandardError=$true
$pi.EnvironmentVariables['GCM_INTERACTIVE']='Never'
$cp=[Diagnostics.Process]::Start($pi)
$cp.StandardInput.Write("protocol=https`nhost=github.com`n`n");$cp.StandardInput.Close()
$answer=$cp.StandardOutput.ReadToEnd();$cp.StandardError.ReadToEnd()|Out-Null;$cp.WaitForExit()
if($cp.ExitCode){throw 'No cached GitHub credential'}
$secretLine=$answer -split "`n"|Where-Object {$_.StartsWith('password=')}|Select-Object -First 1
if(!$secretLine){throw 'Missing cached GitHub token'}
$headers=@{Authorization=('Bearer '+$secretLine.Substring(9).Trim());Accept='application/vnd.github+json';'User-Agent'='AVRoad-Release'}
$api='https://api.github.com/repos/tienhoaa/AVRoad'
$repo=Invoke-RestMethod $api -Headers $headers
if($repo.full_name -ne 'tienhoaa/AVRoad' -or !$repo.permissions.push){throw 'Unexpected repository or missing push permission'}
$existing=@(Invoke-RestMethod ($api+'/releases?per_page=100') -Headers $headers)
if(@($existing|Where-Object {$_.tag_name -eq 'v2.0-rc.1'}).Count){throw 'RC release already exists; do not duplicate or replace assets'}
$desktop=[Environment]::GetFolderPath([Environment+SpecialFolder]::DesktopDirectory)
$desktopExe=Join-Path $desktop 'Setup_AVRoad_2.0.exe'
if(Test-Path $desktopExe){Copy-Item -LiteralPath $desktopExe -Destination (Join-Path $out 'Desktop-Setup_AVRoad_2.0.previous.exe')}
Copy-Item -LiteralPath (Join-Path $out 'Setup_AVRoad_2.0.exe') -Destination $desktopExe -Force
if((Get-FileHash $desktopExe).Hash -ne (Get-FileHash (Join-Path $out 'Setup_AVRoad_2.0.exe')).Hash){throw 'Desktop copy mismatch'}
Push-Location $root
try{
 & git fetch origin main
 if($LASTEXITCODE){throw 'Fetch failed'}
 $behind=& git rev-list --count HEAD..origin/main
 if([int]$behind -ne 0){throw 'Remote main advanced; review before publishing'}
 # Explicit allowlist. No snapshots, credentials, legacy patches, runtime or payload in Git.
 $files=@('.gitignore','AVRoad_Launcher.cs','AVUpdater.cs','Setup_AVRoad_2026.cs','UpdateToastForm.cs','Cad7ComRegistration.cs','PatchTransaction.cs','version-2.0-rc.json','RELEASE_2.0_RC1.md','tools/Build-V2RC.ps1','tools/Test-V2RC.ps1','tools/Test-V2Com.ps1','tools/Publish-V2RC.ps1')
 & git add -- $files
 if($LASTEXITCODE){throw 'Git stage failed'}
 $staged=@(& git diff --cached --name-only)
 foreach($name in $staged){if($name -notin $files){throw "Unexpected staged file: $name"}}
 & git diff --cached --check
 if($LASTEXITCODE){throw 'Staged diff check failed'}
 & git commit -m 'Prepare AVRoad 2.0 RC1: scoped COM deployment, verified updates and rollback'
 if($LASTEXITCODE){throw 'Commit failed'}
 $commit=(& git rev-parse HEAD).Trim()
 & git push origin HEAD:main
 if($LASTEXITCODE){throw 'Push failed'}
}finally{Pop-Location}
$body=Get-Content -Raw (Join-Path $root 'RELEASE_2.0_RC1.md')
$request=@{tag_name='v2.0-rc.1';target_commitish=$commit;name='AVRoad 2.0 RC1';body=$body;draft=$true;prerelease=$true;make_latest='false'}|ConvertTo-Json -Depth 5
$release=Invoke-RestMethod ($api+'/releases') -Method Post -Headers $headers -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($request))
# Persist the draft ID immediately; any upload failure leaves a reviewable draft.
[pscustomobject]@{id=$release.id;html_url=$release.html_url;draft=$true;commit=$commit;DesktopExe=$desktopExe}|ConvertTo-Json|Set-Content -Encoding utf8 (Join-Path $out 'github-release.json')
$upload=($release.upload_url -split '\{')[0]
foreach($file in @((Join-Path $out 'Setup_AVRoad_2.0.exe'),(Join-Path $out 'patch_2.0_rc1.zip'),(Join-Path $out 'SHA256SUMS.txt'),(Join-Path $root 'version-2.0-rc.json'))){
 $name=[IO.Path]::GetFileName($file)
 $asset=Invoke-RestMethod ($upload+'?name='+[Uri]::EscapeDataString($name)) -Method Post -Headers $headers -ContentType 'application/octet-stream' -InFile $file -TimeoutSec 600
 if($asset.size -ne (Get-Item $file).Length){throw "Uploaded size mismatch: $name"}
 if($asset.digest -and $asset.digest -ne ('sha256:'+(Get-FileHash $file).Hash.ToLowerInvariant())){throw "Uploaded digest mismatch: $name"}
 Write-Output "Uploaded $name ($($asset.size) bytes)"
}
$published=Invoke-RestMethod ($api+'/releases/'+$release.id) -Method Patch -Headers $headers -ContentType 'application/json' -Body '{"draft":false,"prerelease":true,"make_latest":"false"}'
$patchUrl=($published.assets|Where-Object {$_.name -eq 'patch_2.0_rc1.zip'}).browser_download_url
$download=Join-Path $out 'evidence\downloaded-patch.zip'
Invoke-WebRequest $patchUrl -OutFile $download -UseBasicParsing -TimeoutSec 120
if((Get-FileHash $download).Hash.ToLowerInvariant() -ne $manifest.patch_sha256){throw 'Hosted patch checksum mismatch'}
$stable=Invoke-RestMethod 'https://raw.githubusercontent.com/tienhoaa/AVRoad/main/version.json' -Headers @{'User-Agent'='AVRoad-Release'}
if($stable.version -ne '1.9'){throw 'Stable manifest unexpectedly changed'}
[pscustomobject]@{id=$published.id;html_url=$published.html_url;draft=$published.draft;prerelease=$published.prerelease;commit=$commit;DesktopExe=$desktopExe;patch_url=$patchUrl;patch_sha256=$manifest.patch_sha256;download_verified=$true;stable_version=$stable.version;assets=@($published.assets|Select-Object name,size,digest,browser_download_url)}|ConvertTo-Json -Depth 7|Set-Content -Encoding utf8 (Join-Path $out 'github-release.json')
Write-Output $published.html_url
Write-Output $desktopExe
