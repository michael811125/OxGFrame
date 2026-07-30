# OxGFrame Coding Style

> **Ask before applying — coding style is a per-team decision.**
>
> This guide is the standard for code inside the OxGFrame package itself (framework contributions, extensions, and samples). It is NOT automatically the standard for a project's application/business-logic code — every team defines its own conventions.
>
> Before writing the first code for a project, ask the developer:
> "Should project code follow the OxGFrame coding style, or your team's own conventions?"
> Then apply the answer consistently for the rest of the session (persist it to project memory/context when available).
>
> - Developer chooses **team conventions** (the default assumption): use this document only when touching OxGFrame framework code; everywhere else follow the project's existing style — when in doubt, match the surrounding code.
> - Developer chooses **OxGFrame style**: apply the rules below to project code as well.

The rules below reproduce the official coding-style wiki (`https://github.com/michael811125/OxGFrame/wiki/Coding-Style`).

## Naming

| Element | Convention | Example |
|---|---|---|
| public / internal fields | lowerCamelCase | `assetName` |
| protected / private fields | `_` + lowerCamelCase | `_audioSource` |
| public..protected methods | UpperCamelCase | `PlayPreview()` |
| private methods | `_` + UpperCamelCase | `_PlayPreview()` |
| classes / structs | UpperCamelCase | `AudioBase` |
| interfaces | `I` + UpperCamelCase | `INetTips` |
| public / internal consts | ALL_UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |
| protected / private consts | `_` + ALL_UPPER_SNAKE_CASE | `_MAX_PREPARE_TIME_SECONDS` |
| enum type + options | UpperCamelCase | `SoundType.SoundEffect` |
| status / step enum values | ALL_UPPER_SNAKE_CASE | `PATCH_PREPARE` |

Always use `this.` when accessing instance members. Address-constant classes may mirror asset file names verbatim (e.g. `menu_bgm_01`) — an intentional exception.

## Class organization

Order: consts → serialized/public fields → private fields → generated `#region Binding Components` (for frames) → lifecycle overrides in call order → public methods → private methods. Mark leaf UI/SR/CP/stage classes `sealed` when inheritance is not intended.

## Async (UniTask)

- Framework lifecycle methods that are async return `UniTask` (`async override UniTask OnEnter()`).
- Fire-and-forget: `CoreFrames.UIFrame.Show(...).Forget();` — never ignore a UniTask silently.
- Long-running loops own a `CancellationTokenSource`; cancel in `OnPreClose`/`OnRelease`/`OnExit`.
- `async void` only for Unity/editor callbacks that cannot return UniTask.

## Lifecycle responsibilities (UI/SR/CP)

- `OnAutoBind`: collector reference binding only (generated code).
- `OnBind`: one-time event registration (button clicks, input events).
- `OnPreShow`: async preparation before display.
- `OnShow(object)`: consume `Show(..., data)` payloads; start view behavior; must be repeat-safe.
- `OnReceiveAndRefresh(object)`: redraw data/localized text; repeat-safe; no event registration.
- `OnPreClose`/`OnClose`: stop view behavior.
- `OnRelease`: remove listeners, cancel tokens, release owned resources.

## Comments

- Comments explain non-obvious behavior or constraints only — not what the next line does.
- XML doc comments (`/// <summary>`) on framework-facing members.
- The framework's own comments are Traditional Chinese with full-width punctuation (，。：) but half-width parentheses `(...)`; match the target project's comment language for project code.

## Practical rules

- Prefer OxGFrame wrappers over lower-level providers (no direct YooAsset in gameplay code).
- Prefer address constants over string literals; be explicit with `packageName` in multi-package projects.
- Guard Unity-object access after async awaits (`if (this == null) return;` in editor/tool code; check destroyed state in runtime code).
- Graceful degradation over exceptions in framework/editor extension code: null-safe reflection, fallbacks, no hard dependencies on editor-only APIs at runtime.
- Compatibility target: Unity 2021.3 through Unity 6.x, Windows and macOS editors; wrap newer editor APIs with version defines (`#if UNITY_2022_1_OR_NEWER`).
