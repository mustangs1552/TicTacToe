# Findings inventory — mustangs1552/TicTacToe @ 2a54260082

Task: t_45686ff2 (recon + code review lane; feeds report writer t_bc18e3e0 and commit task t_f1694d37)
Pinned commit: 2a54260082b18622dffb29eadfdf41ffb7beb44c — "Built Android and PC versions.", 2016-10-05T21:36:50Z (2016-10-05 17:36:50 -0400), author Matthew George.
Clone (kept, do not delete yet): `/opt/data/kanban/boards/github/workspaces/t_45686ff2/TicTacToe`

## Verification of the checkout

- Clone: `GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none` then `git fetch --depth 1 origin <sha>` + `git checkout <sha>`.
- `git rev-parse HEAD` = `2a54260082b18622dffb29eadfdf41ffb7beb44c` (matches the pin).
- `git ls-remote origin` = `2a54260082b18622dffb29eadfdf41ffb7beb44c  HEAD` and `... refs/heads/master` — the pinned sha **is** the current default-branch head, so owner notes and code are guaranteed to describe the same tree.
- Tracked files: 52 at HEAD (`git ls-files` and `git ls-tree -r` agree) — the tree is tiny: 13 files under `Assets/`, 20 under `Builds/PC/`, 17 under `ProjectSettings/`, plus `.gitignore` and `sh.exe.stackdump`.
- 0 LFS pointer files. The repo has no LFS objects at all, so the expected ~130-byte pointer files simply do not exist here.
- Unity version: `ProjectSettings/ProjectVersion.txt:1` → `m_EditorVersion: 5.4.1f1` (asset headers say `5.4.0f3`).
- `Assets/sceneOne.unity` and `Assets/GridSpot/GridSpot.prefab` are serialized in Unity's **binary** format (EditorSettings `m_SerializationMode`), so they are not diff-reviewable as text; strings extracted instead.

## Owner notes

`repos/mustangs1552/TicTacToe/contents/HermesReviews/reviewNotes.md` on the default branch returns HTTP 404 / `{"message":"Not Found"}` (unauthenticated call; the repo is public — the clone succeeded anonymously). The default-branch root listing (`contents/`) contains only `.gitignore`, `Assets`, `Builds`, `ProjectSettings`, `sh.exe.stackdump` — **no `HermesReviews/` directory exists at head**, and no `HermesReviews` path appears anywhere in the pinned tree.

**Owner exceptions applied: none.** There were no owner notes to apply, so nothing was suppressed and nothing can be misread as waived.

## What was checked and came back clean (negative results — do not invent findings here)

- **Secrets / credentials / tokens:** binary-safe pattern sweep over all 52 tracked blobs (AWS access/secret, private-key PEM headers, JWT, GitHub `ghp_*`, Slack `xox*`, Google `AIza*`, Stripe `sk|pk|rk_live/test`, OpenAI `sk-*`, `scheme://user:pass@` URLs, mongodb/postgres/mysql/redis/amqp connection strings, `password|secret|api_key|token = "…"` assignments). Only hits are stock Microsoft public-key tokens (`PublicKeyToken="b77a5c561934e089"` etc.) inside Unity's bundled Mono config templates. **No real secret in the repository.**
- **Injection:** no SQL, no shell/`Process`, no `System.IO`, no path construction, no template rendering. The only user input is mouse position (`Assets/GridSpot/GridSpot.cs:102` `Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero)`), which is bounds-checked by the collider comparison on line 103 and only selects a grid cell. No sink, no injection surface.
- **Unsafe deserialization:** no `BinaryFormatter`, `XmlSerializer`, `JsonUtility`, `Type.GetType`, `Activator`, AssetBundle or `WWW`/`UnityWebRequest` usage anywhere in the three C# scripts.
- **Auth / authorization:** none exists and none is needed — a local single-player game, no accounts, no network, no persistence (`PlayerPrefs` is never used), no IPC.
- **Logging of sensitive data:** all `Debug.Log*` calls (GameController.cs:187-195, GridSpot.cs:73-81, UIController.cs:84-92) print only script name, GameObject name and fixed strings, and the chatty path is gated behind `isDebug = false` (GameController.cs:11, GridSpot.cs:16, UIController.cs:10). No user data is logged.
- **CI / supply chain:** no `.github/`, no GitLab/Jenkins/Travis/AppVeyor config, no `package.json`, `Packages/manifest.json`, `*.csproj`, `*.sln`, gradle, `Podfile`, `requirements*`, `Dockerfile`, `.lfsconfig` or `.gitattributes`. There is no package manager in play, so there is no lockfile to pin and no transitive dependency to compromise.
- **Default-branch vs pin conflict:** none — same sha.

