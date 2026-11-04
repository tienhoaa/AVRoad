# AVRoad 2.0 RC1 — prerelease, not production approved

This RC continues tienhoaa/AVRoad's existing version.json/ZIP updater protocol.
The stable main/version.json remains at v1.9. RC metadata is version-2.0-rc.json;
its numeric version 2.0.0.1 remains comparable by v1.x clients.

Changes: exactly three per-user 32-bit native COM factories (AcadDatabase,
AcadCircle, AcadLine) point to the local AVRoad axdb.dll. Shared deployment code
is invoked by Setup, startup, and the updater. No Autodesk binary modifications.
No UI redesign. RC disables the legacy application-controlled HKLM, FullControl,
type-library bulk registration, file-association and automatic runtime-installer
repairs; a pristine machine may still need dependencies not covered by this scope.

New updater: mandatory patch_sha256 verification, normal certificate validation,
reject traversal/duplicate/unapproved patch entries, backup before writes,
rollback on failure, commit version.json last. It runs from a temporary directory
so installed updater/deployment files can be replaced. A side-by-side
AVUpdater.v2.exe lets a v1.x updater deliver it without overwriting its running EXE.
The launcher uses that updater when available. Patch contents are only four
AVRoad-owned files, not Autodesk engine files or diagnostics.

Rollback: file journals live under .avroad-rollback/files/<transaction>/rollback.xml;
COM rollback is restricted to the three values with preflight checks against
unrelated later edits. Run AVUpdater.v2.exe /rollback <journal> <result-file>
with AVRoad closed. Standalone COM rollback files are .avroad-rollback/com-v2.xml.
Keep backups; never restore the whole machine registry from these snapshots.

Known blockers to a final release:
- Three core-engine test cycles read Circle Radius=50, Center=(100,100,0),
  Line Length=100 and layer data. Normal shutdown is not clean: at least the
  first cycle logs the existing F49Dh access violation, other cycles do not
  finish within the bounded wait. No binary workaround was applied.
- Prior on-machine UI evidence confirms CIRCLE/LINE Properties, QSELECT
  filtering and layer creation without a separate CAD7 install after reinstalling
  AVRoad. Those UI checks were not repeated in every shutdown cycle.
- Clean Windows installation, actual unchanged v1.9 updater end-to-end transition,
  and online update/restart against the final hosted artifacts are not yet proven.
  Legacy v1.x updaters do not validate SHA-256; do not advertise their path as
  checksum-verified merely because a new updater is included in the patch.
- Setup extraction verification is file-only and deliberately does not invoke
  unrelated machine configuration. Not equivalent to a clean-machine install.

Do not promote this RC or replace the stable manifest until these blockers pass.
Artifacts: Setup_AVRoad_2.0.exe (RC1), patch_2.0_rc1.zip, SHA256SUMS.txt.
