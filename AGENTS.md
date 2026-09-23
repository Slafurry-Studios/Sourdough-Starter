# AGENTS.md

Unity **2022.3.62f3** URP starter (C#). Verify C# with `./scripts/check-compile.sh` (Unity batchmode) or open the Editor. Style: root **`.editorconfig`** (hints only — not a gate). CI: GitHub Actions under `.github/workflows/` (**PR compile gate** `unity-compile` + itch.io deploy + Drive retrieve/track).

## Sources of truth

- **`ARCHITECTURE.md`** — folder rules, base classes, lifecycle, naming (System vs Manager vs Controller). Prefer this over README.
- **`README.md`** — pitch + setup; trees use `_Game/` + `Systems/`; only scene is `TemplateScene.unity`.
- Executable wins over docs: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `.gitattributes`, `.github/workflows/`.

## Layout

```
Assets/_Game/          # all game content
  00_Scripts/          # Core, Systems, Manager, Game, UI, Utils
  01_Objects/          # Prefabs/ + Data/ (SO variants)
  03_Audio/            # GameAudioMixer.mixer (+ Music/, SFX/ for Drive clips)
  04_Scenes/           # TemplateScene only (in Build Settings)
  05_Settings/         # URP, Input
Assets/Editor/         # BuildScript.cs (CI -executeMethod) + GameAssetCreator
Assets/_Vendor/        # third-party — do not edit
.github/workflows/     # unity-compile (PR), unity-itchio-deploy, retrieve, track
.githooks/             # versioned hooks — enable: git config core.hooksPath .githooks
scripts/check-compile.sh  # shared batchmode compile check (pre-push + this file)
scripts/test_state.py     # Drive state regressions (deleted-file re-notify)
retrieve.py, track.py, core/, state/, requirements.txt   # Drive → assets tooling
```

- Namespaces mirror folders: `Slafurry.Systems.*`, `Slafurry.Core.*`, `Slafurry.Utils.*`. Folder is **`Systems/`** (plural) — not `System/` (BCL conflict).
- **No `.asmdef`** — every non-`Editor` script compiles into `Assembly-CSharp`. Editor-only code must live under an `Editor/` folder.
- Internal rule: max 6 subfolders per folder (ARCHITECTURE.md).

## Boot / lifecycle (easy to get wrong)

- `Singleton`/`Manager` `Awake` is sealed and auto-calls `LoadingSystem.Instance.Register(this)`. **`LoadingSystem` must exist first** — on `AudioSystem.prefab`, the **AudioSystem** GameObject’s first component after Transform (script GUID `2ba4b0833086a19a0b6ff4653a00344a`). Do not remove it.
- `Initialize()` = internal setup only (no other-object refs). Cross-object wiring only in `PostInitialize()`. Avoid `Start()` (guard with `_isReady` if unavoidable).
- `IInitializable.Priority` orders boot (lower first). Late registrants after boot get a one-frame delayed batch (`LoadingSystem`).
- Base class: cross-scene singleton → `GameSystem<T>`; scene-bound singleton → `LocalSingleton<T>`; session coordinator → `Manager` (registers with `GameManager` in `PostInitialize` via abstract `RegisterToGameManager`/`OnPostInitialize`).
- **`GameManager` does not exist** — `Manager.cs` and ARCHITECTURE still describe it; **any `Manager` subclass will not compile** until you add one (or drop the abstract hooks).
- `BootstrapLoader` (Systems/Scene) is optional: wire `systemsToWaitFor` + `targetSceneName` if you add a boot scene. Default `targetSceneName` is `"MainMenu"` (not in Build Settings).

## Static facades (prefer over `.Instance` chains)

`Audio` · `Save` · `Pause` · `Controls` (input) · `Localize` · `SceneSystem`

- Pause is a **key stack** (`Pause.On("UI")` / `Pause.Off("UI")`), not a bool — multiple holders can keep pause.
- Save: Newtonsoft.Json → `Application.persistentDataPath/{fileName}.json` (caller omits `.json`).
- Linear volume → dB via `Mathf.Log10(x) * 20` before `AudioMixer.SetFloat`.
- `GameFeel` is a **MonoBehaviour**, not a static facade.

## Unity / git gotchas

- **Git LFS required** (`git lfs install` before clone). `.gitattributes` LFS-covers binaries. Working tree may show dirty `_Vendor` LFS files — **do not commit vendor churn**.
- `.gitattributes` sets `merge=unityyamlmerge` for `*.unity`/`*.asset`/etc., but the **driver is not in git config** — Unity YAML conflict merges fail until UnityYAMLMerge is configured (`Editor/Data/Tools/UnityYAMLMerge`).
- Serialization: Force Text YAML + Visible Meta Files. **Commit `.meta` with every new asset**; never hand-edit GUIDs.
- Input System only (`activeInputHandler: 2`). Actions: `Assets/_Game/05_Settings/Input/Main Input.inputactions` → generated `Main Input.cs` (regenerate in Unity after editing actions). Legacy `Input.GetKeyDown` in `DIalogHUD` is dead code — do not copy that pattern.
- Editor menu: **Slafurry → Game Data** (GameAssetCreator). Types for that window use `[GameAssetCreator(category, displayName, order)]`; other SOs still use `[CreateAssetMenu]`.
- `EditorBuildSettings` lists only `TemplateScene` (enabled). Do not assume other scenes are loadable.
- **Drive bots re-sync from `origin/main` every run**: `retrieve.yml` `rm -rf`s `state/`, `Assets/_Game/02_Art/Sprite`, `Assets/_Game/03_Audio` then restores via `git archive origin/main`. Commit `GameAudioMixer.mixer` (and any local state) to `main` before relying on those workflows; Drive only fills Music/SFX under those folders.
- **`track` / `retrieve` commit `state/` to `main`** (`[skip ci]`) so `deleted_in_drive` / `retrieve_status` / `downloaded_*` persist. Assets still go `chore/asset` → PR. `core/state.py` must keep retrieve fields when track merges a fresh listing (`scripts/test_state.py`).
- CI secrets: deploy needs `UNITY_EMAIL`, `UNITY_PASSWORD`, `BUTLER_API_KEY`, optional `DISCORD_WEBHOOK_URL_DEPLOY`, `GEMINI_API_KEY`. PR compile needs `UNITY_EMAIL` / `UNITY_PASSWORD`. Drive tools need `GOOGLE_SERVICE_ACCOUNT_JSON_B64`, `DRIVE_SPRITE_FOLDER_ID`, `DRIVE_AUDIO_FOLDER_ID`, `DISCORD_WEBHOOK_URL`.
- `core.hooksPath` is **not** committed (local `git config`). Each clone: `git config core.hooksPath .githooks` or pre-push compile will not run; LFS chains via `.githooks/pre-push`.

## Verify changes

- **Style only:** `.editorconfig`.
- **Compile:** `./scripts/check-compile.sh` (fails on `error CS` or non-successful exit). Bypass once: `SKIP_UNITY_COMPILE=1`. Override binary: `UNITY_BIN=...`.
- **Local push gate:** `git config core.hooksPath .githooks` once per clone. `pre-push` runs **Git LFS first**, then `check-compile.sh`.
- **CI:** `unity-compile.yml` runs the same check on **PRs only** (paths: `Assets/`, `Packages/`, `ProjectSettings/`). Require this check in branch protection to block broken merges.
- **Drive tooling:** `python3 scripts/test_state.py` after touching `core/state.py` / `track.py` / `retrieve.py`.

```bash
./scripts/check-compile.sh
# or
~/Unity/Hub/Editor/2022.3.62f3/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" -logFile /tmp/unity-compile.log
```

Prefer this (or opening the Editor) after any C# change. Play-mode behavior still needs a manual Editor run (`TemplateScene`).
