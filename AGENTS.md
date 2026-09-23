# AGENTS.md

Unity **2022.3.62f3** URP starter (C#). Verify C# with Unity batchmode (`scripts/check-compile.sh`) or opening the Editor. Style: root **`.editorconfig`** (no Roslyn analyzer pack yet). CI: GitHub Actions under `.github/workflows/` (**PR compile gate** `unity-compile` + itch.io deploy + Drive asset retrieve/track).

## Sources of truth

- **`ARCHITECTURE.md`** — folder rules, base classes, lifecycle, naming (System vs Manager vs Controller). Prefer this over README.
- **`README.md`** — project pitch + setup; folder tree uses `_Game/` + `Systems/`, points at `TemplateScene.unity` (Boot scene does not exist).
- Executable config wins over docs: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `.gitattributes`, `.github/workflows/`.

## Layout (real paths, not README's)

```
Assets/_Game/          # all game content
  00_Scripts/          # Core, Systems, Manager, Game, UI, Utils
  01_Objects/          # Prefabs/ + Data/ (SO variants)
  03_Audio/            # GameAudioMixer.mixer (+ Music/, SFX/ for Drive-synced clips)
  04_Scenes/           # TemplateScene only (in Build Settings)
  05_Settings/         # URP, Input
Assets/Editor/         # BuildScript.cs (CI -executeMethod) + GameAssetCreator
Assets/_Vendor/        # third-party — do not edit
.github/workflows/     # unity-compile (PR), unity-itchio-deploy, retrieve, track
.githooks/              # versioned hooks — enable: git config core.hooksPath .githooks
scripts/check-compile.sh  # shared batchmode compile check (pre-push + docs)
retrieve.py, track.py, core/, state/, requirements.txt   # Drive → assets tooling
```

- Namespaces mirror folders: `Slafurry.Systems.*`, `Slafurry.Core.*`, `Slafurry.Utils.*`. Folder is **`Systems/`** (plural) — not `System/` (BCL conflict).
- **No `.asmdef`** — every non-`Editor` script compiles into `Assembly-CSharp`. Editor-only code must live under an `Editor/` folder.
- Internal rule: max 6 subfolders per folder (ARCHITECTURE.md).

## Boot / lifecycle (easy to get wrong)

- `Singleton`/`Manager` `Awake` is sealed and auto-calls `LoadingSystem.Instance.Register(this)`. **`LoadingSystem` must exist in the scene first** — it is on `AudioSystem.prefab` (first component after Transform); do not remove it.
- `Initialize()` = internal setup only (no other-object refs). Cross-object wiring only in `PostInitialize()`. Avoid `Start()` (guard with `_isReady` if unavoidable).
- `IInitializable.Priority` orders the boot (lower runs first). Late registrants after boot get a one-frame delayed batch (`LoadingSystem`).
- Base class choice: cross-scene singleton → `GameSystem<T>`; scene-bound singleton → `LocalSingleton<T>`; session coordinator → `Manager` (registers with `GameManager` in `PostInitialize`).
- **`GameManager` does not exist yet** despite ARCHITECTURE.md / `Manager` docs referencing it — Manager subclasses cannot compile until it's added.
- `BootstrapLoader` (Systems/Scene) is optional: wire `systemsToWaitFor` + `targetSceneName` if you add a boot scene.

## Static facades (prefer these over `.Instance` chains)

`Audio` · `Save` · `Pause` · `Controls` (input) · `Localize` · `SceneSystem` · `GameFeel`

- Pause is a **key stack** (`Pause.On("UI")` / `Pause.Off("UI")`), not a bool — multiple sources can hold pause.
- Save: Newtonsoft.Json → `Application.persistentDataPath/{name}.json`.
- Linear volume → dB via `Mathf.Log10(x) * 20` before `AudioMixer.SetFloat`.

## Unity / git gotchas

- **Git LFS required** (`git lfs install` before clone). `.gitattributes` LFS-covers binaries (png/ttf/pdf/audio/models…). Working tree may show dirty `_Vendor` LFS files (mode/filter noise) — don't commit vendor churn by accident.
- `.gitattributes` sets `merge=unityyamlmerge` for `*.unity`/`*.asset`/etc., but the driver is **not configured in git config** — Unity YAML conflict merges will fail until UnityYAMLMerge is set up (`Editor/Data/Tools/UnityYAMLMerge`).
- Serialization: Force Text YAML + Visible Meta Files. **Commit `.meta` with every new asset**; never hand-edit GUIDs.
- Input System only (`activeInputHandler: 2`). Actions: `Assets/_Game/05_Settings/Input/Main Input.inputactions` → generated `Main Input.cs` (regenerate in Unity after editing actions).
- Editor menu: **Slafurry → Game Data** (GameAssetCreator). New ScriptableObjects for that window use `[GameAssetCreator(category, displayName, order)]`, not `[CreateAssetMenu]` ordering.
- `EditorBuildSettings` lists Menu scenes that are disabled and missing from disk — don't assume they're loadable.
- **`retrieve.yml` does `rm -rf Assets/_Game/03_Audio` then restores from `origin/main`** — commit `GameAudioMixer.mixer` before relying on that workflow; Drive only fills Music/SFX clips.
- CI deploy needs secrets: `UNITY_EMAIL`, `UNITY_PASSWORD`, `BUTLER_API_KEY`, optional `DISCORD_WEBHOOK_URL_DEPLOY`, `GEMINI_API_KEY`. PR compile reuses `UNITY_EMAIL` / `UNITY_PASSWORD`. Drive tools need `GOOGLE_SERVICE_ACCOUNT_JSON_B64`, `DRIVE_SPRITE_FOLDER_ID`, `DRIVE_AUDIO_FOLDER_ID`.
- `core.hooksPath` is **not** committed (local `git config`). Each clone must run `git config core.hooksPath .githooks` or pre-push compile will not run; LFS then chains via `.githooks/pre-push`.

## Verify changes

- **Style only:** `.editorconfig` (editor/formatter hints — not a compile gate).
- **Compile:** `./scripts/check-compile.sh` (Unity batchmode; fails on `error CS` or non-successful exit). Bypass once: `SKIP_UNITY_COMPILE=1`.
- **Local push gate:** enable once per clone — `git config core.hooksPath .githooks`. `pre-push` runs **Git LFS first**, then `check-compile.sh`.
- **CI:** `unity-compile.yml` runs the same batchmode check on **pull requests only** (paths: `Assets/`, `Packages/`, `ProjectSettings/`). Needs secrets `UNITY_EMAIL`, `UNITY_PASSWORD`. Require this check in branch protection to block merging broken PRs.

Manual equivalent:

```bash
./scripts/check-compile.sh
# or
~/Unity/Hub/Editor/2022.3.62f3/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" -logFile /tmp/unity-compile.log
```

Prefer this (or opening the Editor) after any C# or `.asmdef`-adjacent change. Play-mode behavior still needs a manual Editor run (`TemplateScene`).
