# Security review — mustangs1552/TicTacToe @ 2a54260082

| | |
|---|---|
| Repository | `mustangs1552/TicTacToe` (public) |
| Pinned commit | `2a54260082b18622dffb29eadfdf41ffb7beb44c` — abbreviated `2a54260082`, committed 2016-10-05T21:36:50Z (2016-10-05 17:36:50 -0400) by Matthew George, subject "Built Android and PC versions." |
| Review date | 2026-10-23 (UTC) |
| Review scope | Full tree at the pinned sha: 52 tracked files (3 hand-written C# scripts, 1 scene, 1 prefab, 20 committed build files, 17 `ProjectSettings/` assets, `.gitignore`, `sh.exe.stackdump`) |
| Engine | Unity 5.4.1f1 |
| Method | Read-only. Blobless shallow clone, `git rev-parse HEAD` verified equal to the pin. All 52 blobs enumerated; C# read end to end; binary artifacts inspected via string extraction. GitHub was not written to by this task. |

## Verdict

**0 Critical, 0 High, 0 Medium, 4 Low, 2 Informational.**

There is **no hardcoded secret, token, password or key anywhere in the repository**. There is no
injection sink, no deserialization sink, no authentication surface, no network code, no persistence
and no third-party dependency. Nothing here is exploitable by an attacker.

Every finding below is a committed-artifact or engine-lifecycle hygiene issue: the repo ships a
developer build directory, a player log, an out-of-support engine version, and a leftover crash dump.
That is the honest result — this is a 2016 single-player Unity tic-tac-toe with three scripts totalling
~15 KB of C#.

## What was checked and came back clean

Listed so future reviews do not have to re-derive it, and to make clear no finding was invented:

- **Secrets / credentials / tokens.** Binary-safe pattern sweep over all 52 tracked blobs: AWS access
  and secret keys, PEM private-key headers, JWTs, GitHub `ghp_*`, Slack `xox*`, Google `AIza*`,
  Stripe `sk|pk|rk_*`, OpenAI `sk-*`, `scheme://user:pass@` URLs, mongodb/postgres/mysql/redis/amqp
  connection strings, and `password|secret|api_key|token = "…"` assignments. The only matches are
  stock Microsoft public-key tokens (`PublicKeyToken="b77a5c561934e089"` and similar) inside Unity's
  bundled Mono configuration templates — public constant values, not credentials.
- **Injection.** No SQL, no shell, no `System.Diagnostics`/`Process`, no `System.IO`, no path
  construction, no template rendering. The only user input is the mouse position, consumed at
  `Assets/GridSpot/GridSpot.cs:102` via `Physics2D.Raycast` and bounds-checked by the collider
  comparison on line 103. It selects a grid cell; there is no sink for it to reach.
- **Unsafe deserialization.** No `BinaryFormatter`, `XmlSerializer`, `JsonUtility`, `Type.GetType`,
  `Activator`, AssetBundle, `WWW` or `UnityWebRequest` in `Assets/`. The scene and prefab are stored
  in Unity's binary serialization format, but that is an asset format read only by the Unity runtime
  from the project itself — not an attacker-controlled input path.
- **Auth / authorization / insecure defaults.** No accounts, no network, no IPC, no persistence
  (`PlayerPrefs` is never used). A local single-player game has no auth surface to get wrong. No
  service is exposed and no default listener is bound.
- **Logging of sensitive data.** All `Debug.Log` calls are gated behind `isDebug = false`
  (`Assets/GameController.cs:11`, `Assets/UIController.cs:10`, `Assets/GridSpot/GridSpot.cs:16`), and
  every message is a fixed string plus the script and GameObject name
  (`Assets/GameController.cs:187-195`, `Assets/UIController.cs:84-92`, `Assets/GridSpot/GridSpot.cs:73-81`).
  No user data, path or identifier is logged by the game.
- **Supply chain / dependencies.** No `.github/`, GitLab, Jenkins, Travis or AppVeyor config; no
  `package.json`, `Packages/manifest.json`, `*.csproj`, `*.sln`, Gradle, Podfile, `requirements*`,
  `Dockerfile`, `.lfsconfig` or `.gitattributes`. There is no package manager in play, so there is no
  lockfile to pin and no transitive dependency to compromise. 0 LFS objects.

## Findings

### F-1 — Low — committed player log leaks the author's local build paths and hardware fingerprint

**Location:** `Builds/PC/ticTacToe_v01_Data/output_log.txt:9` (same disclosure repeats on lines 10-19;
GPU on line 5; display mode on line 20). File is 45 lines / 3,005 bytes and is tracked.

**What is wrong:** a Unity player log from the developer's own machine is committed. Line 9 records an
absolute Windows build path of the shape `D:\…\SourceTreeProjects\GitHub\TicTacToe\Builds\PC\…`;
line 5 names the GPU (`NVIDIA GeForce GTX 970M`, VRAM 3037 MB); line 20 records
`desktop: 3840x2160 60Hz`. No username, no credential and no customer data — but it is permanent,
public attribution data about the author's workstation, and it serves no purpose in a source repo.
Because the repo is public the content is already published; removal is hygiene, not remediation.

**Fix:** `git rm -r --cached Builds` (see F-2 for the same directory), add `/[Bb]uilds/` to
`.gitignore`, and distribute builds as release assets rather than source-tree files. A history
rewrite is the only way to purge the log completely; it is not worth doing for content of this kind.

### F-2 — Low — 10.4 MB of committed build output, including Mono debug symbols, and the build is not runnable

**Location:** `Builds/PC/ticTacToe_v01_Data/Managed/UnityEngine.dll.mdb` (387,064 bytes),
`Builds/PC/ticTacToe_v01_Data/Managed/UnityEngine.xml` (2,108,916 bytes),
`Builds/PC/ticTacToe_v01_Data/sharedassets0.assets.resS` (6,225,636 bytes), plus the rest of
`Builds/PC/ticTacToe_v01_Data/Mono/**`. Whole tree: 10,408,527 bytes across 20 tracked files.
(No line numbers — these are binary artifacts; the path and byte size are the locator.)

**What is wrong:** `Builds/` is ~10.4 MB against ~15 KB of actual source. It ships the Mono debug
symbol database `UnityEngine.dll.mdb` and the full `UnityEngine.xml` API documentation, which make
the shipped assemblies trivially decompilable and debuggable. At the same time the directory is not
runnable as committed: there is no player `.exe` and no `Managed/Assembly-CSharp.dll` — the `Managed/`
directory contains only the two files listed above, even though `output_log.txt:11-12` shows
`Assembly-CSharp.dll` being loaded when the build was produced on the author's machine. So the
committed directory is simultaneously oversized and useless as a distributable artifact.

**Fix:** untrack `Builds/` (F-1), publish working builds through GitHub Releases, and if binaries are
ever redistributed again, build with development build / managed debug symbols stripped so `.mdb`
and API XML are not shipped.

### F-3 — Low — Unity 5.4.1f1 is end-of-life and receives no security fixes; the engine version is not pinned anywhere machine-enforceable

**Location:** `ProjectSettings/ProjectVersion.txt:1` (`m_EditorVersion: 5.4.1f1`; asset headers in the
binary files record `5.4.0f3`), with an embedded Mono 2.x-era runtime under
`Builds/PC/ticTacToe_v01_Data/Mono/etc/mono/1.0` and `.../2.0`.

**What is wrong:** the project targets a 2016 engine on a line that has been out of support for years
and will never receive another security patch. Real-world exploitability here is essentially nil —
no networking, no file parsers, no scripting host, one scene, mouse-only input, so there is no
attacker-reachable path into the engine runtime. It is recorded as Low because rebuilding or
redistributing on an unsupported engine is a supply-chain fact, not because anything is exploitable
today. Separately, the version lives only in a checked-in text file; nothing enforces it, so a build
machine can silently use a different editor.

**Fix:** if the title is rebuilt or redistributed, port to a supported Unity LTS, record the exact
editor version and hash in the README, and add a CI or pre-commit check that compares
`ProjectVersion.txt` against the expected value. If it stays an archived 2016 demo, accept the risk
explicitly in the README rather than silently.

### F-4 — Low — the shipped player embeds Unity's crash-reporting telemetry endpoint

**Location:** `Builds/PC/ticTacToe_v01_Data/globalgamemanagers` (string table contains
`https://perf-events.cloud.unity3d.com/api/events/crashes`), configured by
`ProjectSettings/UnityConnectSettings.asset` (`CrashReportingSettings.m_EventUrl` holds the same URL;
the asset also carries `UnityAnalyticsSettings` and `UnityPurchasingSettings` blocks).

**What is wrong:** the built player will POST crash and diagnostic events to Unity's cloud service
when it crashes. Nothing from the game's own logic is collected — the game stores and transmits
nothing itself — and no project or organization identifier appears in the asset (Unity 5.4's
`UnityConnectSettings` schema has no `cloudProjectId`/`organizationId` field at all). This is a
privacy/telemetry disclosure, not a data-leak flaw. The on/off booleans live in the binary settings
blob, so the repository alone cannot prove whether reporting is enabled in this particular build.

**Fix:** in the Editor, open Services and disable Crash Reporting and Analytics (or set the
`CrashReportingSettings` / `UnityAnalyticsSettings` entries to disabled) before building, then state
the resulting behaviour in the README. Treat disabling-then-shipping as the default for a
redistributable build.

### F-5 — Informational — Unity's bundled Mono ASP.NET templates are committed with clear-text password storage enabled

**Location:** `Builds/PC/ticTacToe_v01_Data/Mono/etc/mono/2.0/web.config:55` and
`Builds/PC/ticTacToe_v01_Data/Mono/etc/mono/1.0/machine.config:107`, each with a commented-out sample
user entry on the following line.

**What is wrong:** these are stock files shipped with Unity's Mono runtime. Line 55 / 107 is a
membership `<credentials passwordFormat="Clear">` block (setting quoted; no value is reproduced here),
whose only entry is commented out and is a placeholder sample account from Microsoft's template — not
a real credential and not a live one. It is inert for a desktop game, because nothing in this project
hosts ASP.NET. There is therefore nothing to rotate and nothing to revoke. The reason it is recorded
at all is that a clear-text credential format shipped in a public repo is the kind of thing a future
fork could copy into a real deployment.

**Fix:** covered by removing `Builds/` from the repo (F-1 / F-2). No rotation or revocation required.
If these templates were ever deployed under a real web host, the clear-text `passwordFormat` and the
sample entry must be deleted first and any account created from it rotated.

### F-6 — Informational — leftover crash dump committed, and `.gitignore` does not cover builds, symbols or dumps

**Location:** `sh.exe.stackdump` (6 lines, 237 bytes: a three-frame address trace plus
"End of stack trace"); `.gitignore:26-28` (the "# Builds" section lists only `*.apk` and
`*.unitypackage`).

**What is wrong:** a Windows stack dump from a crashed `sh.exe` is tracked. Its content is frame
addresses only — no memory contents and no strings — so nothing sensitive is exposed, but it is dead
weight in a public repo. The root cause of F-1, F-2 and F-5 is visible in the same place: `.gitignore`
covers `/[Ll]ibrary/`, `/[Tt]emp/`, `/[Oo]bj/`, generated project files, `sysinfo.txt`, `*.apk` and
`*.unitypackage`, but **not** `/[Bb]uilds/`, `*.mdb`, `*.stackdump` or `*.pdb`.

**Fix:** `git rm --cached sh.exe.stackdump`, delete the file, and extend `.gitignore` with
`/[Bb]uilds/`, `*.mdb`, `*.stackdump` and `*.pdb`.

## Exploitable issues vs. nitpicks

There are no exploitable issues in this repository. To be explicit about the grading:

- F-1 and F-2 are **disclosure of the author's own environment** and **bloat with a debuggability
  side effect**. They matter because the repo is public; they do not expose any third party.
- F-3 is a **lifecycle/supply-chain note**. The code as written cannot reach the vulnerable parts of
  an EOL engine.
- F-4 is a **privacy/telemetry disclosure** in the shipped binary, not in the source.
- F-5 and F-6 are **Informational**: inert template content and a harmless dump, recorded so they
  are not re-discovered as mysteries.
- Nothing in the C# is a security finding. The three scripts contain no I/O, no network, no parsing
  and no privileges.

## Owner exceptions applied

**None.** There were no owner notes to apply, so nothing was suppressed and nothing is waived.

Verification: `HermesReviews/reviewNotes.md` on the default branch returns 404 / `Not Found`
(checked again during this review via the raw file URL, unauthenticated, against `master`). No
`HermesReviews/` path exists in the pinned tree either, so there is not even a stale copy at the sha.
Consequently this report has no suppressed findings and lists every finding that was found — the
`Owner exceptions applied` section is present and empty by fact, not by omission.

The owner owns that notes file: it must never be created, edited, deleted, or committed by this
review family.

## Default-branch notes vs. pinned sha

No conflict, and none can arise silently: the pinned sha `2a54260082` is the current default-branch
head (`git ls-remote origin` shows the same sha for `HEAD` and `refs/heads/master`, verified during
recon and reverified as `git rev-parse HEAD` = pin in the local checkout used for this report). Any
owner notes added later would describe a strictly newer tree, and this report would then be the
pinned-sha snapshot rather than a description of the branch tip.

## Redaction statement

These review docs are committed into a possibly public repository, so this document is treated as
already published. It contains no secret value: no key, token, password, private-key body, JWT,
credential-bearing connection string or `.env` content. Credential-adjacent locations are named by
`path:line` and type only; the one config line quoted (F-5) is quoted for its `passwordFormat` setting,
with its value fields omitted. Masking was not needed anywhere in this report because the repository
contains no real credential to mask — the only credential-shaped material in the tree is a commented
placeholder in a stock Microsoft template, and its values are deliberately not reproduced.

The report was checked with the review-doc secret gate (`review_doc_secret_check.py`, no `--fix`),
which printed `OK`.

## Non-security observations (correctness, not vulnerabilities)

Recorded because they will be noticed by anyone reading the code; they are **not** security findings.

- `Assets/GameController.cs:49-53`: after each turn `turnNum` is incremented and `if (turnNum >= 9)`
  unconditionally calls `ShowLooseScreen()`, so a win on the ninth move is overwritten by "Draw!".
- `Assets/UIController.cs:42-43`: the winner label is derived from `XTurn` (who would play next)
  rather than from the winning `State`, so the winner can be mislabelled.
- `Assets/GridSpot/GridSpot.cs:126`: stray double semicolon. Cosmetic.
- `Assets/sceneOne.unity` and `Assets/GridSpot/GridSpot.prefab` are serialized in Unity's binary
  format, so they are not reviewable or merge-friendly as text. Consider text serialization for the
  project (`EditorSettings` `m_SerializationMode`).

## Reproduce

1. `GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none https://github.com/mustangs1552/TicTacToe.git`
2. `git -C TicTacToe fetch --depth 1 origin 2a54260082b18622dffb29eadfdf41ffb7beb44c`
3. `git -C TicTacToe checkout 2a54260082b18622dffb29eadfdf41ffb7beb44c` and confirm `git rev-parse HEAD`.
4. `git -C TicTacToe ls-files` (52 files); read the three `Assets/**/*.cs` files.
5. `strings Builds/PC/ticTacToe_v01_Data/globalgamemanagers` and
   `strings ProjectSettings/UnityConnectSettings.asset` for F-4; `du -sb --apparent-size Builds` for F-2.
