# AGENTS.md

Unity **2022.3.62f3** URP starter (C#). Verify C# with `./scripts/check-compile.sh` (Unity batchmode) or open the Editor. Style: root **`.editorconfig`** (hints only). CI: GitHub Actions under `.github/workflows/` (PR compile gate `unity-compile` + itch.io deploy + Drive retrieve/track).

## Sources of truth

- **`ARCHITECTURE.md`** — folder rules, base classes, lifecycle, naming (System vs Manager vs Controller). Prefer this over README.
- **`README.md`** — pitch + setup; trees use `_Game/` + `Systems/`; only scene is `TemplateScene.unity`.
- **`GITHUB_WORKFLOWS.md`** — secrets/vars setup, run steps, changelog for Discord bot.
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
scripts/check-compile.sh    # shared batchmode compile check
scripts/test_state.py       # Drive state regressions
retrieve.py, track.py, core/, state/, requirements.txt  # Drive → assets tooling
```

- Namespaces mirror folders: `Slafurry.Systems.*`, `Slafurry.Core.*`, `Slafurry.Utils.*`. Folder is **`Systems/`** (plural) — not `System/` (BCL conflict).
- **No `.asmdef`** — every non-`Editor` script compiles into `Assembly-CSharp`. Editor-only code must live under an `Editor/` folder.
- Internal rule: max 6 subfolders per folder (ARCHITECTURE.md).

## Boot / lifecycle (easy to get wrong)

- `Singleton`/`Manager` `Awake` is sealed and auto-calls `LoadingSystem.Instance.Register(this)`. **`LoadingSystem` must exist first** — on `AudioSystem.prefab`, the **AudioSystem** GameObject's first component after Transform (script GUID `2ba4b0833086a19a0b6ff4653a00344a`). Do not remove it.
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

## GitHub Actions — workflows use `env:` block exclusively

Both `track.yml` and `retrieve.yml` declare **all** config in a workflow-level `env:` block:

```yaml
env:
  # Non-credential (Settings → Variables)
  DRIVE_SPRITE_FOLDER_ID: ${{ vars.DRIVE_SPRITE_FOLDER_ID }}
  GEMINI_PERSONA: ${{ vars.GEMINI_PERSONA }}
  ...
  # Credentials (Settings → Secrets)
  GEMINI_API_KEY: ${{ secrets.GEMINI_API_KEY }}
  GOOGLE_SERVICE_ACCOUNT_JSON_B64: ${{ secrets.GOOGLE_SERVICE_ACCOUNT_JSON_B64 }}
  DISCORD_WEBHOOK_URL: ${{ secrets.DISCORD_WEBHOOK_URL }}
  GH_TOKEN: ${{ secrets.PR_PAT || secrets.GITHUB_TOKEN }}  # retrieve only
  UNITY_EMAIL / UNITY_PASSWORD                            # retrieve uses them too (meta import)
```

Run steps only use `"$NAME"` — **no inline `${{ secrets.* }}`**, no hardcoded values.

**Secrets** (credentials only): `GEMINI_API_KEY`, `GOOGLE_SERVICE_ACCOUNT_JSON_B64`, `DISCORD_WEBHOOK_URL`, `DISCORD_WEBHOOK_URL_DEPLOY`, `UNITY_EMAIL`, `UNITY_PASSWORD`, `BUTLER_API_KEY`, `PR_PAT` (compile, deploy, **retrieve**).

**Variables** (non-credential): `DRIVE_SPRITE_FOLDER_ID`, `DRIVE_AUDIO_FOLDER_ID`, `GEMINI_MODEL`, `GEMINI_PERSONA`, `GEMINI_LANGUAGE`, `BOT_GIT_USERNAME`, `BOT_GIT_EMAIL`, `ITCH_*`, `GAME_DISPLAY_NAME`, `UNITY_PROJECT_PATH` (deploy, retrieve).

**Retrieve runs a Unity batchmode import after downloading** so new Drive files get generated `.meta` (stable GUID + import settings) — committed inside the asset PR. Requires `UNITY_EMAIL`/`UNITY_PASSWORD` on `retrieve` now.

**Deploy notify** builds a changelog (`git log` since last tag, else `HEAD~1..HEAD`) and passes it to Gemini for the Discord announcement.

## Unity / git gotchas

- **Git LFS required** (`git lfs install` before clone). `.gitattributes` covers binaries. Working tree may show dirty `_Vendor` LFS files — **do not commit vendor churn**.
- `.gitattributes` sets `merge=unityyamlmerge` for `*.unity`/`*.asset`/etc., but the **driver is not in git config** — Unity YAML conflict merges fail until UnityYAMLMerge is configured.
- Serialization: Force Text YAML + Visible Meta Files. **Commit `.meta` with every new asset**; never hand-edit GUIDs.
- Input System only (`activeInputHandler: 2`). Actions: `Assets/_Game/05_Settings/Input/Main Input.inputactions` → generated `Main Input.cs` (regenerate in Unity after editing actions). Legacy `Input.GetKeyDown` in `DIalogHUD` is dead code.
- Editor menu: **Slafurry → Game Data** (GameAssetCreator). Types for that window use `[GameAssetCreator(category, displayName, order)]`; other SOs use `[CreateAssetMenu]`.
- `EditorBuildSettings` lists only `TemplateScene` (enabled). Do not assume other scenes are loadable.
- **Drive bots re-sync from `origin/main` every run**: `retrieve.yml` `rm -rf`s `state/`, `Assets/_Game/02_Art/Sprite`, `Assets/_Game/03_Audio` then restores via `git archive origin/main`. Commit `GameAudioMixer.mixer` (and local state) to `main` before relying on those workflows.
- **`track` / `retrieve` commit `state/` to `main`** (`[skip ci]`) so `deleted_in_drive` / `retrieve_status` / `downloaded_*` persist. Assets still go `chore/asset` → PR. `core/state.py` must keep retrieve fields when track merges a fresh listing (`scripts/test_state.py`).
- `core.hooksPath` is **not** committed (local `git config`). Each clone: `git config core.hooksPath .githooks` or pre-push compile will not run; LFS chains via `.githooks/pre-push`.

## Verify changes

- **Style only:** `.editorconfig`.
- **Compile:** `./scripts/check-compile.sh` (fails on `error CS` or non-successful exit). Bypass: `SKIP_UNITY_COMPILE=1`. Override binary: `UNITY_BIN=...`.
- **Local push gate:** `git config core.hooksPath .githooks` once per clone. `pre-push` runs **Git LFS first**, then `check-compile.sh`.
- **CI:** `unity-compile.yml` runs same check on **PRs only** (paths: `Assets/`, `Packages/`, `ProjectSettings/`). Require in branch protection.
- **Drive tooling:** `python3 scripts/test_state.py` after touching `core/state.py` / `track.py` / `retrieve.py`.

```bash
./scripts/check-compile.sh
# or
~/Unity/Hub/Editor/2022.3.62f3/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" -logFile /tmp/unity-compile.log
```

Prefer this (or opening the Editor) after any C# change. Play-mode behavior still needs a manual Editor run (`TemplateScene`).