## Findings

### F-1 — Low — committed build directory carries the author's local dev paths and hardware fingerprint
`Builds/PC/ticTacToe_v01_Data/output_log.txt:9` (also `:5`, `:10`-`:16`, `:20`)

The Unity player log from the developer's machine is committed. It discloses the absolute Windows build path `D:\Documents\SourceTreeProjects\GitHub\TicTacToe\...` (log lines 9-16), GPU `NVIDIA GeForce GTX 970M` (line 5) and display mode `3840x2160 60Hz` (line 20). No username, no credential, no PII beyond machine/environment fingerprinting — but it is permanent attribution data about the author's workstation and it is unrelated to the game.

Fix: `git rm -r --cached Builds` (or `git rm -r Builds`), add `/[Bb]uilds/` to `.gitignore`, and distribute builds as release assets instead of source-tree files. Because the repo is public, the log is already published; treat removal as hygiene, not remediation — a history rewrite is the only way to fully purge it, and that is not worth doing for this content.

### F-2 — Low — 10.4 MB of compiled engine/runtime blobs and Mono debug symbols committed; the build is also incomplete
`Builds/PC/ticTacToe_v01_Data/Managed/UnityEngine.dll.mdb:1`, `Builds/PC/ticTacToe_v01_Data/Managed/UnityEngine.xml:1`, `Builds/PC/ticTacToe_v01_Data/sharedassets0.assets.resS:1` (6.2 MB), `Builds/PC/ticTacToe_v01_Data/Mono/**`

`Builds/` accounts for ~10.4 MB of a repo whose actual source is three C# files (15 KB total). It ships the Mono debug symbol database `UnityEngine.dll.mdb` and full `UnityEngine.xml` API docs, which make the player's assemblies trivially decompilable/debuggable. At the same time the build is not runnable as committed: the player executable and `Managed/Assembly-CSharp.dll` are absent even though `output_log.txt:11-12` shows them being loaded when the build was made — so the directory is both oversized and useless as an artifact.

Fix: untrack `Builds/` (see F-1), publish working builds through GitHub Releases, and build with `Development Build` off / managed debug symbols stripped if binaries are ever redistributed.

### F-3 — Low — Unity 5.4.1f1 is end-of-life and receives no security fixes; engine version is not pinned anywhere machine-enforceable
`ProjectSettings/ProjectVersion.txt:1`

The project targets Unity 5.4.1f1 (released 2016). The 5.x line is end-of-life and has had no security updates for years, and the committed player bundles a Mono 2.x-era runtime (`Builds/.../Mono/etc/mono/1.0`, `2.0`). Real-world exploitability here is essentially nil: the game has no networking, no file parsers, no scripting host, one scene and mouse-only input, so there is no attacker-reachable path into the engine runtime. It is recorded as Low because rebuild/redistribution on an unsupported engine is a supply-chain fact, not because anything is exploitable today.

Fix: if the title is rebuilt or redistributed, port to a supported Unity LTS and record the exact editor version with hash in the README; if it stays an archived 2016 demo, accept the risk explicitly rather than silently.

