# AVRoad 2.1 RC2 UI baseline

- Baseline ID: AVROAD-2.1-RC2-UI-2026-11-05.
- Source branch: codex/avroad-2.1-rc2-baseline; base commit: d99152e4945cc6bb5b19059fad9bfbe01c2ba340. It does not include d15abaa in its history.
- Runtime payload identity: release-v21/stage; staged AVRoad.exe SHA256 matched the installed C:\AVRoad_2026\AVRoad.exe before branch work.
- Manifest metadata remains 2.1.0.1 / RC1; identify the RC2 UI baseline by the staged binary hashes, not by the manifest label.
- UI source overlay is selective: launcher panel loader from d15 source, with Anti profile-bootstrap calls removed; settings/updater sources use pre-fix backups. Setup and Cad7ComRegistration remain unchanged from base.
- The generated Setup embeds the later d15 payload and is not designated as the clean RC2 installer. No Setup was built.
- Snapshot folder: diagnostics/rc2-baseline-snapshot.

## Snapshot hashes

- AVRoad_Launcher.cs — 108877 bytes — SHA256 C1C729BC00DC0C584F057ED0EF2004DED65C7C320F5DAA48982729514C94B236
- AVRoad_Profile.arg — 59524 bytes — SHA256 63A84451522DA0AD77ED2F4F027128A33B977A5152D3BCBFECA4E9A10734A321
- AVRoad.Deployment.dll — 13312 bytes — SHA256 2416576AEEB799B9034F0E8BF39727CBA9777915981EA59ECC15308FA8B5DBC3
- AVRoad.exe — 305664 bytes — SHA256 D57CC1E4BDC282A4B9FB16400FD7E14F0F597AC2F99CBF1051A749799B63BE50
- AVSetting.cs — 29125 bytes — SHA256 87F3A74FCED0F164649DC7A8C0DFCC2F6C7FF277507E2843C2DAE17BD8F2BEF3
- AVUpdater.cs — 22831 bytes — SHA256 B8D3366827CC6F1049FEA5716FF305F37496E393D83B8A74C765DD87B991F126
- AVUpdater.v2.exe — 65024 bytes — SHA256 8CFD525DA9234EA253403D25EC307B97A5F5AD91FDDA4D8B9BBE6B00575FC9F0
- version.json — 532 bytes — SHA256 E6D814C7602D7691597F1273E5EF4A8A2A51217118B7960C3D5026A8ACD5AC88
