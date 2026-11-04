$ErrorActionPreference='Stop'
[Reflection.Assembly]::LoadFrom('C:\TDT_RESEARCH\release-v2\stage\AVRoad.Deployment.dll')|Out-Null
$backup='C:\TDT_RESEARCH\release-v2\evidence\com-helper-rollback.xml'
$result=[AVRoadDeployment.Cad7ComRegistration]::Apply('C:\AVRoad_2026',$backup)
[AVRoadDeployment.Cad7ComRegistration]::Restore($result)
'PASS: compiled shared helper applied/read back/restored only the three already-approved HKCU Registry32 defaults.'|Set-Content -Encoding utf8 C:\TDT_RESEARCH\release-v2\evidence\com-helper-test.txt
Get-Content C:\TDT_RESEARCH\release-v2\evidence\com-helper-test.txt