### F-4 — Low — shipped player embeds Unity's crash-reporting telemetry endpoint
`Builds/PC/ticTacToe_v01_Data/globalgamemanagers:1` (string table contains `https://perf-events.cloud.unity3d.com/api/events/crashes`); configuration keys in `ProjectSettings/UnityConnectSettings.asset` (`CrashReportingSettings`, `UnityAnalyticsSettings`, `UnityPurchasingSettings`)

The built player carries Unity's cloud crash-reporting URL, i.e. it will POST crash/diagnostic events to Unity's service when a crash occurs. Nothing from the game's own logic is collected (the game stores and sends nothing itself), and no credential or identifier for this project is present (`cloudProjectId`/`organizationId` are empty in the asset), so this is a privacy/telemetry disclosure, not a data-leak flaw. The binary settings blobs do not allow confirming the on/off flags from the repo alone.

Fix: in the Editor, open Services and disable Crash Reporting / Analytics (or set `CrashReportingSettings` and `UnityAnalyticsSettings` disabled) before building, and note the behaviour in the README if it is left on.

### F-5 — Informational — Unity's Mono ASP.NET config templates are committed with a clear-text credential format enabled
`Builds/PC/ticTacToe_v01_Data/Mono/etc/mono/2.0/web.config:55`, `Builds/PC/ticTacToe_v01_Data/Mono/etc/mono/1.0/machine.config:107` (both followed on the next line by a commented-out sample user entry)

These are stock files shipped with Unity's Mono runtime: a `<credentials passwordFormat="Clear">` membership block whose only entry is commented out (a placeholder username/password from Microsoft's template, not a real credential, value deliberately not reproduced here). It is inert for a desktop game — nothing hosts ASP.NET — so there is no live credential to rotate.

Fix: covered by removing `Builds/` from the repo (F-1/F-2). No rotation or revocation is required; if the templates ever were deployed under a web host, the clear-text format and the sample entry must be removed first.

### F-6 — Informational — leftover crash dump committed, and `.gitignore` does not cover builds/symbols/dumps
`sh.exe.stackdump:1`, `.gitignore:1`-`.gitignore:28`

A 237-byte Windows stack dump from a crashed `sh.exe` is tracked. Its content is three frame addresses and a function list — no memory contents, no strings, nothing sensitive. Root cause of F-1/F-2/F-5: `.gitignore` ignores `Library/`, `Temp/`, `Obj/`, `.csproj/.sln/.user*`, `sysinfo.txt`, `*.apk`, `*.unitypackage` — but **not** `/[Bb]uilds/`, `*.mdb`, `*.stackdump`.

Fix: `git rm --cached sh.exe.stackdump` and extend `.gitignore` with `/[Bb]uilds/`, `*.mdb`, `*.stackdump`, `*.pdb`.

## Non-security observations (record them as such, not as vulnerabilities)

- `Assets/GameController.cs:49-53`: after the 9th move, `turnNum >= 9` calls `ShowLooseScreen()` unconditionally, so a win on the final move is overwritten by "Draw!"; `Assets/UIController.cs:42-43` derives the winner from `XTurn` rather than from the winning `State`, which mislabels the winner. Gameplay/UI correctness bugs only.
- `Assets/GridSpot/GridSpot.cs:126`: stray double semicolon. Cosmetic.
- Scene/prefab serialized in binary mode (see above) — hurts reviewability and merge safety, not security.

## Counts

0 Critical / 0 High / 0 Medium / 4 Low (F-1..F-4) / 2 Informational (F-5, F-6). No exploitable vulnerability, no secret, no injection or deserialization sink, no auth surface, no third-party dependency. The repository is clean from a secrets/exploit standpoint; every finding is a committed-artifact or engine-lifecycle hygiene issue.

## Handoff

- Clone to reuse or ignore: `/opt/data/kanban/boards/github/workspaces/t_45686ff2/TicTacToe` (left in place as instructed; the commit task may equally re-clone, since the pin equals the current `master` head).
- Owner exceptions to reproduce in the report: none — state that explicitly.
